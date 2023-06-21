
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

            AnsiConsole.Status().Start("Querying Ldap Service", ctx => {
                AnsiConsole.Cursor.Show();
                ctx.SpinnerStyle(Style.Parse("green"));
                ctx.Spinner(Spinner.Known.Dots3);

                AppendAuthenticationInfo(settings, rootNode);
                if (!_ldapEnumerationService.AnonymousInformationLeaking(settings.ServerIp!, settings.LdapPort ?? 0)) {
                    rootNode.AddNode($"[{DEEP_PINK}]Cannot preform anonymous query![/]");
                    AnsiConsole.Write(rootNode);
                    return;
                }

                AppendConnectionInfo(settings, rootNode);
                EnumerateUsers(settings, rootNode);
                EnumerateMachines(settings, rootNode);
                AppendDseInfo(settings, rootNode);

                AnsiConsole.Write(rootNode);
            });
            return 1;
        }

        // TODO: Clean this up, (DRY)
        private void EnumerateMachines(LdapScanSettings settings, Tree rootNode) {
            var machinesNode = rootNode.AddNode($"[gold3_1]Domain Machines[/]");
            var allDomainMachines = _ldapEnumerationService.GetAllDomainMachines(settings.ServerIp!, settings.LdapPort ?? 0);
            foreach (var machine in allDomainMachines) {
                var machineNode = machinesNode.AddNode($"[red]{machine.Dn}[/]");
                var attributesProperty = typeof(LdapEntry).GetProperty("Attrs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
                var attributes = (LdapAttributeSet)attributesProperty!.GetValue(machine)!;

                if (settings.Verbose ?? false) {
                    foreach (var attribute in attributes) {
                        machineNode.AddNode($"{attribute.Name}")
                            .AddNode(new Text(string.Join(" ", attribute.StringValueArray)));
                    }

                    continue;
                }


                //dNSHostName
                var distinguishedName = attributes["distinguishedName"];
                machineNode.AddNode($"Distinguished Name")
                    .AddNode($"[steelblue1_1]{string.Join(" ", distinguishedName.StringValueArray)}[/]");

                var commonName = attributes["cn"];
                machineNode.AddNode($"Common Name")
                    .AddNode($"[steelblue1_1]{string.Join(" ", commonName.StringValueArray)}[/]");

                if (attributes.ContainsKey("description")) {
                    var description = attributes["description"];
                    machineNode.AddNode($"Description")
                        .AddNode($"[steelblue1_1]{string.Join(" ", description.StringValueArray)}[/]");
                }

                if (attributes.ContainsKey("memberOf")) {
                    var memberOf = attributes["memberOf"];
                    machineNode.AddNode($"Groups")
                        .AddNode($"[steelblue1_1]{string.Join(" ", memberOf.StringValueArray)}[/]");
                }

                var objectGUID = attributes["objectGUID"];
                machineNode.AddNode($"Guid")
                    .AddNode($"[steelblue1_1]{new Guid(objectGUID.ByteValue)}[/]");

                var whenCreated = ConvertWindowsStringToDate(attributes["whenCreated"].StringValue);
                machineNode.AddNode($"Created At")
                    .AddNode($"[steelblue1_1]{whenCreated}[/]");

                var whenChanged = ConvertWindowsStringToDate(attributes["whenChanged"].StringValue);
                machineNode.AddNode($"Last Changed")
                    .AddNode($"[steelblue1_1]{whenChanged}[/]");
            }
        }

        private void EnumerateUsers(LdapScanSettings settings, Tree rootNode) {
            var accountsNode = rootNode.AddNode($"[gold3_1]User Accounts[/]");
            var allUserAccounts = _ldapEnumerationService.GetAllUserAccounts(settings.ServerIp!, settings.LdapPort ?? 0);

            foreach (var userAccount in allUserAccounts) {
                var userNode = accountsNode.AddNode($"[red]{userAccount.Dn}[/]");

                var attributesProperty = typeof(LdapEntry).GetProperty("Attrs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
                var attributes = (LdapAttributeSet)attributesProperty!.GetValue(userAccount)!;

                if (settings.Verbose ?? false) {
                    foreach (var attribute in attributes) {
                        userNode.AddNode($"{attribute.Name}")
                            .AddNode(new Text(string.Join(" ", attribute.StringValueArray)));
                    }

                    continue;
                }


                var distinguishedName = attributes["distinguishedName"];
                userNode.AddNode($"Distinguished Name")
                    .AddNode($"[steelblue1_1]{string.Join(" ", distinguishedName.StringValueArray)}[/]");

                var commonName = attributes["cn"];
                userNode.AddNode($"Common Name")
                    .AddNode($"[steelblue1_1]{string.Join(" ", commonName.StringValueArray)}[/]");

                if (attributes.ContainsKey("description")) {
                    var description = attributes["description"];
                    userNode.AddNode($"Description")
                        .AddNode($"[steelblue1_1]{string.Join(" ", description.StringValueArray)}[/]");
                }

                if (attributes.ContainsKey("memberOf")) {
                    var memberOf = attributes["memberOf"];
                    userNode.AddNode($"Groups")
                        .AddNode($"[steelblue1_1]{string.Join(" ", memberOf.StringValueArray)}[/]");
                }

                var objectGUID = attributes["objectGUID"];
                userNode.AddNode($"Guid")
                    .AddNode($"[steelblue1_1]{new Guid(objectGUID.ByteValue)}[/]");

                var objectSid = attributes["objectSid"];
                userNode.AddNode($"Sid")
                    .AddNode($"[steelblue1_1]{Convert.ToHexString(objectSid.ByteValue)}[/]");

                var objectClass = attributes["objectClass"];
                userNode.AddNode($"Object Class")
                    .AddNode($"[steelblue1_1]{string.Join(" ", objectClass.StringValueArray)}[/]");

                var whenCreated = ConvertWindowsStringToDate(attributes["whenCreated"].StringValue);
                userNode.AddNode($"Created At")
                    .AddNode($"[steelblue1_1]{whenCreated}[/]");

                var whenChanged = ConvertWindowsStringToDate(attributes["whenChanged"].StringValue);
                userNode.AddNode($"Last Changed")
                    .AddNode($"[steelblue1_1]{whenChanged}[/]");

                var pwdLastSet = ConvertWindowsStringToDate(attributes["pwdLastSet"].StringValue);
                userNode.AddNode($"Last Password Reset")
                    .AddNode($"[steelblue1_1]{pwdLastSet}[/]");
            }
        }

        private DateTime ConvertWindowsStringToDate(string windowsDate) {
            if (windowsDate.Length < 14) {
                return DateTime.MinValue;
            }

            int.TryParse(windowsDate.Substring(0, 4), out var year);
            int.TryParse(windowsDate.Substring(4, 2), out var month);
            int.TryParse(windowsDate.Substring(6, 2), out var day);
            int.TryParse(windowsDate.Substring(8, 2), out var hour);
            int.TryParse(windowsDate.Substring(10, 2), out var minute);
            int.TryParse(windowsDate.Substring(12, 2), out var second);

            if (year != 0 && month != 0 && day != 0) {
                try {
                    return new DateTime(year, month, day, hour, minute, second);
                }
                catch {
                    return DateTime.MinValue;
                }
            }

            return DateTime.MinValue;
        }

        private void AppendAuthenticationInfo(LdapScanSettings settings, Tree rootNode) {
            var nullCreds = _ldapEnumerationService.SupportsNullCredentials(settings.ServerIp!, settings.LdapPort ?? 0);
            var valueColor = nullCreds ? LIGHT_GREEN : DEEP_PINK;
            var authNode = rootNode.AddNode($"Authentication");
            authNode.AddNode($"Null Binding Credentials: [{valueColor}]{nullCreds}[/]");

            if (!string.IsNullOrWhiteSpace(settings.Username) || !string.IsNullOrWhiteSpace(settings.Password)) {
                var validCredentials = _ldapEnumerationService
                    .ValidCredentials(settings.ServerIp!, settings.LdapPort ?? 0, settings.Username!, settings.Password!);

                valueColor = validCredentials ? LIGHT_GREEN : DEEP_PINK;
                authNode.AddNode($"User Credentials: [{valueColor}]{validCredentials}[/]");
            }
        }

        private void AppendConnectionInfo(LdapScanSettings settings, Tree rootNode) {
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
