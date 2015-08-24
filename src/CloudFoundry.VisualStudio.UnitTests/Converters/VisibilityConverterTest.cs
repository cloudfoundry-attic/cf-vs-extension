using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.VisualStudio.UnitTests.Converters
{
    [TestClass]
    public class VisibilityConverterTest
    {
        [TestMethod]
        public void VisibilityConverter()
        {
            //Arrange
            CloudFoundry.VisualStudio.Converters.VisibilityConverter visibilityConverter = new CloudFoundry.VisualStudio.Converters.VisibilityConverter();
            var obj = new object();
            InvalidOperationException exception = null;

            //Act
            try
            {
                visibilityConverter.ConvertBack(obj, null, null, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (InvalidOperationException e)
            {
                exception = e;
            }

            //Assert
            Assert.IsNotNull(exception);

        }
    }
}
