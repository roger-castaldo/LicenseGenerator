using Org.Reddragonit.LicenseGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace UnitTests
{
    internal class TestAdditionalPart : ILicensePart
    {
        private string _content = null;
        public string Content { get { return _content; } }

        public bool IsEmpty
        {
            get { return _content==null; }
        }

        public TestAdditionalPart()
        {
        }

        public TestAdditionalPart(string content)
        {
            _content = content;
        }

        public bool CanLoad(XmlElement element)
        {
            return element.Name=="TestAdditionalPart";
        }

        public void Load(XmlElement element)
        {
            _content = element.InnerText;
        }

        public XmlElement ToElement(XmlDocument document)
        {
            XmlElement ret = document.CreateElement("TestAdditionalPart");
            ret.InnerText = _content;
            return ret;
        }
    }
}
