using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Org.Reddragonit.LicenseGenerator
{
    public class SignedLicense
    {
        private License _license;
        private bool _isValid;

        public SignedLicense(string licenseString,string publicKey,out bool isValid)
        {
            _license = new License();
            string sdata;
            try
            {
                RSACryptoServiceProvider csp = new RSACryptoServiceProvider(Constants.KeySize);
                csp.ImportParameters(Utility.ConvertStringToKey(publicKey));
                sdata = System.Text.ASCIIEncoding.ASCII.GetString(csp.Decrypt(Convert.FromBase64String(licenseString), false));
                _license.Decode(sdata, out isValid);
            }catch(Exception e)
            {
                isValid = false;
            }
            _isValid = isValid;
        }

        public bool HasApplication(string applicationID)
        {
            return _isValid && _license.HasApplication(applicationID);
        }

        public DateTime? ValidStart
        {
            get { return _license.ValidStart; }
        }
        public DateTime? ValidEnd
        {
            get { return _license.ValidEnd; }
        }

        public object this[string property]
        {
            get { return _license[property]; }
        }
    }
}
