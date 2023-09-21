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
            StringWriter sw = new();
            new XmlSerializer(typeof(RSAParameters)).Serialize(sw, pars);
            return sw.ToString();
        }

        public static RSAParameters ConvertStringToKey(string key)
            => (RSAParameters)new XmlSerializer(typeof(RSAParameters)).Deserialize(new StringReader(key));
    }
}
