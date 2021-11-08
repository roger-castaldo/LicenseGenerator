using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Org.Reddragonit.LicenseGenerator
{
    internal class License
    {
        private List<SerialNumber> _serialNumbers;
        private DateTime? _validStart;
        public DateTime? ValidStart
        {
            get { return _validStart; }
            set { _validStart = value; }
        }
        private DateTime? _validEnd;
        public DateTime? ValidEnd
        {
            get { return _validEnd; }
            set { _validEnd = value; }
        }
        private Dictionary<string, object> _attributes;

        public License()
        {
            _serialNumbers = new List<SerialNumber>();
            _attributes = new Dictionary<string, object>();
        }

        public void AddApplication(string applicationID)
        {
            _serialNumbers.Add(new SerialNumber(applicationID));
        }

        public bool HasApplication(string applicationID)
        {
            applicationID = applicationID.ToUpper();
            if (_serialNumbers != null)
            {
                foreach (SerialNumber sn in _serialNumbers)
                {
                    if (sn.IsValid && sn.ApplicationID == applicationID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public object this[string property]
        {
            get { return (_attributes.ContainsKey(property) ? _attributes[property] : null); }
            set { 
                if (_attributes.ContainsKey(property))
                    _attributes.Remove(property);
                if (value != null)
                    _attributes.Add(property, value);
            }
        }

        public string EncodedValue
        {
            get
            {
                Hashtable data = new Hashtable();
                if (_serialNumbers.Count>0)
                {
                    ArrayList serials = new ArrayList();
                    foreach (SerialNumber sn in _serialNumbers)
                        serials.Add(sn.ToString());
                    data.Add("SerialNumbers", serials);
                }
                if (_validStart.HasValue)
                    data.Add("ValidStart", _validStart);
                if (_validEnd.HasValue)
                    data.Add("ValidEnd", _validEnd);
                if (_attributes.Count > 0)
                    data.Add("Properties", _attributes);
                string sdata = JSON.JsonEncode(data);
                return sdata+Convert.ToBase64String(new SHA512Managed().ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(sdata)));
            }
        }

        public void Decode(string encodedLicense,out bool isValid)
        {
            string sdata = encodedLicense.Substring(0, encodedLicense.LastIndexOf("}") + 1);
            string hash = encodedLicense.Substring(encodedLicense.LastIndexOf("}") + 1);
            isValid = hash == Convert.ToBase64String(new SHA512Managed().ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(sdata)));
            if (isValid)
            {
                try
                {
                    Hashtable tmp = (Hashtable)JSON.JsonDecode(sdata);
                    if (tmp.ContainsKey("SerialNumbers"))
                    {
                        foreach (string str in (ArrayList)tmp["SerialNumbers"])
                        {
                            _serialNumbers.Add((SerialNumber)str);
                        }
                    }
                    if (tmp.ContainsKey("ValidStart"))
                        _validStart = (DateTime)tmp["ValidStart"];
                    if (tmp.ContainsKey("ValidEnd"))
                        _validEnd = (DateTime)tmp["ValidEnd"];
                    if (tmp.ContainsKey("Properties"))
                    {
                        foreach (string str in (Hashtable)tmp["Properties"])
                        {
                            _attributes.Add(str, ((Hashtable)tmp["Properties"])[str]);
                        }
                    }
                    if (_validStart.HasValue)
                        isValid = _validStart.Value.Ticks >= DateTime.Now.Ticks;
                    if (_validEnd.HasValue && isValid)
                        isValid = _validEnd.Value.Ticks <= DateTime.Now.Ticks;
                }
                catch (Exception e)
                {
                    isValid = false;
                }
            }
        }
    }
}
