using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeC1Contrib.Localization
{
    public class ClassKeysGenerator
    {
        private readonly IEnumerable<string> _keys;
        private readonly string _ns;

        public ClassKeysGenerator(IEnumerable<string> keys, string ns)
        {
            _keys = keys;
            _ns = ns;
        }

        public string Generate()
        {
            var sb = new StringBuilder();

            sb.AppendLine("using CompositeC1Contrib.Localization;");

            sb.AppendLine(String.Empty);

            sb.AppendLine(String.Format("namespace {0}", _ns));
            sb.AppendLine("{");

            sb.AppendLine("    public static class Resources");
            sb.AppendLine("    {");

            GenerateKeysClass(sb);
            GenerateProperties(sb);
            GenerateTMethod(sb);

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateKeysClass(StringBuilder sb)
        {
            sb.AppendLine("        public static class Keys");
            sb.AppendLine("        {");

            foreach (var key in _keys)
            {
                sb.AppendLine("            public const string " + GenerateFieldName(key) + " = \"" + key + "\";");
            }

            sb.AppendLine("        }");
        }

        private void GenerateProperties(StringBuilder sb)
        {
            foreach (var key in _keys)
            {
                sb.AppendLine("        public static string " + GenerateFieldName(key));
                sb.AppendLine("        {");
                sb.AppendLine("            get { return T(Keys." + GenerateFieldName(key) + "); }");
                sb.AppendLine("        }");

                sb.AppendLine(String.Empty);
            }
        }

        private void GenerateTMethod(StringBuilder sb)
        {
            sb.AppendLine("        public static string T(string key)");
            sb.AppendLine("        {");
            sb.AppendLine("            return C1Res.T(key);");
            sb.AppendLine("        }");

            sb.AppendLine(String.Empty);
        }

        private static string GenerateFieldName(string key)
        {
            return key.Replace(".", "_").Replace("-", "_");
        }
    }
}
