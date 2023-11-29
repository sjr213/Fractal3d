namespace Fractal3dTests;

using ImageCalculator;
using ImageCalculator.Movie;

[TestClass]
public class MovieParamCalculatorTests
{
    private const float AngleError = 0.001f;
    private const float BailoutError = 0.01f;

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

        var result2 = MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, false, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(20, result2, AngleError);

        var result3 = MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, false, movieParams, 10);
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

        var result2 = MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, true, movieParams, 2);
        Assert.IsNotNull(result2);
        Assert.AreEqual(36, result2, AngleError);

        var result3 = MovieParamCalculator.CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX, true, movieParams, 10);
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
}

