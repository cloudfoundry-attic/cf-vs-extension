using CloudFoundry.VisualStudio.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CloudFoundry.VisualStudio.UnitTests.Converters
{
    [TestClass]
    public class ImageConverterTest
    {
        [TestMethod]
        public void ImageConvertTest()
        {
            //Arrange
            Bitmap bitmap = new Bitmap(10, 10);
            BitmapImage bitmapImage = null;

            //Act
            bitmapImage = CloudFoundry.VisualStudio.Converters.ImageConverter.ConvertBitmapToBitmapImage(bitmap);

            //Assert
            Assert.IsNotNull(bitmapImage);
            Assert.AreEqual(10, bitmapImage.PixelHeight);
            Assert.AreEqual(10, bitmapImage.PixelWidth);
        }

        [TestMethod]
        public void ImageConvertNullTest()
        {
            //Arrange
            BitmapImage bitmapImage = null;

            //Act
            bitmapImage = CloudFoundry.VisualStudio.Converters.ImageConverter.ConvertBitmapToBitmapImage(null);

            //Assert
            Assert.IsNull(bitmapImage);
        }

    }
}
