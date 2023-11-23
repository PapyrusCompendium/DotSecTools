using System.Reflection;
using System.Runtime.CompilerServices;

using DotGpt.Commands;
using DotGpt.Config;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.Memory.Sqlite;
using Microsoft.SemanticKernel.Plugins.Memory;

using Shared;

using Spectre.Console.Cli;

namespace DotGpt {
    internal class Program {
        static async Task Main(string[] args) {
            var registrar = await ConfigureServiceRegistry();
            var app = new CommandApp<ChatCommand>(registrar);
            app.Run(args);
        }

        private static async Task ConfigureServices(IServiceCollection services) {
            var resourceDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
            var settingsPath = Path.Combine(resourceDirectory, "AppSettings.json");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsPath, false)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables()
                .Build();

            var openAiConfig = new OpenAiConfig(configuration);
            var kernel = new KernelBuilder()
                .WithOpenAIChatCompletionService(
                    openAiConfig.ModelId,
                    openAiConfig.ApiKey,
                    openAiConfig.OrganizationId)
                .Build();


            var sqliteDb = await SqliteMemoryStore.ConnectAsync(Path.Combine(resourceDirectory, "gptMemory.sqlite"));
            var semanticMemory = new MemoryBuilder()
                .WithOpenAITextEmbeddingGenerationService(
                    openAiConfig.EmbeddingModelId,
                    openAiConfig.ApiKey,
                    openAiConfig.OrganizationId)
                .WithMemoryStore(sqliteDb)
                .Build();
            services.AddSingleton(semanticMemory);
            kernel.ImportFunctions(new TextMemoryPlugin(semanticMemory));

            kernel.ImportSemanticFunctionsFromDirectory(resourceDirectory, "SemanticPrompts");

            services.AddScoped(_ => kernel.GetService<IChatCompletion>());
            services.AddScoped(sp => sp.GetRequiredService<IChatCompletion>().CreateNewChat());

            var pipedData = GetPipedData();
            services
                .AddSingleton(kernel)
                .AddSingleton(pipedData);
        }

        private static PipedData GetPipedData() {
            var pipedData = new PipedData() {
                Piped = string.Empty
            };

            if (Console.IsInputRedirected) {
                pipedData.Piped = Console.In.ReadToEnd();
            }

            return pipedData;
        }

        private static async Task<TypeRegistrar> ConfigureServiceRegistry() {
            var registrations = new ServiceCollection();
            await ConfigureServices(registrations);
            var registrar = new TypeRegistrar(registrations);
            return registrar;
        }
    }
}