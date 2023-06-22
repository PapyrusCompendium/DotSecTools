using DotPortScan.Commands;
using DotPortScan.Services;

using Microsoft.Extensions.DependencyInjection;

using Shared;

using Spectre.Console.Cli;

namespace DotPortScan {
    internal class Program {
        static void Main(string[] args) {
            var registrar = ConfigureServiceRegistry();
            var app = new CommandApp<PortScanCommand>(registrar);
            app.Run(args);
        }

        private static void ConfigureServices(IServiceCollection serviceDescriptors) {
            serviceDescriptors
                .AddSingleton<IScanningService, ScanningService>();
        }

        private static TypeRegistrar ConfigureServiceRegistry() {
            var registrations = new ServiceCollection();
            ConfigureServices(registrations);
            var registrar = new TypeRegistrar(registrations);
            return registrar;
        }
    }
}