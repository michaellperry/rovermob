using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.IO;
using System;
using System.IO;
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

        public static Guid ToGuid(this object document)
        {
            var sha = new Sha256Digest();
            var stream = new DigestStream(new MemoryStream(), null, sha);
            using (var writer = new StreamWriter(stream))
            {
                string mementoToString = JsonConvert.SerializeObject(document);
                writer.Write(mementoToString);
            }
            byte[] buffer = new byte[sha.GetDigestSize()];
            sha.DoFinal(buffer, 0);
            Array.Resize(ref buffer, 16);
            return new Guid(buffer);
        }
    }
}
