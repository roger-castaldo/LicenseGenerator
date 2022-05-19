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
            string pub1;
            string pri1;
            Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateKeyPair(out pub1, out pri1);
            string pub2;
            string pri2;
            Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateKeyPair(out pub2, out pri2);
            Assert.AreNotEqual(pub1, pub2);
            Assert.AreNotEqual(pri1, pri2);
        }

        [TestMethod]
        public void TestUniqueSignatures()
        {
            string pub;
            string pri;
            Org.Reddragonit.LicenseGenerator.SignableLicense.GenerateKeyPair(out pub, out pri);
            Org.Reddragonit.LicenseGenerator.SignableLicense sl1 = new Org.Reddragonit.LicenseGenerator.SignableLicense();
            sl1.StartDate = DateTime.Now;
            sl1.AddApplication("Testing1234");
            sl1.EndDate = DateTime.Now.AddDays(30);
            sl1.PrivateKey = pri;
            sl1.PublicKey = pub;

            Org.Reddragonit.LicenseGenerator.SignableLicense sl2 = new Org.Reddragonit.LicenseGenerator.SignableLicense();
            sl2.StartDate = sl1.StartDate.Value.AddMilliseconds(100);
            sl2.AddApplication("Testing1234");
            sl2.EndDate = sl1.EndDate;
            sl2.PrivateKey=pri;
            sl1.PublicKey=pub;

            Assert.AreNotEqual(sl1.LicenseString, sl2.LicenseString);
            Assert.AreNotEqual(Convert.ToBase64String(sl1.LicenseFile),Convert.ToBase64String(sl2.LicenseFile));
        }

        [TestMethod]
        public void TestProperties()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense sl = new Org.Reddragonit.LicenseGenerator.SignableLicense();

            sl["TestNumber"] = 12;
            Assert.AreEqual(12, sl["TestNumber"]);
            Assert.AreEqual(null, sl["Testing"]);
        }

        [TestMethod]
        public void TestAdditionalParts()
        {
            Org.Reddragonit.LicenseGenerator.SignableLicense sl = new Org.Reddragonit.LicenseGenerator.SignableLicense();
            sl.AddAdditionalPart(new TestAdditionalPart("Testing1234"));
            Assert.IsTrue(sl.AdditionalParts.Length==1);
            Assert.IsTrue(sl.AdditionalParts[0] is TestAdditionalPart);
            Assert.AreEqual("Testing1234", ((TestAdditionalPart)sl.AdditionalParts[0]).Content);

            sl.RemoveAdditionalPart(sl.AdditionalParts[0]);
            Assert.IsTrue(sl.AdditionalParts.Length==0);
        }
    }
}