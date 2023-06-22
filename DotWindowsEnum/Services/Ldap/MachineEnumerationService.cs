using DotWindowsEnum.Commands.Settings;

using Novell.Directory.Ldap;

using Spectre.Console;

namespace DotWindowsEnum.Services.Ldap {
    public class MachineEnumerationService : IMachineEnumerationService {
        private readonly ILdapEnumerationService _ldapEnumerationService;
        private readonly ILdapService _ldapService;

        public MachineEnumerationService(ILdapEnumerationService ldapEnumerationService, ILdapService ldapService) {
            _ldapEnumerationService = ldapEnumerationService;
            _ldapService = ldapService;
        }

        public void EnumerateMachines(LdapScanSettings settings, Tree rootNode) {
            var machinesNode = rootNode.AddNode($"[gold3_1]Domain Machines[/]");
            var allDomainMachines = _ldapService.GetAllDomainMachines(settings.ServerIp!, settings.LdapPort ?? 0);

            foreach (var machine in allDomainMachines) {
                var machineNode = machinesNode.AddNode($"[red]{machine.Dn}[/]");
                var attributesProperty = typeof(LdapEntry).GetProperty("Attrs",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
                var attributes = (LdapAttributeSet)attributesProperty!.GetValue(machine)!;

                if (settings.Verbose ?? false) {
                    foreach (var attribute in attributes) {
                        machineNode.AddNode($"{attribute.Name}")
                            .AddNode(new Text(string.Join(" ", attribute.StringValueArray)));
                    }

                    continue;
                }

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Distinguished Name", "distinguishedName", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Common Name", "cn", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Host name", "dNSHostName", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Description", "description", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Groups", "memberOf", (attribute)
                    => string.Join(" ", attribute.StringValueArray));

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Guid", "objectGUID", (attribute)
                    => new Guid(attribute.ByteValue).ToString());

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Created At", "whenCreated", (attribute)
                    => LdapUtilities.ConvertWindowsStringToDate(attribute.StringValue).ToString());

                LdapUtilities.PrintKeyOutput(machineNode, attributes, "Last Changed", "whenChanged", (attribute)
                    => LdapUtilities.ConvertWindowsStringToDate(attribute.StringValue).ToString());
            }
        }
    }
}
