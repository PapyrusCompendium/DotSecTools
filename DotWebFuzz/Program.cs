using DotWebFuzz.Commands;

using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;

using Shared;

using Spectre.Console.Cli;

namespace DotWebFuzz {
    public class Program {
        static void Main(string[] args) {
            var registrar = ConfigureServiceRegistry();
            var app = new CommandApp<WebScanCommand>(registrar);
            app.Run(args);
        }

        private static void ConfigureServices(IServiceCollection serviceDescriptors) {
            serviceDescriptors
                .AddSingleton<IWebScanningService, WebScanningService>();

            serviceDescriptors.AddHttpClient<IWebScanningService, WebScanningService>()
                .ConfigureHttpMessageHandlerBuilder(builder => {
                    builder.PrimaryHandler = new HttpClientHandler {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                });
        }

        private static TypeRegistrar ConfigureServiceRegistry() {
            var registrations = new ServiceCollection();
            ConfigureServices(registrations);
            var registrar = new TypeRegistrar(registrations);
            return registrar;
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}