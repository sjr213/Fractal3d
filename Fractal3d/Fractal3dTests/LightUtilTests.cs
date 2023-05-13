using FractureCommonLib;
using System.Drawing;
using System.Numerics;
using ImageCalculator;

namespace Fractal3dTests
{
    [TestClass]
    public class LightUtilTests
    {
        public static IPalette CreateSkyBluePalette()
        {
            return PaletteFactory.CreateTwoPinPalette(10, Color.FromArgb(255, 0, 127, 255),
                Color.FromArgb(255, 0, 127, 255));
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

        [TestMethod]
        public void TestLightClonePod()
        {
            var light1 = new Light
            {
                LightType = LightType.DirectionalLight,
                SpecularPower = 2.5f,
                DiffusePower = 3.5f,
                ReflectionType = ReflectionType.Phong,
                Shininess = 9.9f
            };

            var light2 = (Light)light1.Clone();
            light2.LightType = LightType.PointLight;
            light2.SpecularPower = 1.3f;
            light2.DiffusePower = 0.7f;
            light2.ReflectionType = ReflectionType.BlinnPhong;
            light2.Shininess = 5.5f;

            Assert.AreEqual(LightType.DirectionalLight, light1.LightType);
            Assert.AreEqual(ReflectionType.Phong, light1.ReflectionType);
            Assert.AreEqual(2.5f, light1.SpecularPower, 0.0001f);
            Assert.AreEqual(3.5f, light1.DiffusePower, 0.0001f);
            Assert.AreEqual(9.9f, light1.Shininess, 0.0001f);

            Assert.AreEqual(LightType.PointLight, light2.LightType);
            Assert.AreEqual(ReflectionType.BlinnPhong, light2.ReflectionType);
            Assert.AreEqual(1.3f, light2.SpecularPower, 0.0001f);
            Assert.AreEqual(0.7f, light2.DiffusePower, 0.0001f);
            Assert.AreEqual(5.5f, light2.Shininess, 0.0001f);
        }

        [TestMethod]
        public void TestLightClonePosition()
        {
            var light1 = new Light
            {
                Position = new Vector3(0.2f, 1.5f, 25.1f)
            };

            var light2 = (Light)light1.Clone();
            var pos = light2.Position;
            pos.X = 2.2f;
            pos.Y = 3.7f;
            pos.Z = 0.75f;
            light2.Position = pos;

            Assert.AreEqual(0.2f, light1.Position.X, 0.0001f);
            Assert.AreEqual(1.5f, light1.Position.Y, 0.0001f);
            Assert.AreEqual(25.1f, light1.Position.Z, 0.0001f);

            Assert.AreEqual(2.2f, light2.Position.X, 0.0001f);
            Assert.AreEqual(3.7f, light2.Position.Y, 0.0001f);
            Assert.AreEqual(0.75f, light2.Position.Z, 0.0001f);
        }

        [TestMethod]
        public void TestLightCloneDiffuseColor()
        {
            var light1 = new Light
            {
                DiffuseColor = new Vector3(0.2f, 1.5f, 0.7f)
            };

            var light2 = (Light)light1.Clone();
            var col = light2.DiffuseColor;
            col.X = 0.9f;
            col.Y = 0.03f;
            col.Z = 0.75f;
            light2.DiffuseColor = col;

            Assert.AreEqual(0.2f, light1.DiffuseColor.X, 0.0001f);
            Assert.AreEqual(1.5f, light1.DiffuseColor.Y, 0.0001f);
            Assert.AreEqual(0.7f, light1.DiffuseColor.Z, 0.0001f);

            Assert.AreEqual(0.9f, light2.DiffuseColor.X, 0.0001f);
            Assert.AreEqual(0.03f, light2.DiffuseColor.Y, 0.0001f);
            Assert.AreEqual(0.75f, light2.DiffuseColor.Z, 0.0001f);
        }

        [TestMethod]
        public void TestLightCloneSpecularColor()
        {
            var light1 = new Light
            {
               SpecularColor = new Vector3(0.2f, 1.5f, 0.7f)
            };

            var light2 = (Light)light1.Clone();
            var col = light2.SpecularColor;
            col.X = 0.9f;
            col.Y = 0.03f;
            col.Z = 0.75f;
            light2.SpecularColor = col;

            Assert.AreEqual(0.2f, light1.SpecularColor.X, 0.0001f);
            Assert.AreEqual(1.5f, light1.SpecularColor.Y, 0.0001f);
            Assert.AreEqual(0.7f, light1.SpecularColor.Z, 0.0001f);

            Assert.AreEqual(0.9f, light2.SpecularColor.X, 0.0001f);
            Assert.AreEqual(0.03f, light2.SpecularColor.Y, 0.0001f);
            Assert.AreEqual(0.75f, light2.SpecularColor.Z, 0.0001f);
        }

    }
}