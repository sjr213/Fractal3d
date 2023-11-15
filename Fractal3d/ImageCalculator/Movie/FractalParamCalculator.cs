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
        if (movieParams.MovieType == MovieTypes.ConstantC)
            return CalculateConstantCParams(fractalParams, movieParams, imageNumber);

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
                var exp = minLog + (imageNumber-1) * logStep;
                newFractalParams.Bailout = (float)Math.Pow(10.0, exp);
            }
            else
            {
                var minLog = Math.Log10(movieParams.EndBailout);
                var maxLog = Math.Log10(movieParams.StartBailout);
                var totalLog = maxLog - minLog;
                var logStep = totalLog / (movieParams.NumberOfImages - 1);
                var exp = maxLog - (imageNumber-1) * logStep;
                newFractalParams.Bailout = (float)Math.Pow(10.0, exp);
            }
        }
        else if(movieParams.DistributionType == DistributionTypes.Linear)
        {
            if (movieParams.EndBailout > movieParams.StartBailout)
            {
                var dif = (movieParams.EndBailout - movieParams.StartBailout) / (movieParams.NumberOfImages-1);
                newFractalParams.Bailout = movieParams.StartBailout + (imageNumber-1) * dif;
            }
            else
            {
                var dif = (movieParams.StartBailout - movieParams.EndBailout) / (movieParams.NumberOfImages - 1);
                newFractalParams.Bailout = movieParams.StartBailout - (imageNumber-1) * dif;
            }
        }

        return newFractalParams;
    }

    private static float? GetExponentialConstantCImageValue(float start, float end, int imageNumber, int numberOfImages)
    {
        if (Math.Abs(end - start) < MovieConstants.MinConstantCDifference)
            return null;

        if (end > start)
        {
            var minLog = Math.Log10(start);
            var maxLog = Math.Log10(end);
            var totalLog = maxLog - minLog;
            var logStep = totalLog / (numberOfImages - 1);
            var exp = minLog + (imageNumber - 1) * logStep;
            return (float)Math.Pow(10.0, exp);
        }
        else
        {
            var minLog = Math.Log10(end);
            var maxLog = Math.Log10(start);
            var totalLog = maxLog - minLog;
            var logStep = totalLog / (numberOfImages - 1);
            var exp = maxLog - (imageNumber - 1) * logStep;
            return (float)Math.Pow(10.0, exp);
        }
    }

    private static float? GetLinearConstantCImageValue(float start, float end, int imageNumber, int numberOfImages)
    {
        if (Math.Abs(end - start) < MovieConstants.MinConstantCDifference)
            return null;

        if (end > start)
        {
            var dif = (end - start) / (numberOfImages - 1);
            return start + (imageNumber - 1) * dif;
        }
        else
        {
            var dif = (start - end) / (numberOfImages - 1);
            return start - (imageNumber - 1) * dif;
        }
    }

    private static FractalParams CalculateConstantCParams(FractalParams fractalParams, MovieParams movieParams,
        int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();
        if (movieParams.NumberOfImages < 2)
            return newFractalParams;

        if(Math.Abs(movieParams.ConstantCEndW - movieParams.ConstantCStartW) < MovieConstants.MinConstantCDifference &&
           Math.Abs(movieParams.ConstantCEndX - movieParams.ConstantCStartX) < MovieConstants.MinConstantCDifference &&
           Math.Abs(movieParams.ConstantCEndY - movieParams.ConstantCStartY) < MovieConstants.MinConstantCDifference &&
           Math.Abs(movieParams.ConstantCEndZ - movieParams.ConstantCStartZ) < MovieConstants.MinConstantCDifference)
            return newFractalParams;

        var c = newFractalParams.C4;

        if (movieParams.DistributionType == DistributionTypes.Exponential)
        {
            var w = GetExponentialConstantCImageValue(movieParams.ConstantCStartW, movieParams.ConstantCEndW, imageNumber, movieParams.NumberOfImages);
            if (w != null)
            {
                c.W = (float)w;
            }

            var x = GetExponentialConstantCImageValue(movieParams.ConstantCStartX, movieParams.ConstantCEndX, imageNumber, movieParams.NumberOfImages);
            if (x != null)
            {
                c.X = (float)x;
            }

            var y = GetExponentialConstantCImageValue(movieParams.ConstantCStartY, movieParams.ConstantCEndY, imageNumber, movieParams.NumberOfImages);
            if (y != null)
            {
                c.Y = (float)y;
            }

            var z = GetExponentialConstantCImageValue(movieParams.ConstantCStartZ, movieParams.ConstantCEndZ, imageNumber, movieParams.NumberOfImages);
            if (z != null)
            {
                c.Z = (float)z;
            }
        }
        else if (movieParams.DistributionType == DistributionTypes.Linear)
        {
            var w = GetLinearConstantCImageValue(movieParams.ConstantCStartW, movieParams.ConstantCEndW, imageNumber, movieParams.NumberOfImages);
            if (w != null)
            {
                c.W = (float)w;
            }

            var x = GetLinearConstantCImageValue(movieParams.ConstantCStartX, movieParams.ConstantCEndX, imageNumber, movieParams.NumberOfImages);
            if (x != null)
            {
                c.X = (float)x;
            }

            var y = GetLinearConstantCImageValue(movieParams.ConstantCStartY, movieParams.ConstantCEndY, imageNumber, movieParams.NumberOfImages);
            if (y != null)
            {
                c.Y = (float)y;
            }

            var z = GetLinearConstantCImageValue(movieParams.ConstantCStartZ, movieParams.ConstantCEndZ, imageNumber, movieParams.NumberOfImages);
            if (z != null)
            {
                c.Z = (float)z;
            }
        }

        newFractalParams.C4 = c;

        return newFractalParams;
    }
}

