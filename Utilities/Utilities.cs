using System.Text.RegularExpressions;

namespace VendorAPI.Utilities

{
    public static class StringExtensions
    {
        public static string NormalizePhoneNumber(this string phoneNumber)
        {
            return Regex.Replace(phoneNumber, "[^+0-9]", "");
        }
    }
}