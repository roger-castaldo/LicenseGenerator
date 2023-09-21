using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace Org.Reddragonit.LicenseGenerator.LicenseParts
{
    internal class SerialNumbers : ILicensePart
    {
        private const string ELEMENT_NAME = "SerialNumbers";

        private readonly List<SerialNumber> _values;

        public SerialNumbers()
        {
            _values = new List<SerialNumber>();
        }

        public void AddSerialNumber(string applicationID) 
            => _values.Add(new SerialNumber(applicationID));

        public bool HasApplication(string applicationID)
            => _values.Any(sn => sn.IsValid && sn.IsForApplication(applicationID));

        internal bool HasSerialNumber(string serialNumber)
        {
            try
            {
                SerialNumber sn = (SerialNumber)serialNumber;
                return sn.IsValid && _values.Any(s => s.IsValid && s.ToString().Equals(sn.ToString()));
            }catch(Exception)
            {
                return false;
            }
        }

        public bool CanLoad(XmlElement element)
            => element.Name == ELEMENT_NAME;

        public void Load(XmlElement element)
        {
            _values.Clear();
            foreach (string str in element.InnerText.Split(','))
                _values.Add((SerialNumber)str);
        }

        public XmlElement ToElement(XmlDocument document)
        {
            XmlElement ret = document.CreateElement(ELEMENT_NAME);
            ret.InnerText = "";
            foreach (SerialNumber sn in _values)
                ret.InnerText += "," + sn.ToString();
            if (ret.InnerText != "")
                ret.InnerText = ret.InnerText[1..];
            return ret;
        }

        public bool IsEmpty 
            => _values.Count == 0;
    }
}
