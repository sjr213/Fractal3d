namespace ImageCalculator.Movie;

public static class FractalParamCalculator
{
    public static FractalParams CalculateFractalParams(FractalParams fractalParams, MovieParams movieParams, int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();

        if (movieParams.MovieType == MovieTypes.Angles)
            return CalculateFractalAngleParams(fractalParams, movieParams, imageNumber);
        if (movieParams.MovieType == MovieTypes.Bailout)

            return CalculateBailoutParams(fractalParams, movieParams, imageNumber);

        return newFractalParams;
    }

    private static FractalParams CalculateFractalAngleParams(FractalParams fractalParams, MovieParams movieParams,
        int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();
        if(movieParams.NumberOfImages < 2)
            return newFractalParams;

        var fromX = movieParams.FromAngleX;
        var toX = movieParams.ToAngleX;
        if (toX < fromX)
        {
            toX += 360;
        }

        var difX = (toX - fromX)/(movieParams.NumberOfImages-1);
        newFractalParams.TransformParams.RotateX = fromX + (imageNumber - 1) * difX;

        var fromY = movieParams.FromAngleY;
        var toY = movieParams.ToAngleY;
        if (toY < fromY)
        {
            toY += 360;
        }

        var difY = (toY - fromY) / (movieParams.NumberOfImages - 1);
        newFractalParams.TransformParams.RotateY = fromY + (imageNumber - 1) * difY;

        var fromZ = movieParams.FromAngleZ;
        var toZ = movieParams.ToAngleZ;
        if (toZ < fromZ)
        {
            toZ += 360;
        }

        var difZ = (toZ - fromZ) / (movieParams.NumberOfImages - 1);
        newFractalParams.TransformParams.RotateZ = fromZ + (imageNumber - 1) * difZ;

        return newFractalParams;
    }

    private static FractalParams CalculateBailoutParams(FractalParams fractalParams, MovieParams movieParams,
        int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();
        if (movieParams.NumberOfImages < 2)
            return newFractalParams;

        var absoluteDif = movieParams.EndBailout - movieParams.StartBailout;
        if (absoluteDif < MovieConstants.MinBailoutDifference)
            return newFractalParams;

        if (movieParams.DistributionType == DistributionTypes.Exponential)
        {
            if (movieParams.EndBailout > movieParams.StartBailout)
            {
                var minLog = Math.Log10(movieParams.StartBailout);
                var maxLog = Math.Log10(movieParams.EndBailout);
                var totalLog = maxLog - minLog;
                var logStep = totalLog / (movieParams.NumberOfImages - 1);
                var exp = minLog + imageNumber * logStep;
                newFractalParams.Bailout = (float)Math.Pow(10.0, exp);
            }
            else
            {
                var minLog = Math.Log10(movieParams.EndBailout);
                var maxLog = Math.Log10(movieParams.StartBailout);
                var totalLog = maxLog - minLog;
                var logStep = totalLog / (movieParams.NumberOfImages - 1);
                var exp = maxLog - imageNumber * logStep;
                newFractalParams.Bailout = (float)Math.Pow(10.0, exp);
            }
        }
        else if(movieParams.DistributionType == DistributionTypes.Linear)
        {
            if (movieParams.EndBailout > movieParams.StartBailout)
            {
                var dif = movieParams.EndBailout - movieParams.StartBailout;
                newFractalParams.Bailout = movieParams.StartBailout + imageNumber * dif;
            }
            else
            {
                var dif = movieParams.StartBailout - movieParams.EndBailout;
                newFractalParams.Bailout = movieParams.StartBailout - imageNumber * dif;
            }
        }

        return newFractalParams;
    }
}

