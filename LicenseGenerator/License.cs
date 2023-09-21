using Org.Reddragonit.LicenseGenerator.Interfaces;
using Org.Reddragonit.LicenseGenerator.LicenseParts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator
{
    internal class License
    {
        private const string _emptyDoc = @"<?xml version=""1.0""?>
<license>
</license>";
        private readonly bool _readonly;
        private readonly XmlDocument _currentDoc = new();

        private readonly List<ILicensePart> _additionalParts;
        public ILicensePart[] AdditionalParts
        {
            get { return _additionalParts.ToArray(); }
        }

        private readonly SerialNumbers _serialNumbers=null;
        private readonly StartDate _startDate=null;
        private readonly EndDate _endDate=null;
        private readonly CustomProperties _properties = null;
        private byte[] _loadedSig;

        private ILicensePart[] AllParts
        {
            get
            {
                ILicensePart[] ret = new ILicensePart[4 + _additionalParts.Count];
                ret[0] = _serialNumbers;
                ret[1] = _startDate;
                ret[2] = _endDate;
                ret[3] = _properties;
                if (_additionalParts.Count > 0)
                    _additionalParts.CopyTo(ret, 4);
                return ret;
            }
        }

        public License(bool ReadOnly)
        {
            _readonly = ReadOnly;
            _currentDoc.LoadXml(_emptyDoc);
            _serialNumbers = new SerialNumbers();
            _startDate = new StartDate();
            _endDate = new EndDate();
            _properties = new CustomProperties();
            _additionalParts = new List<ILicensePart>();
        }

        public DateTime? StartDate
        {
            get { return _startDate.Value; }
            set { 
                _startDate.Value = value;
                UpdateDoc();
            }
        }

        public DateTime? EndDate
        {
            get { return _endDate.Value; }
            set { 
                _endDate.Value = value;
                UpdateDoc();
            }
        }

        public void AddApplication(string applicationID)
        {
            _serialNumbers.AddSerialNumber(applicationID);
            UpdateDoc();
        }

        public bool HasApplication(string applicationID) => _serialNumbers.HasApplication(applicationID);

        internal bool HasSerialNumber(string serialNumber) => _serialNumbers.HasSerialNumber(serialNumber);

        public object this[string property]
        {
            get { return _properties[property]; }
            set { 
                _properties[property] = value;
                UpdateDoc();
            }
        }

        public void AddAdditionalPart(ILicensePart part)
        {
            _additionalParts.Add(part);
            UpdateDoc();
        }

        public void RemoveAdditionalPart(ILicensePart part)
        {
            _additionalParts.Remove(part);
            UpdateDoc();
        }

        private void UpdateDoc()
        {
            if (!_readonly)
            {
                lock (_currentDoc)
                {
                    _currentDoc.LoadXml(_emptyDoc);
                    XmlNode coreNode = _currentDoc.ChildNodes[1];
                    foreach (ILicensePart ilp in AllParts)
                    {
                        if (!ilp.IsEmpty)
                            coreNode.AppendChild(ilp.ToElement(_currentDoc));
                    }
                }
            }
            else
            {
                ReloadEntities();
            }
        }

        private void ReloadEntities()
        {
            ILicensePart[] parts = AllParts;
            foreach (XmlNode n in _currentDoc.ChildNodes[1].ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    for (int x = 0; x < parts.Length; x++)
                    {
                        if (parts[x].CanLoad((XmlElement)n))
                        {
                            parts[x].Load((XmlElement)n);
                            break;
                        }
                    }
                }
            }
        }

        private byte[] BinaryValue
        {
            get { return System.Text.UTF8Encoding.UTF8.GetBytes(_currentDoc.OuterXml); }
            set { _currentDoc.LoadXml(System.Text.UTF8Encoding.UTF8.GetString(value)); }
        }

        private byte[] CompressedValue
        {
            get
            {
                MemoryStream ms = new();
                BinaryWriter bw = new(new GZipStream(ms, CompressionLevel.Optimal, false));
                bw.Write(BinaryValue);
                bw.Flush();
                bw.Close();
                var result = ms.ToArray();
                ms.Dispose();
                return result;
            }
            set
            {
                MemoryStream ms = new();
                BinaryWriter bw = new(ms);
                BinaryReader br = new(new GZipStream(new MemoryStream(value), CompressionMode.Decompress));
                while (true)
                {
                    byte[] data = br.ReadBytes(4096);
                    if (data.Length!=0)
                        bw.Write(data);
                    if (data.Length < 4096)
                        break;
                }
                bw.Flush();
                br.Close();
                BinaryValue = ms.ToArray();
                ms.Dispose();
            }
        }

        private byte[] Sign(string privateKey)
        {
            var rsa = RSACryptoServiceProvider.Create(Constants.RSAKeySize);
            rsa.ImportParameters(Utility.ConvertStringToKey(privateKey));
            return rsa.SignData(BinaryValue, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        public string Encode(string privateKey)
        {
            MemoryStream ms = new();
            BinaryWriter bw = new(ms);
            byte[] chunk = CompressedValue;
            bw.Write(chunk.Length);
            bw.Write(chunk);
            chunk = Sign(privateKey);
            bw.Write(chunk.Length);
            bw.Write(chunk);
            bw.Flush();
            bw.Close();
            var result = Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            return result;
        }

        public void Load(string data,string publicKey,out bool isValid)
        {
            try
            {
                using (BinaryReader br = new(new MemoryStream(Convert.FromBase64String(data))))
                {
                    CompressedValue = br.ReadBytes(br.ReadInt32());
                    _loadedSig = br.ReadBytes(br.ReadInt32());
                }
                isValid = Verify(publicKey);
                if (isValid)
                {
                    ReloadEntities();
                    if (StartDate.HasValue)
                    {
                        if (DateTime.Now.Ticks < StartDate.Value.Ticks)
                            isValid = false;
                    }
                    if (isValid && EndDate.HasValue)
                    {
                        if (DateTime.Now.Ticks > EndDate.Value.Ticks)
                            isValid = false;
                    }
                }
            }
            catch(Exception e)
            {
                isValid = false;
                throw new Exception("Invalid License String",e);
            }
        }

        public bool Verify(string publicKey)
        {
            try
            {
                RSA rsa = RSACryptoServiceProvider.Create(Constants.RSAKeySize);
                rsa.ImportParameters(Utility.ConvertStringToKey(publicKey));
                return rsa.VerifyData(BinaryValue, _loadedSig, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }catch(Exception)
            {
                return false;
            }
        }
    }
}
