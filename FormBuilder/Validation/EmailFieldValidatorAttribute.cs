using System.Text.RegularExpressions;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class EmailFieldValidatorAttribute : RegexValidatorAttribute
    {
        private static readonly string _pattern = @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b";
        private static readonly Regex _regex = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public EmailFieldValidatorAttribute(string message) : base(message, _pattern) { }

        public static bool Validate(string email)
        {
            return !_regex.IsMatch(email);
        }
    }
}
