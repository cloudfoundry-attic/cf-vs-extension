using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
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
            string relativePath = FileUtilities.GetRelativePath(fromPath, toPath);

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
            string relativePath = FileUtilities.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, @"to");
        }


        [TestMethod()]
        public void TestRelativeEmptyTo()
        {
            //Arrange
            string fromPath = @"c:\path\to";
            string toPath = @"";

            //Act
            string relativePath = FileUtilities.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, fromPath);
        }


        [TestMethod()]
        public void TestAddBackslash()
        {
            //Arrange
            string testPathSingleDirWithBackSlash = @"c:\windows\";
            string testPathSubDirWithBackSlash = @"c:\windows\temp\";
            string testPathSingleDirWithoutBackSlash = @"c:\windows";
            string testPathSubDirWithoutBackSlash = @"c:\windows\temp";

            //Act
            string pathSingleDirWithBackSlash = FileUtilities.PathAddBackslash(testPathSingleDirWithBackSlash);
            string pathSubDirWithBackSlash = FileUtilities.PathAddBackslash(testPathSubDirWithBackSlash);
            string pathSingleDirWithoutBackSlash = FileUtilities.PathAddBackslash(testPathSingleDirWithoutBackSlash);
            string pathSubDirWithoutBackSlash = FileUtilities.PathAddBackslash(testPathSubDirWithoutBackSlash);
            
            //Assert
            Assert.AreEqual(testPathSingleDirWithBackSlash, pathSingleDirWithBackSlash);
            Assert.AreEqual(testPathSubDirWithBackSlash, pathSubDirWithBackSlash);
            Assert.AreNotEqual(testPathSingleDirWithoutBackSlash, pathSingleDirWithoutBackSlash);
            Assert.AreNotEqual(testPathSubDirWithoutBackSlash, pathSubDirWithoutBackSlash);
        }



        [TestMethod()]
        public void TestRelativeEqual()
        {
            //Arrange
            string fromPath = @"c:\path\to";
            string toPath = @"c:\path\to";

            //Act
            string relativePath = FileUtilities.GetRelativePath(fromPath, toPath);

            //Assert
            Assert.AreEqual(relativePath, string.Empty);
        }
    }
}
