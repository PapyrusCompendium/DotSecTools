using DotLEnum.Commands;

using Microsoft.Extensions.DependencyInjection;

using Shared;

using Spectre.Console.Cli;

namespace DotLEnum {
    public class Program {
        static void Main(string[] args) {
            var registrar = ConfigureServiceRegistry();
            var app = new CommandApp<SystemEnumerate>(registrar);
            app.Run(args);
        }

        private static void ConfigureServices(IServiceCollection serviceDescriptors) {

        }

        private static TypeRegistrar ConfigureServiceRegistry() {
            var registrations = new ServiceCollection();
            ConfigureServices(registrations);
            var registrar = new TypeRegistrar(registrations);
            return registrar;
        }
    }
}