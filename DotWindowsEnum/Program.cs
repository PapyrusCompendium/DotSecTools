using DotWindowsEnum.Commands;
using DotWindowsEnum.Services;

using Microsoft.Extensions.DependencyInjection;

using Shared;

using Spectre.Console.Cli;

namespace DotWindowsEnum {
    public class Program {
        static void Main(string[] args) {
            var registrar = ConfigureServiceRegistry();
            var app = new CommandApp(registrar);
            app.Configure(ConfigureCommands);
            app.Run(args);
        }

        private static void ConfigureCommands(IConfigurator configurator) {
            configurator.AddCommand<LdapScanCommand>("ldap")
                .WithDescription("Enumerate the Ldap service on a specified address and port.")
                .WithExample("ldap", "-i", "127.0.0.1", "-p", "636");

            configurator.AddCommand<SmbScanCommand>("smb")
                .WithDescription("Enumerate the Smb service on a specified address and port.")
                .WithExample("smb", "-i", "127.0.0.1", "-p", "636");
        }

        private static void ConfigureServices(IServiceCollection serviceDescriptors) {
            serviceDescriptors
                .AddSingleton<ILdapEnumerationService, LdapEnumerationService>()
                .AddSingleton<ISmbEnumerationService, SmbEnumerationService>();
        }

        private static TypeRegistrar ConfigureServiceRegistry() {
            var registrations = new ServiceCollection();
            ConfigureServices(registrations);
            var registrar = new TypeRegistrar(registrations);
            return registrar;
        }
    }
}