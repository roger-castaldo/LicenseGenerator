using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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

        private static Org.Reddragonit.LicenseGenerator.SignableLicense GenerateLicense(DateTime? startDate=null,DateTime? endDate=null)
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense ret = new()
            {
                StartDate=startDate,
                EndDate=endDate
            };
            ret.AddApplication(TEST_APPLICATION_ID);
            ret.AddSerialNumber(TEST_SERIAL_NUMBER);
            ret.AddAdditionalPart(new TestAdditionalPart(TEST_PART_VALUE));
            ret[TEST_PROPERTY_NAME]=TEST_PROPERTY_VALUE;
            return ret;
        }

        [TestMethod]
        public void TestLoadSignedLicense()
        {
            var lic = SignedLicense.GenerateLicense();
            var sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(lic.LicenseString, lic.PublicKey, out var isValid);
            Assert.IsTrue(isValid);
            Assert.IsTrue(!sl.StartDate.HasValue);
            Assert.IsTrue(!sl.EndDate.HasValue);
            byte[] data = lic.LicenseFile;
            _=new Org.Reddragonit.LicenseGenerator.SignedLicense(data, out isValid);
            Assert.IsTrue(isValid);

            MemoryStream ms = new();
            ms.Write(data, 0, data.Length);

            ZipArchive za = new(ms, ZipArchiveMode.Read);

            var msNew = new MemoryStream();

            ZipArchive zaNew = new ZipArchive(msNew, ZipArchiveMode.Create);

            foreach (var zae in za.Entries)
            {
                if (zae.Name=="lic")
                {
                    StreamReader sr = new(zae.Open());
                    var licData = Convert.FromBase64String(sr.ReadToEnd());
                    sr.Close();

                    licData[licData.Length/2]=(byte)(licData[licData.Length/2]==0 ? 128 : licData[licData.Length/2]-1);

                    var zaeNew = zaNew.CreateEntry("lic");
                    StreamWriter sw = new(zaeNew.Open());
                    sw.Write(Convert.ToBase64String(licData));
                    sw.Flush();
                    sw.Close();
                }
                else
                {
                    var zaeNew = zaNew.CreateEntry(zae.Name);
                    BinaryReader br = new BinaryReader(zae.Open());
                    BinaryWriter bw = new BinaryWriter(zaeNew.Open());
                    bw.Write(br.ReadBytes((int)zae.Length));
                    bw.Flush();
                    bw.Close();
                }
            }

            try
            {
                _=new Org.Reddragonit.LicenseGenerator.SignedLicense(msNew.ToArray(), out isValid);
            }catch(FileLoadException fle)
            {
                Assert.AreEqual("The license file specified is not valid and not loadable", fle.Message, true);
                isValid=false;
            }
            Assert.IsFalse(isValid);
        }


        [TestMethod]
        public void TestHasApplication()
        {
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new(SignedLicense.GenerateLicense().LicenseFile, out bool _);
            Assert.IsTrue(sl.HasApplication(TEST_APPLICATION_ID));
            Assert.IsFalse(sl.HasApplication(TEST_APPLICATION_ID+"-wrong"));
        }

        [TestMethod]
        public void TestHasSerialNumber()
        {
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new(SignedLicense.GenerateLicense().LicenseFile, out bool _);
            Assert.IsTrue(sl.HasSerialNumber(Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber(TEST_SERIAL_NUMBER)));
            Assert.IsFalse(sl.HasSerialNumber(Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber(TEST_SERIAL_NUMBER+"-wrong")));
        }

        [TestMethod]
        public void TestDates()
        {
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime end = DateTime.Now.AddDays(1);

            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new(SignedLicense.GenerateLicense(start,end).LicenseFile, out bool isValid);
            Assert.IsTrue(isValid);
            Assert.AreEqual(start.ToString(), sl.StartDate.Value.ToString());
            Assert.AreEqual(end.ToString(), sl.EndDate.Value.ToString());

            start=DateTime.Now.AddDays(1);
            end = DateTime.Now.AddDays(2);
            new Org.Reddragonit.LicenseGenerator.SignedLicense(SignedLicense.GenerateLicense(start, end).LicenseFile, out isValid);
            Assert.IsFalse(isValid);

            start=DateTime.Now.AddDays(-2);
            end = DateTime.Now.AddDays(-1);
            new Org.Reddragonit.LicenseGenerator.SignedLicense(SignedLicense.GenerateLicense(start, end).LicenseFile, out isValid);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void TestProperties()
        {
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new(SignedLicense.GenerateLicense().LicenseFile, out _);
            Assert.AreEqual((ushort)TEST_PROPERTY_VALUE,sl.GetProperty<ushort>(TEST_PROPERTY_NAME));
            Assert.ThrowsException<KeyNotFoundException>(() => sl.GetProperty<ushort>(TEST_PROPERTY_NAME+"-wrong"));
        }

        [TestMethod]
        public void TestAdditionalParts()
        {
            Org.Reddragonit.LicenseGenerator.SignedLicense sl = new(SignedLicense.GenerateLicense().LicenseFile, out _,new Org.Reddragonit.LicenseGenerator.Interfaces.ILicensePart[] {new TestAdditionalPart()});
            Assert.IsTrue(sl.AdditionalParts.Length==1);
            Assert.AreEqual(TEST_PART_VALUE, ((TestAdditionalPart)sl.AdditionalParts[0]).Content);
            sl = new Org.Reddragonit.LicenseGenerator.SignedLicense(SignedLicense.GenerateLicense().LicenseFile, out _);
            Assert.IsTrue(sl.AdditionalParts.Length==0);
        }
    }
}
