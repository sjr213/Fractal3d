using System.Numerics;
using ImageCalculator;
using ImageCalculator.Movie;

namespace Fractal3dTests;

[TestClass]
public class MovieParamsConstantCAlternativeTests
{
    private const float ConstantCError = 0.001f;

    [TestMethod]
    public void TestSimpleCase()
    {
        FractalParams fp = new FractalParams();
        MovieParams movieParams = new MovieParams()
        {
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 0,
            ConstantCEndW = 0,
            ConstantCStartX = 1,
            ConstantCEndX = 1,
            ConstantCStartY = 2,
            ConstantCEndY = 2,
            ConstantCStartZ = 3,
            ConstantCEndZ = 3, Alternate = true,
            StepsW = 1,
            StepsX = 1,
            StepsY = 1,
            StepsZ = 1,
        };

        var result1 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 1);
        Assert.AreEqual(0, result1.C4.W, ConstantCError);
        Assert.AreEqual(1, result1.C4.X, ConstantCError);
        Assert.AreEqual(2, result1.C4.Y, ConstantCError);
        Assert.AreEqual(3, result1.C4.Z, ConstantCError);
    }

    [TestMethod]
    public void TestZOnly()
    {
        FractalParams fp = new FractalParams();
        MovieParams movieParams = new MovieParams()
        {
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 0,
            ConstantCEndW = 0,
            ConstantCStartX = 1,
            ConstantCEndX = 1,
            ConstantCStartY = 2,
            ConstantCEndY = 2,
            ConstantCStartZ = 1,
            ConstantCEndZ = 4,
            Alternate = true,
            StepsW = 1,
            StepsX = 1,
            StepsY = 1,
            StepsZ = 4,
        };

        var result1 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 1);
        Assert.AreEqual(0, result1.C4.W, ConstantCError);
        Assert.AreEqual(1, result1.C4.X, ConstantCError);
        Assert.AreEqual(2, result1.C4.Y, ConstantCError);
        Assert.AreEqual(1, result1.C4.Z, ConstantCError);

        var result2 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 2);
        Assert.AreEqual(0, result2.C4.W, ConstantCError);
        Assert.AreEqual(1, result2.C4.X, ConstantCError);
        Assert.AreEqual(2, result2.C4.Y, ConstantCError);
        Assert.AreEqual(2, result2.C4.Z, ConstantCError);

        var result3 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 3);
        Assert.AreEqual(0, result3.C4.W, ConstantCError);
        Assert.AreEqual(1, result3.C4.X, ConstantCError);
        Assert.AreEqual(2, result3.C4.Y, ConstantCError);
        Assert.AreEqual(3, result3.C4.Z, ConstantCError);

        var result4 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 4);
        Assert.AreEqual(0, result4.C4.W, ConstantCError);
        Assert.AreEqual(1, result4.C4.X, ConstantCError);
        Assert.AreEqual(2, result4.C4.Y, ConstantCError);
        Assert.AreEqual(4, result4.C4.Z, ConstantCError);
    }

    [TestMethod]
    public void TestYAndZOnly()
    {
        FractalParams fp = new FractalParams();
        MovieParams movieParams = new MovieParams()
        {
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 0,
            ConstantCEndW = 0,
            ConstantCStartX = 1,
            ConstantCEndX = 1,
            ConstantCStartY = 2,
            ConstantCEndY = 4,
            ConstantCStartZ = 1,
            ConstantCEndZ = 4,
            Alternate = true,
            StepsW = 1,
            StepsX = 1,
            StepsY = 3,
            StepsZ = 4,
        };

        var result1 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 1);
        Assert.AreEqual(0, result1.C4.W, ConstantCError);
        Assert.AreEqual(1, result1.C4.X, ConstantCError);
        Assert.AreEqual(2, result1.C4.Y, ConstantCError);
        Assert.AreEqual(1, result1.C4.Z, ConstantCError);

        var result3 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 3);
        Assert.AreEqual(0, result3.C4.W, ConstantCError);
        Assert.AreEqual(1, result3.C4.X, ConstantCError);
        Assert.AreEqual(2, result3.C4.Y, ConstantCError);
        Assert.AreEqual(3, result3.C4.Z, ConstantCError);

        var result5 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 5);
        Assert.AreEqual(0, result5.C4.W, ConstantCError);
        Assert.AreEqual(1, result5.C4.X, ConstantCError);
        Assert.AreEqual(3, result5.C4.Y, ConstantCError);
        Assert.AreEqual(1, result5.C4.Z, ConstantCError);

        var result8 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 8);
        Assert.AreEqual(0, result8.C4.W, ConstantCError);
        Assert.AreEqual(1, result8.C4.X, ConstantCError);
        Assert.AreEqual(3, result8.C4.Y, ConstantCError);
        Assert.AreEqual(4, result8.C4.Z, ConstantCError);

        var result9 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 9);
        Assert.AreEqual(0, result9.C4.W, ConstantCError);
        Assert.AreEqual(1, result9.C4.X, ConstantCError);
        Assert.AreEqual(4, result9.C4.Y, ConstantCError);
        Assert.AreEqual(1, result9.C4.Z, ConstantCError);

        var result12 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 12);
        Assert.AreEqual(0, result12.C4.W, ConstantCError);
        Assert.AreEqual(1, result12.C4.X, ConstantCError);
        Assert.AreEqual(4, result12.C4.Y, ConstantCError);
        Assert.AreEqual(4, result12.C4.Z, ConstantCError);
    }

    [TestMethod]
    public void TestXYAndZOnly()
    {
        FractalParams fp = new FractalParams();
        MovieParams movieParams = new MovieParams()
        {
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 0,
            ConstantCEndW = 0,
            ConstantCStartX = 1,
            ConstantCEndX = 3,
            ConstantCStartY = 1,
            ConstantCEndY = 4,
            ConstantCStartZ = 1,
            ConstantCEndZ = 2,
            Alternate = true,
            StepsW = 1,
            StepsX = 3,
            StepsY = 4,
            StepsZ = 2,
        };

        var result1 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 1);
        Assert.AreEqual(0, result1.C4.W, ConstantCError);
        Assert.AreEqual(1, result1.C4.X, ConstantCError);
        Assert.AreEqual(1, result1.C4.Y, ConstantCError);
        Assert.AreEqual(1, result1.C4.Z, ConstantCError);

        var result3 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 3);
        Assert.AreEqual(0, result3.C4.W, ConstantCError);
        Assert.AreEqual(1, result3.C4.X, ConstantCError);
        Assert.AreEqual(2, result3.C4.Y, ConstantCError);
        Assert.AreEqual(1, result3.C4.Z, ConstantCError);

        var result4 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 4);
        Assert.AreEqual(0, result4.C4.W, ConstantCError);
        Assert.AreEqual(1, result4.C4.X, ConstantCError);
        Assert.AreEqual(2, result4.C4.Y, ConstantCError);
        Assert.AreEqual(2, result4.C4.Z, ConstantCError);

        var result7 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 7);
        Assert.AreEqual(0, result7.C4.W, ConstantCError);
        Assert.AreEqual(1, result7.C4.X, ConstantCError);
        Assert.AreEqual(4, result7.C4.Y, ConstantCError);
        Assert.AreEqual(1, result7.C4.Z, ConstantCError);

        var result10 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 10);
        Assert.AreEqual(0, result10.C4.W, ConstantCError);
        Assert.AreEqual(2, result10.C4.X, ConstantCError);
        Assert.AreEqual(1, result10.C4.Y, ConstantCError);
        Assert.AreEqual(2, result10.C4.Z, ConstantCError);

        var result13 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 13);
        Assert.AreEqual(0, result13.C4.W, ConstantCError);
        Assert.AreEqual(2, result13.C4.X, ConstantCError);
        Assert.AreEqual(3, result13.C4.Y, ConstantCError);
        Assert.AreEqual(1, result13.C4.Z, ConstantCError);

        var result18 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 18);
        Assert.AreEqual(0, result18.C4.W, ConstantCError);
        Assert.AreEqual(3, result18.C4.X, ConstantCError);
        Assert.AreEqual(1, result18.C4.Y, ConstantCError);
        Assert.AreEqual(2, result18.C4.Z, ConstantCError);

        var result21 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 21);
        Assert.AreEqual(0, result21.C4.W, ConstantCError);
        Assert.AreEqual(3, result21.C4.X, ConstantCError);
        Assert.AreEqual(3, result21.C4.Y, ConstantCError);
        Assert.AreEqual(1, result21.C4.Z, ConstantCError);

        var result24 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 24);
        Assert.AreEqual(0, result24.C4.W, ConstantCError);
        Assert.AreEqual(3, result24.C4.X, ConstantCError);
        Assert.AreEqual(4, result24.C4.Y, ConstantCError);
        Assert.AreEqual(2, result24.C4.Z, ConstantCError);
    }

    [TestMethod]
    public void TestAllDimensions()
    {
        FractalParams fp = new FractalParams();
        MovieParams movieParams = new MovieParams()
        {
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 1,
            ConstantCEndW = 5,
            ConstantCStartX = 1,
            ConstantCEndX = 2,
            ConstantCStartY = 1,
            ConstantCEndY = 3,
            ConstantCStartZ = 1,
            ConstantCEndZ = 4,
            Alternate = true,
            StepsW = 5,
            StepsX = 2,
            StepsY = 3,
            StepsZ = 4,
        };

        var result1 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 1);
        Assert.AreEqual(1, result1.C4.W, ConstantCError);
        Assert.AreEqual(1, result1.C4.X, ConstantCError);
        Assert.AreEqual(1, result1.C4.Y, ConstantCError);
        Assert.AreEqual(1, result1.C4.Z, ConstantCError);

        var result4 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 4);
        Assert.AreEqual(1, result4.C4.W, ConstantCError);
        Assert.AreEqual(1, result4.C4.X, ConstantCError);
        Assert.AreEqual(1, result4.C4.Y, ConstantCError);
        Assert.AreEqual(4, result4.C4.Z, ConstantCError);

        var result5 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 5);
        Assert.AreEqual(1, result5.C4.W, ConstantCError);
        Assert.AreEqual(1, result5.C4.X, ConstantCError);
        Assert.AreEqual(2, result5.C4.Y, ConstantCError);
        Assert.AreEqual(1, result5.C4.Z, ConstantCError);

        var result10 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 10);
        Assert.AreEqual(1, result10.C4.W, ConstantCError);
        Assert.AreEqual(1, result10.C4.X, ConstantCError);
        Assert.AreEqual(3, result10.C4.Y, ConstantCError);
        Assert.AreEqual(2, result10.C4.Z, ConstantCError);

        var result17 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 17);
        Assert.AreEqual(1, result17.C4.W, ConstantCError);
        Assert.AreEqual(2, result17.C4.X, ConstantCError);
        Assert.AreEqual(2, result17.C4.Y, ConstantCError);
        Assert.AreEqual(1, result17.C4.Z, ConstantCError);

        var result27 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 27);
        Assert.AreEqual(2, result27.C4.W, ConstantCError);
        Assert.AreEqual(1, result27.C4.X, ConstantCError);
        Assert.AreEqual(1, result27.C4.Y, ConstantCError);
        Assert.AreEqual(3, result27.C4.Z, ConstantCError);

        var result31 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 31);
        Assert.AreEqual(2, result31.C4.W, ConstantCError);
        Assert.AreEqual(1, result31.C4.X, ConstantCError);
        Assert.AreEqual(2, result31.C4.Y, ConstantCError);
        Assert.AreEqual(3, result31.C4.Z, ConstantCError);

        var result36 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 36);
        Assert.AreEqual(2, result36.C4.W, ConstantCError);
        Assert.AreEqual(1, result36.C4.X, ConstantCError);
        Assert.AreEqual(3, result36.C4.Y, ConstantCError);
        Assert.AreEqual(4, result36.C4.Z, ConstantCError);

        var result37 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 37);
        Assert.AreEqual(2, result37.C4.W, ConstantCError);
        Assert.AreEqual(2, result37.C4.X, ConstantCError);
        Assert.AreEqual(1, result37.C4.Y, ConstantCError);
        Assert.AreEqual(1, result37.C4.Z, ConstantCError);

        var result47 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 47);
        Assert.AreEqual(2, result47.C4.W, ConstantCError);
        Assert.AreEqual(2, result47.C4.X, ConstantCError);
        Assert.AreEqual(3, result47.C4.Y, ConstantCError);
        Assert.AreEqual(3, result47.C4.Z, ConstantCError);

        var result61 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 61);
        Assert.AreEqual(3, result61.C4.W, ConstantCError);
        Assert.AreEqual(2, result61.C4.X, ConstantCError);
        Assert.AreEqual(1, result61.C4.Y, ConstantCError);
        Assert.AreEqual(1, result61.C4.Z, ConstantCError);

        var result73 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 73);
        Assert.AreEqual(4, result73.C4.W, ConstantCError);
        Assert.AreEqual(1, result73.C4.X, ConstantCError);
        Assert.AreEqual(1, result73.C4.Y, ConstantCError);
        Assert.AreEqual(1, result73.C4.Z, ConstantCError);

        var result82 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 82);
        Assert.AreEqual(4, result82.C4.W, ConstantCError);
        Assert.AreEqual(1, result82.C4.X, ConstantCError);
        Assert.AreEqual(3, result82.C4.Y, ConstantCError);
        Assert.AreEqual(2, result82.C4.Z, ConstantCError);

        var result120 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 120);
        Assert.AreEqual(5, result120.C4.W, ConstantCError);
        Assert.AreEqual(2, result120.C4.X, ConstantCError);
        Assert.AreEqual(3, result120.C4.Y, ConstantCError);
        Assert.AreEqual(4, result120.C4.Z, ConstantCError);
    }

    [TestMethod]
    public void TestBigSteps()
    {
        FractalParams fp = new FractalParams();
        MovieParams movieParams = new MovieParams()
        {
            MovieType = MovieTypes.ConstantC,
            ConstantCStartW = 0,
            ConstantCEndW = 3,
            ConstantCStartX = 10,
            ConstantCEndX = 30,
            ConstantCStartY = 1,
            ConstantCEndY = 10,
            ConstantCStartZ = 5,
            ConstantCEndZ = 15,
            Alternate = true,
            StepsW = 4,
            StepsX = 3,
            StepsY = 10,
            StepsZ = 3,
        };

        var result1 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 1);
        Assert.AreEqual(0, result1.C4.W, ConstantCError);
        Assert.AreEqual(10, result1.C4.X, ConstantCError);
        Assert.AreEqual(1, result1.C4.Y, ConstantCError);
        Assert.AreEqual(5, result1.C4.Z, ConstantCError);

        var result7 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 7);
        Assert.AreEqual(0, result7.C4.W, ConstantCError);
        Assert.AreEqual(10, result7.C4.X, ConstantCError);
        Assert.AreEqual(3, result7.C4.Y, ConstantCError);
        Assert.AreEqual(5, result7.C4.Z, ConstantCError);

        var result14 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 14);
        Assert.AreEqual(0, result14.C4.W, ConstantCError);
        Assert.AreEqual(10, result14.C4.X, ConstantCError);
        Assert.AreEqual(5, result14.C4.Y, ConstantCError);
        Assert.AreEqual(10, result14.C4.Z, ConstantCError);

        var result25 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 25);
        Assert.AreEqual(0, result25.C4.W, ConstantCError);
        Assert.AreEqual(10, result25.C4.X, ConstantCError);
        Assert.AreEqual(9, result25.C4.Y, ConstantCError);
        Assert.AreEqual(5, result25.C4.Z, ConstantCError);

        var result31 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 31);
        Assert.AreEqual(0, result31.C4.W, ConstantCError);
        Assert.AreEqual(20, result31.C4.X, ConstantCError);
        Assert.AreEqual(1, result31.C4.Y, ConstantCError);
        Assert.AreEqual(5, result31.C4.Z, ConstantCError);

        var result38 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 38);
        Assert.AreEqual(0, result38.C4.W, ConstantCError);
        Assert.AreEqual(20, result38.C4.X, ConstantCError);
        Assert.AreEqual(3, result38.C4.Y, ConstantCError);
        Assert.AreEqual(10, result38.C4.Z, ConstantCError);

        var result51 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 51);
        Assert.AreEqual(0, result51.C4.W, ConstantCError);
        Assert.AreEqual(20, result51.C4.X, ConstantCError);
        Assert.AreEqual(7, result51.C4.Y, ConstantCError);
        Assert.AreEqual(15, result51.C4.Z, ConstantCError);

        var result64 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 64);
        Assert.AreEqual(0, result64.C4.W, ConstantCError);
        Assert.AreEqual(30, result64.C4.X, ConstantCError);
        Assert.AreEqual(2, result64.C4.Y, ConstantCError);
        Assert.AreEqual(5, result64.C4.Z, ConstantCError);

        var result91 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 91);
        Assert.AreEqual(1, result91.C4.W, ConstantCError);
        Assert.AreEqual(10, result91.C4.X, ConstantCError);
        Assert.AreEqual(1, result91.C4.Y, ConstantCError);
        Assert.AreEqual(5, result91.C4.Z, ConstantCError);

        var result94 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 94);
        Assert.AreEqual(1, result94.C4.W, ConstantCError);
        Assert.AreEqual(10, result94.C4.X, ConstantCError);
        Assert.AreEqual(2, result94.C4.Y, ConstantCError);
        Assert.AreEqual(5, result94.C4.Z, ConstantCError);

        var result95 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 95);
        Assert.AreEqual(1, result95.C4.W, ConstantCError);
        Assert.AreEqual(10, result95.C4.X, ConstantCError);
        Assert.AreEqual(2, result95.C4.Y, ConstantCError);
        Assert.AreEqual(10, result95.C4.Z, ConstantCError);

        var result183 = MovieParamCalculator.CalculateConstantCAlternateParams(fp, movieParams, 183);
        Assert.AreEqual(2, result183.C4.W, ConstantCError);
        Assert.AreEqual(10, result183.C4.X, ConstantCError);
        Assert.AreEqual(1, result183.C4.Y, ConstantCError);
        Assert.AreEqual(15, result183.C4.Z, ConstantCError);
    }
}

