using System;

namespace CustomRPC
{
    internal static class VersionHelper
    {
        public static Version GetVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException("version");

            if (version.StartsWith("v"))
                version = version.Substring(1);

            var array = version.Split('.');

            if (array.Length < 2 || array.Length > 4)
                throw new ArgumentException($"Version has {array.Length} part(s)!", "version");

            switch (array.Length)
            {
                case 2:
                    return Version.Parse(version + ".0.0");
                case 3:
                    return Version.Parse(version + ".0");
            }

            return Version.Parse(version);
        }

        public static string GetVersionString(Version version)
        {
            if (version == null)
                throw new ArgumentNullException("version");

            string res = version.Major + "." + version.Minor;

            if (version.Build > 0 || version.Revision > 0)
                res += "." + version.Build;
            if (version.Revision > 0)
                res += "." + version.Revision;

            return res;
        }

        public static string GetVersionString(string version)
        {
            return GetVersionString(Version.Parse(version));
        }
    }
}
