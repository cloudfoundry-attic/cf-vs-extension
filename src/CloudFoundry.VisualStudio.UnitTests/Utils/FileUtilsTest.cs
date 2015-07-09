using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.UnitTests.Utils
{
    [TestClass()]
    public class FileUtilsTest
    {
        [TestMethod()]
        public void TestRelativePathGreater()
        {
            //Arrange
            string fromPath = @"c:\path\to\";
            string toPath = @"c:\path\";

            //Act
            string relativePath = FileUtils.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, @"..\");

        }

        [TestMethod()]
        public void TestRelativePathLower()
        {

            //Arrange
            string fromPath = @"c:\path\";
            string toPath = @"c:\path\to";

            //Act
            string relativePath = FileUtils.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, @"to");
        }

        [TestMethod()]
        public void TestRelativeEmptyFrom()
        {

            //Arrange
            string fromPath = "";
            string toPath = @"c:\path\to";

            //Act
            string relativePath = FileUtils.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, toPath);
        }

        [TestMethod()]
        public void TestRelativeEmptyTo()
        {
            //Arrange
            string fromPath = @"c:\path\to";
            string toPath = @"";

            //Act
            string relativePath = FileUtils.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, fromPath);
        }

        [TestMethod()]
        public void TestRelativeEqual()
        {
            //Arrange
            string fromPath = @"c:\path\to";
            string toPath = @"c:\path\to";

            //Act
            string relativePath = FileUtils.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, string.Empty);
        }
    }
}
