using System.Diagnostics;
using System.Drawing;
using ImageCalculator;

namespace Fractal3dTests
{
    [TestClass]
    public class ParallelFactoryTests
    {
        static PixelContainer? FindByFromHeight(IList<PixelContainer> con, int fromHt)
        {
            foreach (var c in con)
            {
                if(c.FromHeight == fromHt) return c;
            }

            return null;
        }


        [TestMethod]
        public void TestCreateContainers()
        {
            // IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
            Size size = new Size(10, 200);

            var con = ParallelFractalFactory.CreateContainers(size, 5, 10);
            Assert.IsNotNull(con);
            Assert.AreEqual(10, con.Count);
            var c0 = FindByFromHeight(con, 0);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.FromHeight);
            Assert.AreEqual(19, c0.ToHeight);
            Assert.AreEqual(5, c0.Depth);
            Assert.AreEqual(10, c0.Width);
            Assert.AreEqual(20, c0.Height);
            var c9 = FindByFromHeight(con, 180);
            Assert.IsNotNull(c9);
            Assert.AreEqual(180, c9.FromHeight);
            Assert.AreEqual(199, c9.ToHeight);
            Assert.AreEqual(5, c9.Depth);
            Assert.AreEqual(10, c9.Width);
            Assert.AreEqual(20, c9.Height);
        }

        [TestMethod]
        public void TestCreateContainersExtraHeight()
        {
            // IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
            Size size = new Size(10, 202);

            var con = ParallelFractalFactory.CreateContainers(size, 5, 10);
            Assert.IsNotNull(con);
            Assert.AreEqual(10, con.Count);
            var c0 = FindByFromHeight(con, 0);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.FromHeight);
            Assert.AreEqual(19, c0.ToHeight);
            Assert.AreEqual(5, c0.Depth);
            Assert.AreEqual(10, c0.Width);
            Assert.AreEqual(20, c0.Height);
            var c9 = FindByFromHeight(con, 180);
            Assert.IsNotNull(c9);
            Assert.AreEqual(180, c9.FromHeight);
            Assert.AreEqual(201, c9.ToHeight);
            Assert.AreEqual(5, c9.Depth);
            Assert.AreEqual(10, c9.Width);
            Assert.AreEqual(22, c9.Height);
        }

        [TestMethod]
        public void TestCreateContainersSmall()
        {
            // IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
            Size size = new Size(10, 20);

            var con = ParallelFractalFactory.CreateContainers(size, 5, 10);
            Assert.IsNotNull(con);
            Assert.AreEqual(6, con.Count);
            var c0 = FindByFromHeight(con, 0);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.FromHeight);
            Assert.AreEqual(2, c0.ToHeight);
            Assert.AreEqual(5, c0.Depth);
            Assert.AreEqual(10, c0.Width);
            Assert.AreEqual(3, c0.Height);
            var c5 = FindByFromHeight(con, 15);
            Assert.IsNotNull(c5);
            Assert.AreEqual(15, c5.FromHeight);
            Assert.AreEqual(19, c5.ToHeight);
            Assert.AreEqual(5, c5.Depth);
            Assert.AreEqual(10, c5.Width);
            Assert.AreEqual(5, c5.Height);
        }
    }
}
