using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Reddragonit.LicenseGenerator
{
    internal class SerialNumber
    {
        private const int _MAX_APP_ID_LENGTH = 8;
        private const int _RANDOM_LENGTH = 12;
        private string _applicationID;
        public string ApplicationID
        {
            get { return _applicationID.Replace("_", ""); }
        }
        private string _random;
        private string _checkSum;
        private bool _isValid;
        public bool IsValid { get { return _isValid; } }

        public SerialNumber(string applicationID)
        {
            if (applicationID == null)
                throw new Exception("Unable to generate serial number from null application id");
            if (applicationID=="")
                throw new Exception("Unable to generate serial number from empty application id");
            if (applicationID.Length > _MAX_APP_ID_LENGTH)
                throw new Exception("Unable to generate serial number from application id of length " + applicationID.Length.ToString());
            _applicationID = applicationID;
            while (_applicationID.Length < _MAX_APP_ID_LENGTH)
                _applicationID += "_";
            _random = new MT19937(DateTime.Now.Ticks).NextString(Constants.ValidCharacters, _RANDOM_LENGTH);
            _checkSum = Convert.ToBase64String(Utility.CacluateChecksum(System.Text.ASCIIEncoding.ASCII.GetBytes(_applicationID + _random),4)).ToUpper();
            _isValid = true;
        }

        private string[] Sections
        {
            get
            {
                return new string[]
                {
                    _applicationID,
                    _random.Substring(0,4),
                    _random.Substring(4,4),
                    _random.Substring(8,4),
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
            string tmp = value.Replace("-", "").ToUpper();
            SerialNumber ret = new SerialNumber(tmp.Substring(0, tmp.Length - 3));
            ret._isValid = ret._checkSum == tmp.Substring(tmp.Length - 3);
            return ret;
        }
    }
}
