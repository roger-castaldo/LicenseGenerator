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
        private bool _readonly;
        private XmlDocument _currentDoc = new XmlDocument();

        private List<ILicensePart> _additionalParts;
        public ILicensePart[] AdditionalParts
        {
            get { return _additionalParts.ToArray(); }
        }

        private SerialNumbers _serialNumbers=null;
        private StartDate _startDate=null;
        private EndDate _endDate=null;
        private CustomProperties _properties = null;

        private ILicensePart[] _AllParts
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
                _UpdateDoc();
            }
        }

        public DateTime? EndDate
        {
            get { return _endDate.Value; }
            set { 
                _endDate.Value = value;
                _UpdateDoc();
            }
        }

        public void AddApplication(string applicationID)
        {
            _serialNumbers.AddSerialNumber(applicationID);
            _UpdateDoc();
        }

        public bool HasApplication(string applicationID)
        {
            return _serialNumbers.HasApplication(applicationID);
        }

        internal bool HasSerialNumber(string serialNumber)
        {
            return _serialNumbers.HasSerialNumber(serialNumber);
        }

        public object this[string property]
        {
            get { return _properties[property]; }
            set { 
                _properties[property] = value;
                _UpdateDoc();
            }
        }

        public void AddAdditionalPart(ILicensePart part)
        {
            _additionalParts.Add(part);
            _UpdateDoc();
        }

        public void RemoveAdditionalPart(ILicensePart part)
        {
            _additionalParts.Remove(part);
            _UpdateDoc();
        }

        private void _UpdateDoc()
        {
            if (!_readonly)
            {
                lock (_currentDoc)
                {
                    _currentDoc.LoadXml(_emptyDoc);
                    XmlNode coreNode = _currentDoc.ChildNodes[1];
                    foreach (ILicensePart ilp in _AllParts)
                    {
                        if (!ilp.IsEmpty)
                            coreNode.AppendChild(ilp.ToElement(_currentDoc));
                    }
                }
            }
            else
            {
                _ReloadEntities();
            }
        }

        private void _ReloadEntities()
        {
            ILicensePart[] parts = _AllParts;
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

        private byte[] _BinaryValue
        {
            get { return System.Text.UTF8Encoding.UTF8.GetBytes(_currentDoc.OuterXml); }
            set { _currentDoc.LoadXml(System.Text.UTF8Encoding.UTF8.GetString(value)); }
        }

        private byte[] _CompressedValue
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(new GZipStream(ms, CompressionLevel.Optimal, false));
                bw.Write(_BinaryValue);
                bw.Flush();
                bw.Close();
                return ms.ToArray();
            }
            set
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                BinaryReader br = new BinaryReader(new GZipStream(new MemoryStream(value), CompressionMode.Decompress));
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
                _BinaryValue = ms.ToArray();
            }
        }

        private byte[] Sign(string privateKey)
        {
            RSA rsa = RSACryptoServiceProvider.Create(Constants.RSAKeySize);
            rsa.ImportParameters(Utility.ConvertStringToKey(privateKey));
            return rsa.SignData(_BinaryValue, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        public string Encode(string privateKey)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] chunk = _CompressedValue;
            bw.Write(chunk.Length);
            bw.Write(chunk);
            chunk = Sign(privateKey);
            bw.Write(chunk.Length);
            bw.Write(chunk);
            bw.Flush();
            bw.Close();
            return Convert.ToBase64String(ms.ToArray());
        }

        public void Load(string data,string publicKey,out bool isValid)
        {
            isValid = false;
            try
            {
                BinaryReader br = new BinaryReader(new MemoryStream(Convert.FromBase64String(data)));
                _CompressedValue = br.ReadBytes(br.ReadInt32());
                byte[] sig = br.ReadBytes(br.ReadInt32());
                RSA rsa = RSACryptoServiceProvider.Create(Constants.RSAKeySize);
                rsa.ImportParameters(Utility.ConvertStringToKey(publicKey));
                isValid = rsa.VerifyData(_BinaryValue, sig, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                if (isValid)
                {
                    _ReloadEntities();
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
                throw new Exception("Invalid License String");
            }
        }
    }
}
