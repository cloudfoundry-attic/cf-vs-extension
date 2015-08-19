using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;

namespace CloudFoundry.VisualStudio.UnitTests
{
    [TestClass]
    public class LoggerTest
    {

        [TestMethod]
        public void ErrorExceptionTest()
        {
            //Arrange
            Logger logger = new Logger();
            CloudFoundry.VisualStudio.ProjectPush.VisualStudioException exception = null;
            string message = "test error message";
            Boolean success = true;
            //Act
            try
            {
                Logger.Error(message, exception);
            }
            catch (Exception ex)
            {
                success = false;
            }

            
            //Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void WarningTest()
        {
            //Arrange
            Logger logger = new Logger();
            string message = "test warning message";
            Boolean success = true;
            //Act
            try
            {
                Logger.Warning(message);
            }
            catch (Exception ex)
            {
                success = false;
            }


            //Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void InfoTest()
        {
            //Arrange
            Logger logger = new Logger();
            string message = "test info message";
            Boolean success = true;
            //Act
            try
            {
                Logger.Info(message);
            }
            catch (Exception ex)
            {
                success = false;
            }


            //Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void ErrorTest()
        {
            //Arrange
            Logger logger = new Logger();
            string message = "test error message";
            Boolean success = true;
            //Act
            try
            {
                Logger.Error(message);
            }
            catch (Exception ex)
            {
                success = false;
            }


            //Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void IncompleteErrorTest()
        {
            //Arrange
            Logger logger = new Logger();
            string message = "test error message";
            Boolean success = false;

            string testLogName = EventLog.LogNameFromSourceName("Cloud Foundry Helion Visual Studio Extension", ".");
            EventLog testLog = new EventLog();
            testLog.Log = testLogName;
            EventLogEntryCollection testLogEntryCollection = testLog.Entries;

            //Act
            Logger.Error(message);

            for (int i = 0; i < testLogEntryCollection.Count;i++ )
            {
                if (testLogEntryCollection[i].Message == message) success = true;

            }
                //Assert
                Assert.IsTrue(success);
        }
 
    }
}
