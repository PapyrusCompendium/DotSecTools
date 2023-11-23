using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using DotGpt.Commands.Settings;
using DotGpt.Config;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotGpt.Commands {
    public class ChatCommand : Command<ChatCommandSettings> {
        private const string KEEP_SHORT_SYSTEM = "Use concise wording. Keep your responses concise and to the point, ensuring they are informative yet brief. Avoid unnecessary details to maintain clarity and brevity.";
        private const string BOT_SHORT_CORRESPONDENCE = "Understood, I'll keep my responses concise and focused on providing clear and informative answers. If you have any specific questions or need information, feel free to ask!";
        private readonly PipedData _pipedData;
        private readonly IKernel _kernel;
        private readonly ISemanticTextMemory _semanticTextMemory;
        private readonly IChatCompletion _chatCompletion;
        private readonly ISKFunction _keepShortSummary;
        private readonly ISKFunction _commandGenerator;
        private readonly ISKFunction _analysisRecommendation;


        private readonly ChatHistory _chatMessages;

        public ChatCommand(PipedData pipedData, IKernel kernel, ISemanticTextMemory semanticTextMemory, IChatCompletion chatCompletion) {
            _pipedData = pipedData;
            _kernel = kernel;
            _semanticTextMemory = semanticTextMemory;
            _chatCompletion = chatCompletion;

            _chatMessages = new();

            _keepShortSummary = kernel.CreateSemanticFunction("""
Use concise wording!
Keep your summary concise and to the point, ensuring you are informative yet brief.
Avoid unnecessary details to maintain clarity and brevity.

Response To Summarize:
{{$input}}
""");

            _commandGenerator = _kernel.Functions.GetFunction("SemanticPrompts", "CreateCommand");
            _analysisRecommendation = _kernel.Functions.GetFunction("SemanticPrompts", "AnalysisRecommendation");
        }

        public override int Execute(CommandContext context, ChatCommandSettings settings) {
            if (!string.IsNullOrWhiteSpace(_pipedData.Piped)) {
                if (!string.IsNullOrWhiteSpace(settings.Prompt)) {
                    settings.Prompt = $"{settings.Prompt}\n{_pipedData.Piped}";
                    settings.SingleShot = true;
                }
                else {
                    settings.Prompt = _pipedData.Piped;
                }
            }

            if (settings.Recommend) {
                RecommendPlan(settings);
                return 1;
            }

            if (settings.Command) {
                GenerateCommand(settings);
                return 1;
            }

            UserllmChat(settings);
            return 1;
        }

        private void RecommendPlan(ChatCommandSettings settings) {
            Task.Run(async () => {
                var semanticContext = _kernel.CreateNewContext(new ContextVariables() {
                    ["input"] = settings.Prompt
                });

                var recommendationResponse = await _analysisRecommendation.InvokeAsync(semanticContext);
                if (settings.ShortResponse) {
                    await _keepShortSummary.InvokeAsync(semanticContext);
                }

                var recommendation = semanticContext.Variables["input"];

                AnsiConsole.Write(recommendation);
            }).Wait();
        }

        private void GenerateCommand(ChatCommandSettings settings) {
            var systemInfo = new StringBuilder();
            systemInfo.AppendLine($"OS: {RuntimeInformation.OSDescription}");
            systemInfo.AppendLine($"Cpu Architecture: {RuntimeInformation.ProcessArchitecture.ToString()}");

            Task.Run(async () => {
                var semanticContext = _kernel.CreateNewContext(new ContextVariables() {
                    ["input"] = settings.Prompt,
                    ["system"] = systemInfo.ToString()
                });

                var commandResponse = await _commandGenerator.InvokeAsync(semanticContext);
                var fullCommand = commandResponse.GetValue<string>();

                if (settings.Execute) {
                    var execute = AnsiConsole.Prompt(new TextPrompt<bool>($"Would you like to Execute\n{fullCommand}\n(true/false): "));
                    if (execute) {
                        using var process = ExecuteCommand(fullCommand);
                    }
                }

                AnsiConsole.Write(fullCommand);
            }).Wait();
        }

        private static Process ExecuteCommand(string fullCommand) {
            var firstSpaceIndex = fullCommand.IndexOf(' ');
            var command = firstSpaceIndex >= 0 ? fullCommand.Substring(0, firstSpaceIndex) : fullCommand;
            var arguments = firstSpaceIndex >= 0 ? fullCommand.Substring(firstSpaceIndex + 1) : string.Empty;

            var startInfo = new ProcessStartInfo {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();

            var result = process.StandardOutput.ReadToEnd();
            Console.WriteLine(result);
            return process;
        }

        private void UserllmChat(ChatCommandSettings settings) {
            if (settings.ShortResponse) {
                _chatMessages.AddUserMessage(KEEP_SHORT_SYSTEM);
                _chatMessages.AddAssistantMessage(BOT_SHORT_CORRESPONDENCE);
            }

            _chatMessages.AddUserMessage(settings.Prompt);

            do {
                var chatStream = _chatCompletion.GenerateMessageStreamAsync(_chatMessages);
                Task.Run(async () => {
                    await foreach (var token in chatStream) {
                        AnsiConsole.Write(token);
                    }

                    AnsiConsole.WriteLine();
                }).Wait();

                if (!settings.SingleShot) {
                    var userMessage = AnsiConsole.Prompt(new TextPrompt<string>("User: "));
                    _chatMessages.AddUserMessage(userMessage);
                }
            }
            while (!settings.SingleShot);
        }
    }
}
