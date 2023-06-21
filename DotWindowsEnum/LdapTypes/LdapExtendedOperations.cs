namespace DotWindowsEnum.LdapTypes {
    public static class LdapExtendedOperations {
        public static Dictionary<string, ExtendedOperationType> Extentions = new() {
            { "1.2.840.113556.1.4.1781", ExtendedOperationType.LDAP_SERVER_FAST_BIND_OID },
            { "1.3.6.1.4.1.1466.20037", ExtendedOperationType.LDAP_SERVER_START_TLS_OID },
            { "1.3.6.1.4.1.1466.101.119.1", ExtendedOperationType.LDAP_TTL_REFRESH_OID },
            { "1.3.6.1.4.1.4203.1.11.3", ExtendedOperationType.LDAP_SERVER_WHO_AM_I_OID },
            { "1.2.840.113556.1.4.2212", ExtendedOperationType.LDAP_SERVER_BATCH_REQUEST_OID }
        };
    }

    public enum ExtendedOperationType {
        LDAP_SERVER_FAST_BIND_OID,
        LDAP_SERVER_START_TLS_OID,
        LDAP_TTL_REFRESH_OID,
        LDAP_SERVER_WHO_AM_I_OID,
        LDAP_SERVER_BATCH_REQUEST_OID
    }
}
