using Novell.Directory.Ldap;

using Spectre.Console;

namespace DotWindowsEnum.Services.Ldap {
    public static class LdapUtilities {
        internal const string DEEP_PINK = "deeppink4_2";
        internal const string LIGHT_GREEN = "chartreuse1";
        internal const string LIGHT_BLUE = "steelblue1_1";
        internal const string GOLD = "gold3_1";

        public static void PrintKeyOutput(TreeNode userNode, LdapAttributeSet attributes, string label, string keyname, Func<LdapAttribute, string> convertValue) {
            if (attributes.ContainsKey(keyname)) {
                var attribute = attributes[keyname];
                userNode.AddNode(label)
                    .AddNode($"[steelblue1_1]{convertValue.Invoke(attribute)}[/]");
            }
        }

        public static DateTime ConvertWindowsStringToDate(string windowsDate) {
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
    }
}
