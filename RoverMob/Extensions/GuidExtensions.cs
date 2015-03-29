using System;
using System.Text.RegularExpressions;

namespace RoverMob
{
    public static class GuidExtensions
    {
        private static readonly Regex Punctuation = new Regex(@"[{}-]");

        public static string ToCanonicalString(this Guid guid)
        {
            return Punctuation.Replace(guid.ToString(), "").ToLower();
        }
    }
}
