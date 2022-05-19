using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Org.Reddragonit.LicenseGenerator
{
    internal class SerialNumber
    {
        private const string _CHARACTER_SET = "[A-Za-z0-9+/]";
        private static readonly Regex _REG_VALID_SERIAL_NUMBER = new Regex("^("+_CHARACTER_SET+ "{4})-(" + _CHARACTER_SET + "{6})-(" + _CHARACTER_SET + "{8})-(" + _CHARACTER_SET + "{6})-(" + _CHARACTER_SET + "{4})$", RegexOptions.Compiled | RegexOptions.Multiline);

        private string _hash;
        private bool _isValid;
        public bool IsValid { get { return _isValid; } }

        private string _computeHash(string applicationID)
        {
            string code = applicationID;
            while (code.Length < 128) {
                code += "-" + applicationID;
            }
            byte[] data = MD5.Create().ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes(code));
            byte[] checksum = Utility.CacluateChecksum(data, 2);
            byte[] hash = new byte[18];
            data.CopyTo(hash, 0);
            checksum.CopyTo(hash, 16);
            return Convert.ToBase64String(hash);
        }

        private bool _checkHash(string hash)
        {
            bool ret = true;
            byte[] data = new byte[16];
            byte[] checkSum = new byte[2];
            byte[] converted = Convert.FromBase64String(hash);
            for(int x = 0; x < data.Length; x++)
            {
                data[x] = converted[x];
            }
            for(int x = 0; x < checkSum.Length; x++)
            {
                checkSum[x] = converted[x + data.Length];
            }
            byte[] check = Utility.CacluateChecksum(data, 2);
            for(int x = 0; x < checkSum.Length; x++)
            {
                if (check[x] != checkSum[x])
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        public bool IsForApplication(string applicationID)
        {
            return _hash == _computeHash(applicationID);
        }

        private string _checkSum
        {
            get
            {
                return Convert.ToBase64String(Utility.CacluateChecksum((_hash=="" ? new byte[] { 0x00, 0x01 } : Convert.FromBase64String(_hash)), 3));
            }
        }

        private SerialNumber(string hash,string checksum)
        {
            _hash = hash;
            _isValid = checksum == _checkSum && _checkHash(_hash);
        }

        public SerialNumber(string applicationID)
        {
            if (applicationID == null)
                throw new Exception("Unable to generate serial number from null application id");
            if (applicationID=="")
                throw new Exception("Unable to generate serial number from empty application id");
            if (_REG_VALID_SERIAL_NUMBER.IsMatch(applicationID))
            {
                Match m = _REG_VALID_SERIAL_NUMBER.Match(applicationID);
                _hash = m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value + m.Groups[4].Value;
                _isValid  = m.Groups[5].Value==_checkSum && _checkHash(_hash);
            }
            else
            {
                _hash = _computeHash(applicationID);
                _isValid = true;
            }
        }

        private string[] Sections
        {
            get
            {
                return new string[]
                {
                    _hash.Substring(0,4),
                    _hash.Substring(4,6),
                    _hash.Substring(10,8),
                    _hash.Substring(18,6),
                    _checkSum
                };
            }
        }

        public string DisplayString
        {
            get { return ToString().Replace("_", ""); }
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}-{3}-{4}", (object[])Sections);
        }

        public static implicit operator SerialNumber(string value)
        {
            string tmp = value.Replace("-", "");
            return new SerialNumber(tmp.Substring(0,tmp.Length-4),tmp.Substring(tmp.Length-4));
        }
    }
}
