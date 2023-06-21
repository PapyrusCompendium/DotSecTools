namespace DotWindowsEnum.LdapTypes {
    public static class LdapCapabilities {
        public static Dictionary<string, CapabilityType> Capabilities = new() {
            { "1.2.840.113556.1.4.800", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_OID },
            { "1.2.840.113556.1.4.1791", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_LDAP_INTEG_OID },
            { "1.2.840.113556.1.4.1670", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_V51_OID },
            { "1.2.840.113556.1.4.1880", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_ADAM_DIGEST_OID },
            { "1.2.840.113556.1.4.1851", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_ADAM_OID },
            { "1.2.840.113556.1.4.1920", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_PARTIAL_SECRETS_OID },
            { "1.2.840.113556.1.4.1935", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_V60_OID },
            { "1.2.840.113556.1.4.2080", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_V61_R2_OID },
            { "1.2.840.113556.1.4.2237", CapabilityType.LDAP_CAP_ACTIVE_DIRECTORY_W8_OID }
        };
    }

    public enum CapabilityType {
        LDAP_CAP_ACTIVE_DIRECTORY_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_LDAP_INTEG_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_V51_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_ADAM_DIGEST_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_ADAM_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_PARTIAL_SECRETS_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_V60_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_V61_R2_OID,
        LDAP_CAP_ACTIVE_DIRECTORY_W8_OID
    }
}
