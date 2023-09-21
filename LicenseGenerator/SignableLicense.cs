using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Org.Reddragonit.LicenseGenerator
{
    /// <summary>
    /// This class houses a Signable License.  It is used to generate a signed license, 
    /// with either a provided key pair or it can auto generate one.  It allows for 
    /// all the component building to occur and generate the license from it.
    /// </summary>
    public class SignableLicense
    {
        private readonly License _license;
        /// <summary>
        /// A string representing the private key from the RSA key pair
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// A string representing the public key from the RSA key pair
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Create a new signable license instance to build a license
        /// </summary>
        public SignableLicense()
        {
            _license = new License(false);
        }

        /// <summary>
        /// Returns an array of all the ILicenseParts that were added to the license
        /// </summary>
        public ILicensePart[] AdditionalParts
            => _license.AdditionalParts;

        /// <summary>
        /// Gets/Sets the optional start date for the datetime when this license will be valid.  Null
        /// means that it has no start date for validation.
        /// </summary>
        public DateTime? StartDate
        {
            get { return _license.StartDate; }
            set { _license.StartDate = value; }
        }

        /// <summary>
        /// Gets/Sets the optional end date for the datetime when this license will be valid until.  Null
        /// means that it has no end date and therefore does not expire.
        /// </summary>
        public DateTime? EndDate
        {
            get { return _license.EndDate; }
            set { _license.EndDate = value; }
        }

        /// <summary>
        /// Add an application to the license based on its ID (this is used to validate against in the SignedLicense)
        /// </summary>
        /// <param name="applicationID">The unique ID of the application being license</param>
        public void AddApplication(string applicationID)
            => _license.AddApplication(applicationID);

        /// <summary>
        /// Add a serial number to the license.  This can be supplied as a SerialNumber created by the SerialNumber class 
        /// or a generic string to be encoded into a serial number.  When validating within the Signed License, it has 
        /// be submitted as the encoded Serial Number.
        /// </summary>
        /// <param name="serialNumber">The serial number to add to the license</param>
        public void AddSerialNumber(string serialNumber)
            => _license.AddApplication(serialNumber);

        /// <summary>
        /// Generates a Serial Number from an Application Name/ID
        /// </summary>
        /// <param name="applicationID">The name/ID of the application</param>
        /// <returns>An string representing the encoded serial number</returns>
        public static string GenerateSerialNumber(string applicationID)
            => new SerialNumber(applicationID).ToString();

        /// <summary>
        /// Called to get/set additional properties within the license.  These can be used 
        /// for things like a company name, licensee address, etc.
        /// </summary>
        /// <param name="property">The name of the property to encode in the license</param>
        /// <returns>The value of the property from within the license if it exists</returns>
        public object this[string property]
        {
            get { return _license[property]; }
            set { _license[property] = value; }
        }

        /// <summary>
        /// Called to generate a storable public/private key rsa pair.  This can be used 
        /// if you want to sign multiple licenses with the same key as well as for verification 
        /// purposes later by having the keys available.
        /// </summary>
        /// <param name="publicKey">The public key portion</param>
        /// <param name="privateKey">The private key portion</param>
        public static void GenerateKeyPair(out string publicKey,out string privateKey)
        {
            RSACryptoServiceProvider csp = new(Constants.RSAKeySize);
            publicKey = Utility.ConvertKeyToString(csp.ExportParameters(false));
            privateKey = Utility.ConvertKeyToString(csp.ExportParameters(true));
        }

        /// <summary>
        /// Called to add an additional part (implemented ILicensePart) to the license
        /// </summary>
        /// <param name="part">The ILicensePart implemented class</param>
        public void AddAdditionalPart(ILicensePart part)
            => _license.AddAdditionalPart(part);

        /// <summary>
        /// Called to remove an additional part (implement ILicensePart) from the license
        /// </summary>
        /// <param name="part">The ILicensePart implement class</param>
        public void RemoveAdditionalPart(ILicensePart part)
            => _license.RemoveAdditionalPart(part);

        /// <summary>
        /// Returns the encoded license string for this signable license.  This string 
        /// will need to be pairs off with the public key portion in order to be verified later.
        /// </summary>
        public string LicenseString
        {
            get
            {
                if (PrivateKey == null)
                {
                    GenerateKeyPair(out var publicKey, out var privateKey);
                    PublicKey=publicKey;
                    PrivateKey=privateKey;
                }
                return _license.Encode(PrivateKey);
            }
        }

        /// <summary>
        /// Returns an encoded License File.  The contents of which can be loaded in a SignedLicense
        /// that will allow for license verification.  This file will include the Public Key portion
        /// so that license verification can occur without it being supplied seperately.
        /// </summary>
        public byte[] LicenseFile
        {
            get
            {
                using MemoryStream ms = new();
                var za = new ZipArchive(ms, ZipArchiveMode.Create);
                var lic = za.CreateEntry(Constants.LicenseFileName, CompressionLevel.Optimal);
                var sw = new StreamWriter(lic.Open());
                sw.Write(LicenseString);
                sw.Flush();
                sw.Close();
                var key = za.CreateEntry(Constants.KeyFileName, CompressionLevel.Optimal);
                sw = new(key.Open());
                sw.WriteLine(PublicKey);
                sw.Flush();
                sw.Close();
                za.Dispose();
                return ms.ToArray();
            }
        }
    }
}
