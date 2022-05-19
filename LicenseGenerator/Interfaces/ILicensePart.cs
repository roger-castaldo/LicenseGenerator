using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Reddragonit.LicenseGenerator.Interfaces
{
    /// <summary>
    /// This interface is used to implement a custom part that can be 
    /// embedded and read from the license content.
    /// </summary>
    public interface ILicensePart
    {
        /// <summary>
        /// Called to see if the part can be loaded from the given xml element
        /// </summary>
        /// <param name="element">The xml element found in the internal document for the license that is not a built in piece</param>
        /// <returns>true if the part can be loaded from the supplied element</returns>
        bool CanLoad(XmlElement element);
        /// <summary>
        /// Called to load the given license part from the supplied xml element
        /// </summary>
        /// <param name="element">The xml element found in the internal document for the license part in question</param>
        void Load(XmlElement element);
        /// <summary>
        /// Called to convert the given license part into an xml element do be added to the internal document
        /// </summary>
        /// <param name="document">The xml document that contains the license information.  This is supplied for creating new elements and content.</param>
        /// <returns>The xml element that will define this part as well as allow it to be loaded later</returns>
        XmlElement ToElement(XmlDocument document);
        /// <summary>
        /// Returns true if the part was loaded but is empty or was not loaded.
        /// </summary>
        bool IsEmpty { get; }

    }
}
