using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Org.Reddragonit.LicenseGenerator
{
    public class SignedLicense
    {
        private License _license;
        private bool _isValid;

        public SignedLicense(string licenseString, string publicKey, out bool isValid)
            : this(licenseString,publicKey,null,out isValid)
        {}

        public SignedLicense(string licenseString,string publicKey,ILicensePart[] parts,out bool isValid)
        {
            _license = new License(true);
            _license.Load(licenseString, publicKey, out _isValid);
            if (parts != null)
            {
                foreach (ILicensePart ilp in parts)
                    _license.AddAdditionalPart(ilp);
            }
            isValid = _isValid;
        }

        public bool HasApplication(string applicationID)
        {
            return _isValid && _license.HasApplication(applicationID);
        }

        public bool HasSerialNumber(string serialNumber)
        {
            return _isValid && _license.HasSerialNumber(serialNumber);
        }

        public DateTime? StartDate
        {
            get { return _license.StartDate; }
        }
        public DateTime? EndDate
        {
            get { return _license.EndDate; }
        }

        public object this[string property]
        {
            get { return _license[property]; }
        }

        public void AddAdditionalPart(ILicensePart part)
        {
            _license.AddAdditionalPart(part);
        }

        public ILicensePart[] AdditionalParts
        {
            get { return _license.AdditionalParts; }
        }
    }
}
