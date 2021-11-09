using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator.Interfaces
{
    public interface ILicensePart
    {
        bool CanLoad(XmlElement element);
        void Load(XmlElement element);
        XmlElement ToElement(XmlDocument document);
        bool IsEmpty { get; }

    }
}
