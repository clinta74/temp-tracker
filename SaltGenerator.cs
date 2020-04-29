using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace temp_tracker
{
    public static class SaltGenerator
    {
        internal static byte[] MakeSalty()
        {
            // generate a 128-bit salt using a secure PRNG
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }
    }
}
