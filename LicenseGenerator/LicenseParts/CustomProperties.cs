using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator.LicenseParts
{
    internal class CustomProperties : ILicensePart
    {
        private const string ELEMENT_NAME = "Properties";
        private Dictionary<string, object> _values;
        public object this[string property]
        {
            get { return (_values.ContainsKey(property) ? _values[property] : null); }
            set
            {
                if (_values.ContainsKey(property))
                    _values.Remove(property);
                if (value != null)
                    _values.Add(property, value);
            }
        }

        public CustomProperties()
        {
            _values = new Dictionary<string, object>();
        }

        public bool CanLoad(XmlElement element)
        {
            return element.Name == ELEMENT_NAME;
        }

        public void Load(XmlElement element)
        {
            _values.Clear();
            Hashtable ht = (Hashtable)JSON.JsonDecode(((XmlCDataSection)element.ChildNodes[0]).Data);
            foreach (string key in ht.Keys)
            {
                _values.Add(key, ht[key]);
            }
        }

        public XmlElement ToElement(XmlDocument document)
        {
            XmlElement ret = document.CreateElement(ELEMENT_NAME);
            Hashtable tmp = new Hashtable();
            foreach (string key in _values.Keys)
                tmp.Add(key, _values[key]);
            ret.AppendChild(document.CreateCDataSection(JSON.JsonEncode(tmp)));
            return ret;
        }

        public bool IsEmpty { get { return _values.Count==0; } }
    }
}
