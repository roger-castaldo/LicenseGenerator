using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public SignedLicense(byte[] licenseFile, out bool isValid)
            : this(licenseFile,null,out isValid) { }

        public SignedLicense(byte[] licenseFile, ILicensePart[] parts, out bool isValid)
        {
            _license = new License(true);
            string licenseString = null;
            string publicKey = null;
            try
            {
                ZipArchive za = new ZipArchive(new MemoryStream(licenseFile), ZipArchiveMode.Read);
                StreamReader sr;
                foreach (ZipArchiveEntry zae in za.Entries)
                {
                    if (zae.Name == Constants.LicenseFileName)
                    {
                        sr = new StreamReader(zae.Open());
                        licenseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    else if (zae.Name == Constants.KeyFileName)
                    {
                        sr = new StreamReader(zae.Open());
                        publicKey = sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }catch(Exception e)
            {
                licenseString = null;
                publicKey = null;
            }
            if (licenseString == null || publicKey == null)
                throw new FileLoadException("The license file specified is not valid and not loadable");
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

        public bool Validate(string publicKey)
        {
            return _license.Verify(publicKey);
        }
    }
}
