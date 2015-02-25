using Vmax44Parser;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ParserExistTests
    {
        [TestMethod]
        public void SummParseTest()
        {
            Vmax44Parser.ParserExist parser=new ParserExist();
            decimal expected=293839.49m;
            string s1 = "f293 839.49r.";
            //string s2 = "293,839.49r.";
            string s3 = "293 839,49rub";

            //Assert.AreEqual(expected, parser.summParse(s1));
            //Assert.AreEqual(expected, parser.summParse(s2));
            //Assert.AreEqual(expected, parser.summParse(s3));

        }
    }
}
