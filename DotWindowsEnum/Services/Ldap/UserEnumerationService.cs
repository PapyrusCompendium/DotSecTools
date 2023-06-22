using DotWindowsEnum.Commands.Settings;

using Novell.Directory.Ldap;

using Spectre.Console;

namespace DotWindowsEnum.Services.Ldap {
    public class UserEnumerationService : IUserEnumerationService {
        private readonly ILdapEnumerationService _ldapEnumerationService;
        private readonly ILdapService _ldapService;

        public UserEnumerationService(ILdapEnumerationService ldapEnumerationService, ILdapService ldapService) {
            _ldapEnumerationService = ldapEnumerationService;
            _ldapService = ldapService;
        }

        public void EnumerateUsers(LdapScanSettings settings, Tree rootNode) {
            var accountsNode = rootNode.AddNode($"[{LdapUtilities.GOLD}]User Accounts[/]");
            var allUserAccounts = _ldapService.GetAllUserAccounts(settings.ServerIp!, settings.LdapPort ?? 0);

            foreach (var userAccount in allUserAccounts) {
                var userNode = accountsNode.AddNode($"[red]{userAccount.Dn}[/]");

                var attributesProperty = typeof(LdapEntry).GetProperty("Attrs",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
                var attributes = (LdapAttributeSet)attributesProperty!.GetValue(userAccount)!;

                if (settings.Verbose ?? false) {
                    foreach (var attribute in attributes) {
                        userNode.AddNode($"{attribute.Name}")
                            .AddNode(new Text(string.Join(" ", attribute.StringValueArray)));
                    }

                    continue;
                }

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Distinguished Name", "distinguishedName", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Common Name", "cn", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Description", "description", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Groups", "memberOf", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Guid", "objectGUID", (attribute)
                    => new Guid(attribute.ByteValue).ToString());

                LdapUtilities.PrintKeyOutput(userNode, attributes, "SID", "objectSid", (attribute)
                    => Convert.ToHexString(attribute.ByteValue));

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Object Class", "objectClass", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Created At", "whenCreated", (attribute)
                    => LdapUtilities.ConvertWindowsStringToDate(attribute.StringValue).ToString());

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Last Changed", "whenChanged", (attribute)
                    => LdapUtilities.ConvertWindowsStringToDate(attribute.StringValue).ToString());

                LdapUtilities.PrintKeyOutput(userNode, attributes, "Last Password Reset", "pwdLastSet", (attribute)
                    => LdapUtilities.ConvertWindowsStringToDate(attribute.StringValue).ToString());
            }
        }
    }
}
