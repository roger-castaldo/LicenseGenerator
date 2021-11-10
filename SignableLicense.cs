using Org.Reddragonit.LicenseGenerator.Interfaces;
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
            _license = new License(false);
        }

        public ILicensePart[] AdditionalParts { get { return _license.AdditionalParts; } }

        public DateTime? StartDate
        {
            get { return _license.StartDate; }
            set { _license.StartDate = value; }
        }

        public DateTime? EndDate
        {
            get { return _license.EndDate; }
            set { _license.EndDate = value; }
        }

        public void AddApplication(string applicationID)
        {
            _license.AddApplication(applicationID);
        }

        public void AddSerialNumber(string serialNumber)
        {
            _license.AddApplication(serialNumber);
        }

        public static string GenerateSerialNumber(string applicationID)
        {
            return new SerialNumber(applicationID).ToString();
        }

        public object this[string property]
        {
            get { return _license[property]; }
            set { _license[property] = value; }
        }

        public static void GenerateKeyPair(out string publicKey,out string privateKey)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(Constants.RSAKeySize);
            publicKey = Utility.ConvertKeyToString(csp.ExportParameters(false));
            privateKey = Utility.ConvertKeyToString(csp.ExportParameters(true));
        }

        public void AddAdditionalPart(ILicensePart part)
        {
            _license.AddAdditionalPart(part);
        }

        public void RemoveAdditionalPart(ILicensePart part)
        {
            _license.RemoveAdditionalPart(part);
        }

        public string LicenseString
        {
            get
            {
                if (_privateKey == null)
                    GenerateKeyPair(out _publicKey, out _privateKey);
                return _license.Encode(_privateKey);
            }
        }
    }
}
