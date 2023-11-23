using System.ComponentModel;

using Spectre.Console.Cli;

namespace DotGpt.Commands.Settings {
    public class ChatCommandSettings : CommandSettings {
        [CommandOption("--system")]
        [Description("The system prompt that pre-faces the chat history")]
        [DefaultValue("You are a chat bot inside of a CLI terminal that does NOT support markdown, helping security professionals evaluate and solve CTF challanges more quickly.")]
        public string SystemPrompt { get; set; }

        [CommandOption("-h|--history")]
        [Description("The maximum number of historic messages to keep during chats. (This can increase API usage)")]
        [DefaultValue(15)]
        public int MaxHistory { get; set; }

        [CommandOption("-p|--prompt")]
        [Description("The prompt to bein your chat with. (Piped data is interpolated with argument value, {Prompt}\\n{PipedData})")]
        [DefaultValue("Hello")]
        public string Prompt { get; set; }

        [CommandOption("-s|--single")]
        [Description("Single shot prompt flag. Will close after the first respone.")]
        public bool SingleShot { get; set; }

        [CommandOption("-c|--command")]
        [Description("Quickly generate a single command.")]
        public bool Command { get; set; }

        [CommandOption("-e|--execute")]
        [Description("Execute the command output.")]
        public bool Execute { get; set; }

        [CommandOption("-r|--recommended")]
        [Description("Recommend a strategy to attain privileges.")]
        public bool Recommend { get; set; }

        [CommandOption("-t|--tldr")]
        [Description("Keep the response concise and short.")]
        public bool ShortResponse { get; set; }
    }
}
