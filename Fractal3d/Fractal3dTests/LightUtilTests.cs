using FractureCommonLib;
using ImageCalculator;
using System.Drawing;
using System.Numerics;

namespace Fractal3dTests
{
    [TestClass]
    public class LightUtilTests
    {
        public static IPalette CreateSkyBluePalette()
        {
            return PaletteFactory.CreateTwoPinPalette(10, Color.FromArgb(255, 0, 127, 255), Color.FromArgb(255, 0, 127, 255));
        }

        [TestMethod]
        public void TestCreateTwoPinPalette()
        {
            var pal = CreateSkyBluePalette();

            Assert.AreEqual(10, pal.NumberOfColors);
            Assert.AreEqual(2, pal.GetNumberOfColorPoints());

            var col0 = pal.GetColor(0);
            Assert.AreEqual(0, col0.R);
            Assert.AreEqual(127, col0.G);
            Assert.AreEqual(255, col0.B);

            var col5 = pal.GetColor(0);
            Assert.AreEqual(0, col5.R);
            Assert.AreEqual(127, col5.G);
            Assert.AreEqual(255, col5.B);

            var col9 = pal.GetColor(9);
            Assert.AreEqual(0, col9.R);
            Assert.AreEqual(127, col9.G);
            Assert.AreEqual(255, col9.B);
        }

        [TestMethod]
        public void TestCalculateLight()
        {
            var pal = CreateSkyBluePalette();
            Vector3 lite = new Vector3(0, 0, 0);

            var color = LightingUtil.CalculateLight(5, pal, lite, 1.0f);

            Assert.AreEqual(0, color.R);
            Assert.AreEqual(127, color.G);
            Assert.AreEqual(255, color.B);
        }

        [TestMethod]
        public void TestCalculateLightSat()
        {
            var pal = CreateSkyBluePalette();
            Vector3 lite = new Vector3(1.0f, 1.0f, 1.0f);

            var color = LightingUtil.CalculateLight(5, pal, lite, 1.0f);

            Assert.AreEqual(255, color.R);
            Assert.AreEqual(255, color.G);
            Assert.AreEqual(255, color.B);
        }

        [TestMethod]
        public void TestCalculateLightMid()
        {
            var pal = CreateSkyBluePalette();
            Vector3 lite = new Vector3(0.5f, 0.2f, 0.2f);

            var color = LightingUtil.CalculateLight(5, pal, lite, 1.0f);

            Assert.AreEqual(127, color.R);
            Assert.AreEqual(178, color.G);
            Assert.AreEqual(255, color.B);
        }

        [TestMethod]
        public void TestAmbientPowerWithCalculateLight()
        {
            var pal = CreateSkyBluePalette();
            Vector3 lite = new Vector3(0, 0, 0);

            var color = LightingUtil.CalculateLight(5, pal, lite, 0.5f);

            Assert.AreEqual(0, color.R);
            Assert.AreEqual(63, color.G);
            Assert.AreEqual(127, color.B);
        }
    }
}