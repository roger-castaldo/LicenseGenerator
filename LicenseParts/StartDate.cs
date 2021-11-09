using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator.LicenseParts
{
    internal class StartDate : ILicensePart
    {
        private const string ELEMENT_NAME = "StartDate";
        private DateTime? _value;
        public DateTime? Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public bool CanLoad(XmlElement element)
        {
            return element.Name == ELEMENT_NAME;
        }

        public void Load(XmlElement element)
        {
            _value = DateTime.Parse(element.InnerText);
        }

        public XmlElement ToElement(XmlDocument document)
        {
            XmlElement ret = document.CreateElement(ELEMENT_NAME);
            ret.InnerText = _value.ToString();
            return ret;
        }

        public bool IsEmpty { get { return !_value.HasValue; } }
    }
}
