using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class SignableLicense
    {
        [TestMethod]
        public void TestSerialNumbers()
        {
            Assert.AreEqual(Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber("Testing123"), Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber("Testing123"));
            Assert.AreNotEqual(Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber("Testing1234"), Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateSerialNumber("Testing123"));
        }

        [TestMethod]
        public void TestKeyPairs()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateKeyPair(out var pub1, out var pri1);
            Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateKeyPair(out var pub2, out var pri2);
            Assert.AreNotEqual(pub1, pub2);
            Assert.AreNotEqual(pri1, pri2);
        }

        [TestMethod]
        public void TestUniqueSignatures()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateKeyPair(out var pub, out var pri);
            Org.Reddragonit.LicenseGenerator.SignableLicense sl1 = new()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                PrivateKey=pri,
                PublicKey=pub
            };
            sl1.AddApplication("Testing1234");

            Org.Reddragonit.LicenseGenerator.SignableLicense sl2 = new()
            {
                StartDate = sl1.StartDate.Value.AddMilliseconds(100),
                EndDate=sl1.EndDate,
                PrivateKey=pri,
                PublicKey=pub
            };
            sl2.AddApplication("Testing1234");
            

            Assert.AreNotEqual(sl1.LicenseString, sl2.LicenseString);
            Assert.AreNotEqual(Convert.ToBase64String(sl1.LicenseFile),Convert.ToBase64String(sl2.LicenseFile));
        }

        [TestMethod]
        public void TestProperties()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense sl = new();

            sl["TestNumber"] = 12;
            Assert.AreEqual(12, sl["TestNumber"]);
            Assert.AreEqual(null, sl["Testing"]);
        }

        [TestMethod]
        public void TestAdditionalParts()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense sl = new();
            sl.AddAdditionalPart(new TestAdditionalPart("Testing1234"));
            Assert.IsTrue(sl.AdditionalParts.Length==1);
            Assert.IsTrue(sl.AdditionalParts[0] is TestAdditionalPart);
            Assert.AreEqual("Testing1234", ((TestAdditionalPart)sl.AdditionalParts[0]).Content);

            sl.RemoveAdditionalPart(sl.AdditionalParts[0]);
            Assert.IsTrue(sl.AdditionalParts.Length==0);
        }
    }
}