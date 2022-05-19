using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator.LicenseParts
{
    internal class SerialNumbers : ILicensePart
    {
        private const string ELEMENT_NAME = "SerialNumbers";

        private List<SerialNumber> _values;

        public SerialNumbers()
        {
            _values = new List<SerialNumber>();
        }

        public void AddSerialNumber(string applicationID)
        {
            _values.Add(new SerialNumber(applicationID));
        }

        public bool HasApplication(string applicationID)
        {
            foreach (SerialNumber sn in _values)
            {
                if (sn.IsValid && sn.IsForApplication(applicationID))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasSerialNumber(string serialNumber)
        {
            bool ret = false;
            try
            {
                SerialNumber sn = (SerialNumber)serialNumber;
                if (sn.IsValid)
                {
                    foreach (SerialNumber s in _values)
                    {
                        if (s.IsValid && s.ToString() == sn.ToString())
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }catch(Exception e)
            {
                ret = false;
            }
            return ret;
        }

        public bool CanLoad(XmlElement element)
        {
            return element.Name == ELEMENT_NAME;
        }

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
                ret.InnerText = ret.InnerText.Substring(1);
            return ret;
        }

        public bool IsEmpty { get { return _values.Count == 0; } }
    }
}
