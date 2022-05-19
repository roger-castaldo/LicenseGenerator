using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class SignedLicense
    {
        private const int TEST_PROPERTY_VALUE = 12;
        private const string TEST_PROPERTY_NAME = "TestProperty";
        private const string TEST_PART_VALUE = "Testing123";
        private const string TEST_APPLICATION_ID = "Testing-51234";
        private const string TEST_SERIAL_NUMBER = "Test-Serial-Number";

        private Org.Reddragonit.LicenseGenerator.SignableLicense _GenerateLicense(DateTime? startDate=null,DateTime? endDate=null)
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense ret = new Org.Reddragonit.LicenseGenerator.SignableLicense();
            ret.StartDate=startDate;
            ret.EndDate=endDate;
            ret.AddApplication(TEST_APPLICATION_ID);
            ret.AddSerialNumber(TEST_SERIAL_NUMBER);
            ret.AddAdditionalPart(new TestAdditionalPart(TEST_PART_VALUE));
            ret[TEST_PROPERTY_NAME]=TEST_PROPERTY_VALUE;
            return ret;
        }

        [TestMethod]
        public void TestLoadSignedLicense()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense lic = _GenerateLicense();
            bool isValid;
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(lic.LicenseString, lic.PublicKey, out isValid);
            Assert.IsTrue(isValid);
            Assert.IsTrue(!sl.StartDate.HasValue);
            Assert.IsTrue(!sl.EndDate.HasValue);
            byte[] data = lic.LicenseFile;
            sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(data, out isValid);
            Assert.IsTrue(isValid);

            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);

            ZipArchive za = new ZipArchive(ms, ZipArchiveMode.Update);
            string content = null;

            foreach (ZipArchiveEntry zae in za.Entries)
            {
                if (zae.Name=="lic")
                {
                    StreamReader sr = new StreamReader(zae.Open());
                    content = sr.ReadToEnd();
                    sr.Close();
                    break;
                }
            }

            byte[] tmp = Convert.FromBase64String(content);
            tmp[tmp.Length/2]=(byte)(tmp[tmp.Length/2]==0 ? 128 : tmp[tmp.Length/2]-1);

            ZipArchiveEntry zaen = za.CreateEntry("lic", CompressionLevel.Optimal);
            StreamWriter sw = new StreamWriter(zaen.Open());
            sw.Write(Convert.ToBase64String(tmp));
            sw.Flush();
            sw.Close();

            za.Dispose();

            sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(ms.ToArray(), out isValid);
            Assert.IsFalse(isValid);
        }


        [TestMethod]
        public void TestHasApplication()
        {
            bool isValid;
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense().LicenseFile, out isValid);
            Assert.IsTrue(sl.HasApplication(TEST_APPLICATION_ID));
            Assert.IsFalse(sl.HasApplication(TEST_APPLICATION_ID+"-wrong"));
        }

        [TestMethod]
        public void TestHasSerialNumber()
        {
            bool isValid;
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense().LicenseFile, out isValid);
            Assert.IsTrue(sl.HasSerialNumber(Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber(TEST_SERIAL_NUMBER)));
            Assert.IsFalse(sl.HasSerialNumber(Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber(TEST_SERIAL_NUMBER+"-wrong")));
        }

        [TestMethod]
        public void TestDates()
        {
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime end = DateTime.Now.AddDays(1);

            bool isValid;
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense(start,end).LicenseFile, out isValid);
            Assert.IsTrue(isValid);
            Assert.AreEqual(start.ToString(), sl.StartDate.Value.ToString());
            Assert.AreEqual(end.ToString(), sl.EndDate.Value.ToString());

            start=DateTime.Now.AddDays(1);
            end = DateTime.Now.AddDays(2);
            sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense(start, end).LicenseFile, out isValid);
            Assert.IsFalse(isValid);

            start=DateTime.Now.AddDays(-2);
            end = DateTime.Now.AddDays(-1);
            sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense(start, end).LicenseFile, out isValid);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void TestProperties()
        {
            bool isValid;
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense().LicenseFile, out isValid);
            Assert.AreEqual((ushort)TEST_PROPERTY_VALUE,sl[TEST_PROPERTY_NAME]);
            Assert.AreEqual(null,sl[TEST_PROPERTY_NAME+"-wrong"]);
        }

        [TestMethod]
        public void TestAdditionalParts()
        {
            bool isValid;
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense().LicenseFile, out isValid,new Org.Reddragonit.LicenseGenerator.Interfaces.ILicensePart[] {new TestAdditionalPart()});
            Assert.IsTrue(sl.AdditionalParts.Length==1);
            Assert.AreEqual(TEST_PART_VALUE, ((TestAdditionalPart)sl.AdditionalParts[0]).Content);
            sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(_GenerateLicense().LicenseFile, out isValid);
            Assert.IsTrue(sl.AdditionalParts.Length==0);
        }
    }
}
