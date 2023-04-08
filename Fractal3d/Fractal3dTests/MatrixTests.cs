using ImageCalculator;

namespace Fractal3dTests;

using System.Numerics;

[TestClass]
public class MatrixTests
{
    [TestMethod]
    public void TestCreateTransMatrix()
    {
        var trans = Matrix4x4.CreateTranslation(2, 3, 4);

        Assert.AreEqual(1, trans.M11, 0.0001);
        Assert.AreEqual(0, trans.M12, 0.0001);
        Assert.AreEqual(0, trans.M13, 0.0001);
        Assert.AreEqual(0, trans.M14, 0.0001);

        Assert.AreEqual(0, trans.M21, 0.0001);
        Assert.AreEqual(1, trans.M22, 0.0001);
        Assert.AreEqual(0, trans.M23, 0.0001);
        Assert.AreEqual(0, trans.M24, 0.0001);

        Assert.AreEqual(0, trans.M31, 0.0001);
        Assert.AreEqual(0, trans.M32, 0.0001);
        Assert.AreEqual(1, trans.M33, 0.0001);
        Assert.AreEqual(0, trans.M34, 0.0001);

        Assert.AreEqual(2, trans.M41, 0.0001);
        Assert.AreEqual(3, trans.M42, 0.0001);
        Assert.AreEqual(4, trans.M43, 0.0001);
        Assert.AreEqual(1, trans.M44, 0.0001);
    }

    [TestMethod]
    public void TestNoTransform()
    {
        var tParams = new TransformationParams();
        var m = TransformationCalculator.CreateTransformationMatrix(tParams);

        var pos = new Vector3(3.0f, 7.5f, 0.25f);

        var newPos = TransformationCalculator.Transform(m, pos);

        Assert.AreEqual(3.0f, newPos.X, 0.0001);
        Assert.AreEqual(7.5f, newPos.Y, 0.0001);
        Assert.AreEqual(0.25f, newPos.Z, 0.0001);
    }

    [TestMethod]
    public void TestTranslationTransform()
    {
        var tParams = new TransformationParams
        {
            TranslateY = 1.0f
        };
        var m = TransformationCalculator.CreateTransformationMatrix(tParams);

        var pos = new Vector3(3.0f, 7.5f, 0.25f);

        var newPos = TransformationCalculator.Transform(m, pos);

        Assert.AreEqual(3.0f, newPos.X, 0.0001);
        Assert.AreEqual(8.5f, newPos.Y, 0.0001);
        Assert.AreEqual(0.25f, newPos.Z, 0.0001);
    }

    [TestMethod]
    public void TestRotationTransform()
    {
        var tParams = new TransformationParams
        {
            RotateX = 180.0f
        };
        var m = TransformationCalculator.CreateTransformationMatrix(tParams);

        var pos = new Vector3(3.0f, 7.5f, 0.25f);

        var newPos = TransformationCalculator.Transform(m, pos);

        Assert.AreEqual(3.0f, newPos.X, 0.0001);
        Assert.AreEqual(-7.5f, newPos.Y, 0.0001);
        Assert.AreEqual(-0.25f, newPos.Z, 0.0001);
    }

    [TestMethod]
    public void TestScaleTransform()
    {
        var tParams = new TransformationParams
        {
            ScaleZ = 2.0f
        };
        var m = TransformationCalculator.CreateTransformationMatrix(tParams);

        var pos = new Vector3(3.0f, 7.5f, 0.25f);

        var newPos = TransformationCalculator.Transform(m, pos);

        Assert.AreEqual(3.0f, newPos.X, 0.0001);
        Assert.AreEqual(7.5f, newPos.Y, 0.0001);
        Assert.AreEqual(0.5f, newPos.Z, 0.0001);
    }
}