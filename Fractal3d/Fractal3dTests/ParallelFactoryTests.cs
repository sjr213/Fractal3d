using System.Drawing;
using ImageCalculator;

namespace Fractal3dTests
{
    [TestClass]
    public class ParallelFactoryTests
    {
        static PixelContainer? FindByFromWidth(IList<PixelContainer> con, int fromWidth)
        {
            foreach (var c in con)
            {
                if(c.FromWidth == fromWidth) return c;
            }

            return null;
        }


        [TestMethod]
        public void TestCreateContainers()
        {
            // IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
            Size size = new Size(200, 10);

            var con = ParallelFractalFactory.CreateContainers(size, 5, 10);
            Assert.IsNotNull(con);
            Assert.AreEqual(10, con.Count);
            var c0 = FindByFromWidth(con, 0);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.FromWidth);
            Assert.AreEqual(19, c0.ToWidth);
            Assert.AreEqual(5, c0.Depth);
            Assert.AreEqual(20, c0.Width);
            Assert.AreEqual(10, c0.Height);
            var c9 = FindByFromWidth(con, 180);
            Assert.IsNotNull(c9);
            Assert.AreEqual(180, c9.FromWidth);
            Assert.AreEqual(199, c9.ToWidth);
            Assert.AreEqual(5, c9.Depth);
            Assert.AreEqual(20, c9.Width);
            Assert.AreEqual(10, c9.Height);
        }

        [TestMethod]
        public void TestCreateContainersExtraHeight()
        {
            // IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
            Size size = new Size(202, 10);

            var con = ParallelFractalFactory.CreateContainers(size, 5, 10);
            Assert.IsNotNull(con);
            Assert.AreEqual(10, con.Count);
            var c0 = FindByFromWidth(con, 0);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.FromWidth);
            Assert.AreEqual(19, c0.ToWidth);
            Assert.AreEqual(5, c0.Depth);
            Assert.AreEqual(20, c0.Width);
            Assert.AreEqual(10, c0.Height);
            var c9 = FindByFromWidth(con, 180);
            Assert.IsNotNull(c9);
            Assert.AreEqual(180, c9.FromWidth);
            Assert.AreEqual(201, c9.ToWidth);
            Assert.AreEqual(5, c9.Depth);
            Assert.AreEqual(22, c9.Width);
            Assert.AreEqual(10, c9.Height);
        }

        [TestMethod]
        public void TestCreateContainersSmall()
        {
            // IList<PixelContainer> CreateContainers(Size size, int depth, int numberOfContainers)
            Size size = new Size(20, 10);

            var con = ParallelFractalFactory.CreateContainers(size, 5, 10);
            Assert.IsNotNull(con);
            Assert.AreEqual(6, con.Count);
            var c0 = FindByFromWidth(con, 0);
            Assert.IsNotNull(c0);
            Assert.AreEqual(0, c0.FromWidth);
            Assert.AreEqual(2, c0.ToWidth);
            Assert.AreEqual(5, c0.Depth);
            Assert.AreEqual(3, c0.Width);
            Assert.AreEqual(10, c0.Height);
            var c5 = FindByFromWidth(con, 15);
            Assert.IsNotNull(c5);
            Assert.AreEqual(15, c5.FromWidth);
            Assert.AreEqual(19, c5.ToWidth);
            Assert.AreEqual(5, c5.Depth);
            Assert.AreEqual(5, c5.Width);
            Assert.AreEqual(10, c5.Height);
        }

        [TestMethod]
        public void TestArrayCopy()
        {
            int[,] original =
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
            };

            int[,] cpy = new int[4, 3];

            Array.Copy(original, 0, cpy, 6, 6);

  //          Assert.AreEqual(1, cpy[0, 0]);
   //         Assert.AreEqual(2, cpy[0, 1]);
 //           Assert.AreEqual(3, cpy[0, 2]);

 //           Assert.AreEqual(1, cpy[1, 0]);
 //           Assert.AreEqual(2, cpy[1, 1]);
 //           Assert.AreEqual(3, cpy[1, 2]);

            Assert.AreEqual(1, cpy[2, 0]);
            Assert.AreEqual(2, cpy[2, 1]);
            Assert.AreEqual(3, cpy[2, 2]);

            Assert.AreEqual(4, cpy[3, 0]);
            Assert.AreEqual(5, cpy[3, 1]);
            Assert.AreEqual(6, cpy[3, 2]);
        }
    }
}
