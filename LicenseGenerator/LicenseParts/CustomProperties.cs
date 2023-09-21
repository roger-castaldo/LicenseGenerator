using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator.LicenseParts
{
    internal class CustomProperties : ILicensePart
    {
        private const string ELEMENT_NAME = "Properties";
        private Dictionary<string, object> _values = new();
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

        public CustomProperties() { }

        public bool CanLoad(XmlElement element) 
            => element.Name == ELEMENT_NAME;

        public void Load(XmlElement element)
            => _values = JsonSerializer.Deserialize<Dictionary<string, object>>(((XmlCDataSection)element.ChildNodes[0]).Data);

        public XmlElement ToElement(XmlDocument document)
        {
            var ret = document.CreateElement(ELEMENT_NAME);
            ret.AppendChild(document.CreateCDataSection(JsonSerializer.Serialize(_values)));
            return ret;
        }

        public bool IsEmpty
            => _values.Count==0;
    }
}
