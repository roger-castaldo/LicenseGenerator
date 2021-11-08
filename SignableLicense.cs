using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Org.Reddragonit.LicenseGenerator
{
    public class SignableLicense
    {
        private License _license;
        private string _privateKey;
        public string PrivateKey
        {
            get { return _privateKey; }
            set { _privateKey = value; }
        }
        private string _publicKey;
        public string PublicKey
        {
            get { return _publicKey;}
            set { _publicKey = value; }
        }

        public SignableLicense()
        {
            _license = new License();
        }

        public void AddApplication(string applicationID)
        {
            _license.AddApplication(applicationID);
        }

        public object this[string property]
        {
            get { return _license[property]; }
            set { _license[property] = value; }
        }

        public DateTime? ValidStart
        {
            get { return _license.ValidStart; }
            set { _license.ValidStart = value; }
        }
        public DateTime? ValidEnd
        {
            get { return _license.ValidEnd; }
            set { _license.ValidEnd = value; }
        }

        public void GenerateKeyPair(out string publicKey,out string privateKey)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(Constants.KeySize);
            publicKey = Utility.ConvertKeyToString(csp.ExportParameters(false));
            privateKey = Utility.ConvertKeyToString(csp.ExportParameters(true));
        }

        public string LicenseString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(_license.EncodedValue);
                if (_privateKey == null)
                {
                    GenerateKeyPair(out _publicKey, out _privateKey);
                }
                RSACryptoServiceProvider csp = new RSACryptoServiceProvider(Constants.KeySize);
                csp.ImportParameters(Utility.ConvertStringToKey(_privateKey));
                return Convert.ToBase64String(csp.Encrypt(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()), false));
            }
        }
    }
}
