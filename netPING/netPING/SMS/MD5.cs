using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace netPING
{
    public static class MD5
    {
        public static string HashString(string value)
        {

            byte[] data = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(value));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                hashedString.Append(data[i].ToString("x2"));
            }
            return hashedString.ToString();

        }
        public static bool verifyHashString(string input, string enCodeString)
        {
            string str = HashString(input);
            StringComparer sCompare = StringComparer.OrdinalIgnoreCase;
            if (sCompare.Compare(enCodeString, str) == 0)
                return true;
            return false;
        }
    }
}
