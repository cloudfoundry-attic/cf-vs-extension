using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.VisualStudio.UnitTests.Converters
{
    [TestClass]
    public class InverseBooleanConverterTest
    {
        [TestMethod]
        public void InverseBoleanConverter()
        {
            //Arrange
            CloudFoundry.VisualStudio.Converters.InverseBooleanConverter inverseBooleanAndConverter = new CloudFoundry.VisualStudio.Converters.InverseBooleanConverter();
            var obj = new object();
            NotSupportedException exception = null;

            //Act
            try
            {
                inverseBooleanAndConverter.ConvertBack(obj, null, null, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (NotSupportedException e)
            {
                exception = e;
            }

            //Assert
            Assert.IsNotNull(exception);
        }
    }
}
