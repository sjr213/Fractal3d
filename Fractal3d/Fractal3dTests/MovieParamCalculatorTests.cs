using System.Numerics;
using ImageCalculator;
using ImageCalculator.Movie;

namespace Fractal3dTests;

[TestClass]
public class MovieParamCalculatorTests
{
    private const float AngleError = 0.001f;
    private const float BailoutError = 0.01f;
    private const float ConstantCError = 0.001f;

    [TestMethod]
    public void TestCalculateAngleParamsGeneral()
    {
        MovieParams movieParams = new MovieParams()
        {
            FromAngleX = 0,
            ToAngleX = 180,
            NumberOfImages = 10
        };

        var result1 =
            MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, false, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1, AngleError);

        var result2 =
            MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, false, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(20, result2, AngleError);

        var result3 =
            MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, false, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(180, result3, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsLoop()
    {
        MovieParams movieParams = new MovieParams()
        {
            FromAngleX = 0,
            ToAngleX = 180,
            NumberOfImages = 10
        };

        var result1 =
            MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, true, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1, AngleError);

        var result2 =
            MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, true, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(36, result2, AngleError);

        var result3 =
            MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, true, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(324, result3, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsGeneralX()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            FromAngleX = 0,
            ToAngleX = 180,
            NumberOfImages = 10
        };

        var result1 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1.TransformParams.RotateX, AngleError);

        var result2 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(20, result2.TransformParams.RotateX, AngleError);

        var result3 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(180, result3.TransformParams.RotateX, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsLoopX()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            FromAngleX = 0,
            ToAngleX = 180,
            LoopAngleX = true,
            NumberOfImages = 10
        };

        var result1 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1.TransformParams.RotateX, AngleError);

        var result2 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(36, result2.TransformParams.RotateX, AngleError);

        var result3 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(324, result3.TransformParams.RotateX, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsGeneralY()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            FromAngleY = 0,
            ToAngleY = 180,
            NumberOfImages = 10
        };

        var result1 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1.TransformParams.RotateY, AngleError);

        var result2 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(20, result2.TransformParams.RotateY, AngleError);

        var result3 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(180, result3.TransformParams.RotateY, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsLoopY()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            FromAngleY = 0,
            ToAngleY = 0,
            LoopAngleY = true,
            NumberOfImages = 10
        };

        var result1 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1.TransformParams.RotateY, AngleError);

        var result2 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(36, result2.TransformParams.RotateY, AngleError);

        var result3 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(324, result3.TransformParams.RotateY, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsGeneralZ()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            FromAngleZ = 50,
            ToAngleZ = 200,
            NumberOfImages = 20
        };

        var result1 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(50, result1.TransformParams.RotateZ, AngleError);

        var result2 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(57.8947, result2.TransformParams.RotateZ, AngleError);

        var result3 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result1);
        Assert.AreEqual(121.0526, result3.TransformParams.RotateZ, AngleError);
    }

    [TestMethod]
    public void TestCalculateAngleParamsLoopZ()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            FromAngleZ = 0,
            ToAngleZ = 0,
            LoopAngleY = true,
            NumberOfImages = 5
        };

        var result1 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1.TransformParams.RotateY, AngleError);

        var result2 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(72, result2.TransformParams.RotateY, AngleError);

        var result3 = MovieParamCalculator.CalculateFractalAngleParams(fractalParams, movieParams, 5);
        Assert.IsNotNull(result1);
        Assert.AreEqual(288, result3.TransformParams.RotateY, AngleError);
    }

    [TestMethod]
    public void TestCalculateLinearBailout()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            StartBailout = 1,
            EndBailout = 10,
            NumberOfImages = 10
        };

        var result1 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Bailout, BailoutError);

        var result2 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 5);
        Assert.IsNotNull(result2);
        Assert.AreEqual(5, result2.Bailout, BailoutError);

        var result3 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result3);
        Assert.AreEqual(10, result3.Bailout, BailoutError);
    }

    [TestMethod]
    public void TestCalculateReverseLinearBailout()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            StartBailout = 10,
            EndBailout = 1,
            NumberOfImages = 10
        };

        var result1 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(10, result1.Bailout, BailoutError);

        var result2 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 6);
        Assert.IsNotNull(result2);
        Assert.AreEqual(5, result2.Bailout, BailoutError);

        var result3 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 10);
        Assert.IsNotNull(result3);
        Assert.AreEqual(1, result3.Bailout, BailoutError);
    }

    [TestMethod]
    public void TestCalculateExpoBailout()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            StartBailout = 1,
            EndBailout = 100,
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Exponential
        };

        var result1 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Bailout, BailoutError);

        var result2 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(3.1623, result2.Bailout, BailoutError);

        var result3 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 5);
        Assert.IsNotNull(result3);
        Assert.AreEqual(100, result3.Bailout, BailoutError);
    }

    [TestMethod]
    public void TestCalculateReverseExpoBailout()
    {
        FractalParams fractalParams = new();
        MovieParams movieParams = new MovieParams()
        {
            StartBailout = 100,
            EndBailout = 1,
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Exponential
        };

        var result1 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 1);
        Assert.IsNotNull(result1);
        Assert.AreEqual(100, result1.Bailout, BailoutError);

        var result2 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(31.6228, result2.Bailout, BailoutError);

        var result3 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 4);
        Assert.IsNotNull(result3);
        Assert.AreEqual(3.1623, result3.Bailout, BailoutError);

        var result4 = MovieParamCalculator.CalculateBailoutParams(fractalParams, movieParams, 5);
        Assert.IsNotNull(result4);
        Assert.AreEqual(1, result4.Bailout, BailoutError);
    }

    [TestMethod]
    public void TestGetLinearConstantCImageValue()
    {
        var noRange = MovieParamCalculator.GetLinearConstantCImageValue(100, 100, 1, 10);
        Assert.IsNull(noRange);

        var newC1 = MovieParamCalculator.GetLinearConstantCImageValue(-2, 2, 1, 5);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-2f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetLinearConstantCImageValue(-2, 2, 2, 5);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-1f, newC2.Value, ConstantCError);

        var newC3 = MovieParamCalculator.GetLinearConstantCImageValue(-2, 2, 3, 5);
        Assert.IsNotNull(newC3);
        Assert.AreEqual(0f, newC3.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetLinearConstantCImageValue(-2, 2, 5, 5);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(2f, newC5.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetLinearConstantCImageValueNegativeRange()
    {
        var newC1 = MovieParamCalculator.GetLinearConstantCImageValue(2, -2, 1, 5);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(2f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetLinearConstantCImageValue(2, -2, 2, 5);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(1f, newC2.Value, ConstantCError);

        var newC3 = MovieParamCalculator.GetLinearConstantCImageValue(2, -2, 3, 5);
        Assert.IsNotNull(newC3);
        Assert.AreEqual(0f, newC3.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetLinearConstantCImageValue(2, -2, 5, 5);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-2f, newC5.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCImageValueIncreasingPositive()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, 100f, 1, 10);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(0.01f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, 100f, 2, 10);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(0.02783, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, 100f, 5, 10);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(0.59948, newC5.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, 100f, 8, 10);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(12.9154, newC8.Value, ConstantCError);

        var newC10 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, 100f, 10, 10);
        Assert.IsNotNull(newC10);
        Assert.AreEqual(100, newC10.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCImageValueDecreasingPositive()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, 0.01f, 1, 10);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, 0.01f, 2, 10);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(35.9382f, newC2.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, 0.01f, 7, 10);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(0.21544f, newC7.Value, ConstantCError);

        var newC10 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, 0.01f, 10, 10);
        Assert.IsNotNull(newC10);
        Assert.AreEqual(0.01f, newC10.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueWithBothEndsNegativeIncreasingNegative()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(-1000f, -0.1f, 1, 7);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-1000f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(-1000f, -0.1f, 2, 7);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-215.443, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(-1000f, -0.1f, 5, 7);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-2.15443, newC5.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(-1000f, -0.1f, 7, 7);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(-0.1, newC7.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueWithBothEndsNegativeDecreasingNegative()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.1f, -1000f, 1, 7);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-0.1f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.1f, -1000f, 2, 7);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-0.46416, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.1f, -1000f, 5, 7);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-46.416, newC5.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.1f, -1000f, 7, 7);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(-1000, newC7.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosAndOddNumberOfImages()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 1, 7);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 2, 7);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-2.15443, newC2.Value, ConstantCError);

        var newC4 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 4, 7);
        Assert.IsNotNull(newC4);
        Assert.AreEqual(0f, newC4.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 5, 7);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(0.046416, newC5.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 7, 7);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(100, newC7.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosAndEvenNumberOfImages()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 1, 8);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 2, 8);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-4.64159, newC2.Value, ConstantCError);

        var newC4 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 4, 8);
        Assert.IsNotNull(newC4);
        Assert.AreEqual(-0.01f, newC4.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 5, 8);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(0.01, newC5.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 7, 8);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(4.64159, newC7.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 100f, 8, 8);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(100, newC8.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosSmallStart()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.01f, 100f, 1, 10);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(0.001f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.01f, 100f, 2, 10);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(0.0035938, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.01f, 100f, 5, 10);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(0.16681, newC5.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.01f, 100f, 8, 10);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(7.74264, newC8.Value, ConstantCError);

        var newC10 = MovieParamCalculator.GetExponentialConstantCImageValue(-0.01f, 100f, 10, 10);
        Assert.IsNotNull(newC10);
        Assert.AreEqual(100, newC10.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosSmallEnd()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 0.01f, 1, 10);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 0.01f, 2, 10);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-27.8256, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 0.01f, 5, 10);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-0.59948, newC5.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 0.01f, 8, 10);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(-0.012915, newC8.Value, ConstantCError);

        var newC10 = MovieParamCalculator.GetExponentialConstantCImageValue(-100f, 0.01f, 10, 10);
        Assert.IsNotNull(newC10);
        Assert.AreEqual(-0.001, newC10.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValuePosToNegAndOddNumberOfImages()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 1, 7);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 2, 7);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(2.15443, newC2.Value, ConstantCError);

        var newC4 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 4, 7);
        Assert.IsNotNull(newC4);
        Assert.AreEqual(0f, newC4.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 5, 7);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-0.046416, newC5.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 7, 7);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(-100, newC7.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValuePosToNegAndEvenNumberOfImages()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 1, 8);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 2, 8);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(4.64159, newC2.Value, ConstantCError);

        var newC4 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 4, 8);
        Assert.IsNotNull(newC4);
        Assert.AreEqual(0.01f, newC4.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 5, 8);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-0.01, newC5.Value, ConstantCError);

        var newC7 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 7, 8);
        Assert.IsNotNull(newC7);
        Assert.AreEqual(-4.64159, newC7.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -100f, 8, 8);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(-100, newC8.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValuePosToNegSmallStart()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, -100f, 1, 10);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(-0.001f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, -100f, 2, 10);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(-0.0035938, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, -100f, 5, 10);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-0.16681, newC5.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, -100f, 8, 10);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(-7.74264, newC8.Value, ConstantCError);

        var newC10 = MovieParamCalculator.GetExponentialConstantCImageValue(0.01f, -100f, 10, 10);
        Assert.IsNotNull(newC10);
        Assert.AreEqual(-100, newC10.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValuePosToNegSmallEnd()
    {
        var newC1 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -0.01f, 1, 10);
        Assert.IsNotNull(newC1);
        Assert.AreEqual(100f, newC1.Value, ConstantCError);

        var newC2 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -0.01f, 2, 10);
        Assert.IsNotNull(newC2);
        Assert.AreEqual(27.8256, newC2.Value, ConstantCError);

        var newC5 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -0.01f, 5, 10);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(0.59948, newC5.Value, ConstantCError);

        var newC8 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -0.01f, 8, 10);
        Assert.IsNotNull(newC8);
        Assert.AreEqual(0.012915, newC8.Value, ConstantCError);

        var newC10 = MovieParamCalculator.GetExponentialConstantCImageValue(100f, -0.01f, 10, 10);
        Assert.IsNotNull(newC10);
        Assert.AreEqual(0.001, newC10.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetLinearConstantCImageValueExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };
        MovieParams mp1 = new()
        {
            NumberOfImages = 10,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 100,
            ConstantCEndW = 100
        };
        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(0.6f, fp1.C4.W, ConstantCError);

        MovieParams mp2 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = -2,
            ConstantCEndW = 2
        };
        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp2, 1);
        Assert.AreEqual(-2f, fp2.C4.W, ConstantCError);

        MovieParams mp3 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = -2,
            ConstantCEndW = 2
        };
        var fp3 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp3, 2);
        Assert.AreEqual(-1f, fp3.C4.W, ConstantCError);

        MovieParams mp4 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartX = -2,
            ConstantCEndX = 2
        };
        var fp4 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp4, 3);
        Assert.AreEqual(0f, fp4.C4.X, ConstantCError);

        MovieParams mp5 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -2,
            ConstantCEndY = 2
        };
        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp5, 5);
        Assert.AreEqual(2f, fp5.C4.Y, ConstantCError);
    }

    [TestMethod]
    public void TestGetLinearConstantCImageValueNegativeRangeExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 2,
            ConstantCEndW = -2
        };
        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(2f, fp1.C4.W, ConstantCError);

        MovieParams mp2 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartZ = 2,
            ConstantCEndZ = -2
        };
        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp2, 2);
        Assert.AreEqual(1f, fp2.C4.Z, ConstantCError);

        MovieParams mp3 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = 2,
            ConstantCEndY = -2
        };
        var fp3 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp3, 3);
        Assert.AreEqual(0f, fp3.C4.Y, ConstantCError);

        MovieParams mp4 = new()
        {
            NumberOfImages = 5,
            DistributionType = DistributionTypes.Linear,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartX = 2,
            ConstantCEndX = -2
        };
        var fp4 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp4, 5);
        Assert.AreEqual(-2f, fp4.C4.X, ConstantCError);

        var newC5 = MovieParamCalculator.GetLinearConstantCImageValue(2, -2, 5, 5);
        Assert.IsNotNull(newC5);
        Assert.AreEqual(-2f, newC5.Value, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCImageValueIncreasingPositiveExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 10,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartX = 0.01f,
            ConstantCEndX = 100f
        };
        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(0.01f, fp1.C4.X, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(0.02783f, fp2.C4.X, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(0.59948f, fp5.C4.X, ConstantCError);

        var fp8 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 8);
        Assert.AreEqual(12.9154f, fp8.C4.X, ConstantCError);

        var fp10 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 10);
        Assert.AreEqual(100f, fp10.C4.X, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCImageValueDecreasingPositiveExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 10,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartZ = 100f,
            ConstantCEndZ = 0.01f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(100f, fp1.C4.Z, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(35.9382f, fp2.C4.Z, ConstantCError);

        var fp7 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 7);
        Assert.AreEqual(0.21544f, fp7.C4.Z, ConstantCError);

        var fp10 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 10);
        Assert.AreEqual(0.01f, fp10.C4.Z, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueWithBothEndsNegativeIncreasingNegativeExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 7,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -1000f,
            ConstantCEndY = -0.1f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(-1000f, fp1.C4.Y, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(-215.443f, fp2.C4.Y, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(-2.15443f, fp5.C4.Y, ConstantCError);

        var fp7 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 7);
        Assert.AreEqual(-0.1f, fp7.C4.Y, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueWithBothEndsNegativeDecreasingNegativeExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 7,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -0.1f,
            ConstantCEndY = -1000f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(-0.1f, fp1.C4.Y, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(-0.46416f, fp2.C4.Y, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(-46.416f, fp5.C4.Y, ConstantCError);

        var fp7 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 7);
        Assert.AreEqual(-1000f, fp7.C4.Y, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosAndOddNumberOfImagesExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 7,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -100f,
            ConstantCEndY = 100f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(-100f, fp1.C4.Y, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(-2.15443f, fp2.C4.Y, ConstantCError);

        var fp4 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 4);
        Assert.AreEqual(0f, fp4.C4.Y, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(0.046416f, fp5.C4.Y, ConstantCError);

        var fp7 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 7);
        Assert.AreEqual(100f, fp7.C4.Y, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosAndEvenNumberOfImagesExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 8,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -100f,
            ConstantCEndY = 100f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(-100f, fp1.C4.Y, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(-4.64159f, fp2.C4.Y, ConstantCError);

        var fp4 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 4);
        Assert.AreEqual(-0.01f, fp4.C4.Y, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(0.01f, fp5.C4.Y, ConstantCError);

        var fp7 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 7);
        Assert.AreEqual(4.64159f, fp7.C4.Y, ConstantCError);

        var fp8 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 8);
        Assert.AreEqual(100f, fp8.C4.Y, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosSmallStartExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 10,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -0.01f,
            ConstantCEndY = 100f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(0.001f, fp1.C4.Y, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(0.0035938f, fp2.C4.Y, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(0.16681f, fp5.C4.Y, ConstantCError);

        var fp8 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 8);
        Assert.AreEqual(7.74264f, fp8.C4.Y, ConstantCError);

        var fp10 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 10);
        Assert.AreEqual(100f, fp10.C4.Y, ConstantCError);
    }

    [TestMethod]
    public void TestGetExponentialConstantCValueNegToPosSmallEndExternally()
    {
        // C4 default = (0.1f, -0.3f, 0.2f, 0.7f);
        FractalParams fractalParams = new()
        {
            C4 = new Vector4(0.3f, 0.4f, 0.5f, 0.6f)
        };

        MovieParams mp1 = new()
        {
            NumberOfImages = 10,
            DistributionType = DistributionTypes.Exponential,
            MovieType = MovieTypes.ConstantC,
            ConstantCStartY = -100f,
            ConstantCEndY = 0.01f
        };

        var fp1 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 1);
        Assert.AreEqual(-100f, fp1.C4.Y, ConstantCError);

        var fp2 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 2);
        Assert.AreEqual(-27.8256f, fp2.C4.Y, ConstantCError);

        var fp5 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 5);
        Assert.AreEqual(-0.59948f, fp5.C4.Y, ConstantCError);

        var fp8 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 8);
        Assert.AreEqual(-0.012915f, fp8.C4.Y, ConstantCError);

        var fp10 = MovieParamCalculator.CalculateMovieParams(fractalParams, mp1, 10);
        Assert.AreEqual(-0.001f, fp10.C4.Y, ConstantCError);
    }
}

