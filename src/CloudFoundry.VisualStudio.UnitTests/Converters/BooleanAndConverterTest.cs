using CloudFoundry.VisualStudio.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.UnitTests.Converters
{
    [TestClass]
    public class BooleanAndConverterTest
    {

        [TestMethod]
        public void BoleanAndConvBackTest()
        {
            //Arrange
            BooleanAndConverter booleanAndConverter = new BooleanAndConverter();
            var obj = new object();
            NotImplementedException exception = null;

            //Act
            try
            {
                booleanAndConverter.ConvertBack(obj, null, null, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (NotImplementedException e)
            {
                exception = e;
            }

            //Assert
            Assert.IsNotNull(exception);
        }
    }
}
