
using System.Diagnostics.CodeAnalysis;

using DotWindowsEnum.Commands.Settings;
using DotWindowsEnum.LdapTypes;
using DotWindowsEnum.Services;

using Novell.Directory.Ldap;

using Spectre.Console;
using Spectre.Console.Cli;

namespace DotWindowsEnum.Commands {
    public class LdapScanCommand : Command<LdapScanSettings> {
        private const string DEEP_PINK = "deeppink4_2";
        private const string LIGHT_GREEN = "chartreuse1";
        private readonly ILdapEnumerationService _ldapEnumerationService;

        public LdapScanCommand(ILdapEnumerationService ldapEnumerationService) {
            _ldapEnumerationService = ldapEnumerationService;
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] LdapScanSettings settings) {
            var rootNode = new Tree($"[orange3]Ldap Root Info ({settings.ServerIp})[/]");

            AppendAuthenticationInfo(settings, rootNode);
            AppendConnectionInfo(settings, rootNode, LIGHT_GREEN);
            AppendDseInfo(settings, rootNode);

            AnsiConsole.Write(rootNode);
            return 1;
        }

        private void AppendAuthenticationInfo(LdapScanSettings settings, Tree rootNode) {
            var nullCreds = _ldapEnumerationService.SupportsNullCredentials(settings.ServerIp!, settings.LdapPort ?? 0);
            var valueColor = nullCreds ? LIGHT_GREEN : DEEP_PINK;
            var authNode = rootNode.AddNode($"Authentication");
            authNode.AddNode($"Null Credentials: [{valueColor}]{nullCreds}[/]");

            if (!string.IsNullOrWhiteSpace(settings.Username) || !string.IsNullOrWhiteSpace(settings.Password)) {
                var validCredentials = _ldapEnumerationService
                    .ValidCredentials(settings.ServerIp!, settings.LdapPort ?? 0, settings.Username!, settings.Password!);

                valueColor = validCredentials ? LIGHT_GREEN : DEEP_PINK;
                authNode.AddNode($"User Credentials: [{valueColor}]{validCredentials}[/]");
            }
        }

        private void AppendConnectionInfo(LdapScanSettings settings, Tree rootNode, string LIGHT_GREEN) {
            var connectionOptionsNode = rootNode.AddNode("Connection Options");
            var validConnectionOptions = _ldapEnumerationService.GetValidConnectionOptions(settings.ServerIp!, settings.LdapPort ?? 0);
            var connection = new LdapConnection(validConnectionOptions);
            connection.Connect(settings.ServerIp!, settings.LdapPort ?? 0);

            var connectedColor = connection.Connected ? LIGHT_GREEN : DEEP_PINK;
            connectionOptionsNode.AddNode($"Connected: [{connectedColor}]{connection.Connected}[/]");
            if (!connection.Connected) {
                return;
            }

            var valueColor = validConnectionOptions.Ssl ? LdapScanCommand.LIGHT_GREEN : DEEP_PINK;
            connectionOptionsNode.AddNode($"Ssl: [{valueColor}]{validConnectionOptions.Ssl}[/]");
            connectionOptionsNode.AddNode($"Tls: [{valueColor}]{connection.Tls}[/]");

            connectionOptionsNode.AddNode($"Dn Schema: [chartreuse1]{connection.GetSchemaDn()}[/]");

            var currentUserId = connection.WhoAmI().AuthzId;
            var whoNode = connectionOptionsNode.AddNode($"WhoAmI: [chartreuse1]{currentUserId}[/]");
            whoNode.AddNode($"ReferralFollowing: [chartreuse1]{connection.Constraints.ReferralFollowing}[/]");
            whoNode.AddNode($"MaxReferrals: [chartreuse1]{connection.Constraints.HopLimit}[/]");
        }

        private void AppendDseInfo(LdapScanSettings settings, Tree rootNode) {
            var dseInfo = _ldapEnumerationService.GetRootInfo(settings.ServerIp!, settings.LdapPort ?? 0);
            var dseNode = rootNode.AddNode($"Root Dse Information");
            dseNode.AddNode($"ServerName: [steelblue1_1]{dseInfo.ServerName}[/]");

            var subRootNode = dseNode.AddNode($"OtherAttributes");
            foreach (var attribute in dseInfo.OtherAttributes) {
                var attributeNode = subRootNode.AddNode($"[thistle1]{attribute.Key}[/]");
                foreach (var subAttributeList in attribute.Value) {
                    attributeNode.AddNode($"[steelblue1_1]{subAttributeList}[/]");
                }
            }

            dseNode.AddNode($"DefaultNamingContext: [steelblue1_1]{dseInfo.DefaultNamingContext}[/]");
            subRootNode = dseNode.AddNode($"NamingContexts");
            foreach (var namingContext in dseInfo.NamingContexts) {
                subRootNode.AddNode($"[steelblue1_1]{namingContext}[/]");
            }

            subRootNode = dseNode.AddNode($"SupportedControls");
            foreach (var control in dseInfo.SupportedControls) {
                var controlName = control;
                if (LdapExtendedControls.ExtendedControls.TryGetValue(controlName, out var capabilityType)) {
                    controlName = capabilityType.ToString();
                }

                subRootNode.AddNode($"[steelblue1_1]{controlName}[/]");
            }

            subRootNode = dseNode.AddNode($"SupportedCapabilities");
            foreach (var capability in dseInfo.SupportedCapabilities) {
                var capabilityName = capability;
                if (LdapCapabilities.Capabilities.TryGetValue(capability, out var capabilityType)) {
                    capabilityName = capabilityType.ToString();
                }
                subRootNode.AddNode($"[steelblue1_1]{capabilityName}[/]");
            }

            subRootNode = dseNode.AddNode($"SupportedExtensions");
            foreach (var extention in dseInfo.SupportedExtensions) {
                var extentionName = extention;
                if (LdapExtendedOperations.Extentions.TryGetValue(extention, out var capabilityType)) {
                    extentionName = capabilityType.ToString();
                }
                subRootNode.AddNode($"[steelblue1_1]{extentionName}[/]");
            }

            subRootNode = dseNode.AddNode($"SupportedLDAPPolicies");
            foreach (var ldapPolicy in dseInfo.SupportedLDAPPolicies) {
                subRootNode.AddNode($"[steelblue1_1]{ldapPolicy}[/]");
            }

            subRootNode = dseNode.AddNode($"SupportedSaslMechanisms");
            foreach (var saslMech in dseInfo.SupportedSaslMechanisms) {
                subRootNode.AddNode($"[steelblue1_1]{saslMech}[/]");
            }
        }
    }
}
