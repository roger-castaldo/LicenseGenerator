using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Org.Reddragonit.LicenseGenerator
{
    internal static class Utility
    {

        private static MT19937 _random;

        static Utility()
        {
            _random = new MT19937(DateTime.Now.Ticks);
        }

        public static byte[] RandomBytes(int length)
        {
            byte[] ret;
            lock (_random)
            {
                ret = _random.NextBytes(length);
            }
            return ret;
        }

        public static string RandomString(string charsAllowed,int length)
        {
            string ret = "";
            lock (_random)
            {
                ret = _random.NextString(charsAllowed, length);
            }
            return ret;
        }

        public static byte[] CacluateChecksum(byte[] data,int length)
        {
            byte[] ret = new byte[length];
            for(int x = 0; x < ret.Length; x++)
            {
                ret[x] = 0x00;
            }
            int idx = 0;
            for(int x = 0; x < data.Length; x++)
            {
                ret[idx] ^= data[x];
                idx++;
                if (idx >= ret.Length)
                {
                    idx = 0;
                }
            }
            return ret;
        }

        public static string ConvertKeyToString(RSAParameters pars)
        {
            StringWriter sw = new StringWriter();
            new XmlSerializer(typeof(RSAParameters)).Serialize(sw, pars);
            return sw.ToString();
        }

        public static RSAParameters ConvertStringToKey(string key)
        {
            StringReader sr = new StringReader(key);
            return (RSAParameters)new XmlSerializer(typeof(RSAParameters)).Deserialize(sr);
        }
    }
}
