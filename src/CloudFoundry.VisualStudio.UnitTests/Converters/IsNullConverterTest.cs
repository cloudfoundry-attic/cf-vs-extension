using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.VisualStudio.UnitTests.Converters
{
    [TestClass]
    public class IsNullConverterTest
    {
        [TestMethod]
        public void IsNullConverterConvertBack()
        {
            //Arrange
            CloudFoundry.VisualStudio.Converters.IsNullConverter isNullConverter = new CloudFoundry.VisualStudio.Converters.IsNullConverter();
            var obj = new object();
            InvalidOperationException exception = null;

            //Act
            try
            {
                isNullConverter.ConvertBack(obj, null, null, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (InvalidOperationException e)
            {
                exception = e;
            }

            //Assert
            Assert.IsNotNull(exception);

        }

        [TestMethod]
        public void IsNullConverterConvert()
        {
            //Arrange
            CloudFoundry.VisualStudio.Converters.IsNullConverter isNullConverter = new CloudFoundry.VisualStudio.Converters.IsNullConverter();
            var obj = new object();

            //Act
            isNullConverter.Convert(obj, null, null, System.Globalization.CultureInfo.InvariantCulture);

            //Assert
            Assert.IsNotNull(obj);

        }

    
    }
}
