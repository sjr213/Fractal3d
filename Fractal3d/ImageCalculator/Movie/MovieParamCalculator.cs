namespace ImageCalculator.Movie;

public static class MovieParamCalculator
{
    public static FractalParams CalculateMovieParams(FractalParams fractalParams, MovieParams movieParams, int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();

        return movieParams.MovieType switch
        {
            MovieTypes.Angles => CalculateFractalAngleParams(fractalParams, movieParams, imageNumber),
            MovieTypes.Bailout => CalculateBailoutParams(fractalParams, movieParams, imageNumber),
            MovieTypes.ConstantC => CalculateConstantCParams(fractalParams, movieParams, imageNumber),
            _ => newFractalParams
        };
    }

    public static float CalculateAngle(float fromAngle, float toAngle, bool loop, MovieParams movieParams, int imageNumber)
    {
        if (loop)
        {
            fromAngle = 0;
            toAngle = 360f - 360f / movieParams.NumberOfImages;
        }

        while (fromAngle > 360f)
            fromAngle -= 360f;

        while (toAngle > 360f)
            toAngle -= 360f;

        if (toAngle < fromAngle)
        {
            (fromAngle, toAngle) = (toAngle, fromAngle);
        }

        var difX = (toAngle - fromAngle) / (movieParams.NumberOfImages - 1);
        return fromAngle + (imageNumber - 1) * difX;
    }

    public static FractalParams CalculateFractalAngleParams(FractalParams fractalParams, MovieParams movieParams,
        int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();
        if(movieParams.NumberOfImages < 2)
            return newFractalParams;

        newFractalParams.TransformParams.RotateX = CalculateAngle(movieParams.FromAngleX, movieParams.ToAngleX,
            movieParams.LoopAngleX, movieParams, imageNumber);

        newFractalParams.TransformParams.RotateY = CalculateAngle(movieParams.FromAngleY, movieParams.ToAngleY,
            movieParams.LoopAngleY, movieParams, imageNumber);

        newFractalParams.TransformParams.RotateZ = CalculateAngle(movieParams.FromAngleZ, movieParams.ToAngleZ,
            movieParams.LoopAngleZ, movieParams, imageNumber);

        return newFractalParams;
    }

    public static FractalParams CalculateBailoutParams(FractalParams fractalParams, MovieParams movieParams,
        int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();
        if (movieParams.NumberOfImages < 2)
            return newFractalParams;

        var absoluteDif = Math.Abs(movieParams.EndBailout - movieParams.StartBailout);
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

    private static float GetExponentialConstantCValueWithBothEndsPositive(float start, float end, int imageNumber, int numberOfImages)
    {
        if (end > start)
        {
            if (start < MovieConstants.MinConstantCExponent)
                start = (float)MovieConstants.MinConstantCExponent;
            var minLog = Math.Log10(start);
            var maxLog = Math.Log10(end);
            var totalLog = maxLog - minLog;
            var logStep = totalLog / (numberOfImages-1);
            var exp = minLog + (imageNumber - 1) * logStep;
            return (float)Math.Pow(10.0, exp);
        }
        else
        {
            if (end < MovieConstants.MinConstantCExponent)
                end = (float)MovieConstants.MinConstantCExponent;
            var minLog = Math.Log10(end);
            var maxLog = Math.Log10(start);
            var totalLog = maxLog - minLog;
            var logStep = totalLog / (numberOfImages-1);
            var exp = maxLog - (imageNumber - 1) * logStep;
            return (float)Math.Pow(10.0, exp);
        }
    }

    private static float GetExponentialConstantCValueWithBothEndsNegative(float start, float end, int imageNumber, int numberOfImages)
    {
        if (end > start)
        {
            if (end > -1.0f * MovieConstants.MinConstantCExponent)
                end = (float) (-1.0f * MovieConstants.MinConstantCExponent);
            var minLog = Math.Log10(-end);
            var maxLog = Math.Log10(-start);
            var totalLog = maxLog - minLog;
            var logStep = totalLog / (numberOfImages-1);
            var exp = minLog + (numberOfImages - imageNumber) * logStep;
            return -1.0f * (float)Math.Pow(10.0, exp);
        }
        else
        {
            if (start > -1.0f * MovieConstants.MinConstantCExponent)
                start = (float)(-1.0f * MovieConstants.MinConstantCExponent);
            var minLog = Math.Log10(-start);
            var maxLog = Math.Log10(-end);
            var totalLog = maxLog - minLog;
            var logStep = totalLog / (numberOfImages-1);
            var exp = maxLog - (numberOfImages - imageNumber) * logStep;
            return -1.0f * (float)Math.Pow(10.0, exp);
        }
    }

    private static float GetExponentialConstantCValueOverPositiveNegativeRange(float start, float end, int imageNumber, int numberOfImages)
    {
        var absStart = Math.Abs(start);
        var absEnd = Math.Abs(end);

        // negative to positive
        if (end > start)
        {
            if (absStart < absEnd / 100.0f || absStart < MovieConstants.MinConstantCExpoDifference)
            {
                // ignore start on negative side
                return GetExponentialConstantCValueWithBothEndsPositive((float)MovieConstants.MinConstantCExponent, end, imageNumber, numberOfImages);
            }

            if (absEnd < absStart/ 100f || absEnd < MovieConstants.MinConstantCExpoDifference)
            {
                // ignore end on positive side
                return GetExponentialConstantCValueWithBothEndsNegative(start, (float)-MovieConstants.MinConstantCExponent, imageNumber, numberOfImages);
            }

            var oddNumberOfImages = (numberOfImages % 2) == 1;
            if (oddNumberOfImages)
            {
                // negative exponent
                if(imageNumber < numberOfImages/2 + 1)
                    return GetExponentialConstantCValueWithBothEndsNegative(start, (float)-MovieConstants.MinConstantCExponent, imageNumber, numberOfImages/2 + 1);

                if (imageNumber == numberOfImages / 2 + 1)
                    return 0f;

                var rightImageNumber = imageNumber - numberOfImages / 2;
                return GetExponentialConstantCValueWithBothEndsPositive((float)MovieConstants.MinConstantCExponent, end, rightImageNumber, numberOfImages/2 + 1);
            }
            else
            {
                // negative exponent
                if (imageNumber <= numberOfImages / 2)  // this will include the zero
                    return GetExponentialConstantCValueWithBothEndsNegative(start, (float)-MovieConstants.LowConstantCExponent, imageNumber, numberOfImages / 2);

                var rightImageNumber = imageNumber - numberOfImages / 2; 
                return GetExponentialConstantCValueWithBothEndsPositive((float)MovieConstants.LowConstantCExponent, end, rightImageNumber, numberOfImages / 2);
            }
        }
        else // positive to negative
        {
            if (absStart < absEnd/100f || absStart < MovieConstants.MinConstantCExpoDifference)
            {
                // ignore start on positive side
                return GetExponentialConstantCValueWithBothEndsNegative((float)-MovieConstants.MinConstantCExponent, end, imageNumber, numberOfImages);
            }

            if (absEnd < absStart/100f || absEnd < MovieConstants.MinConstantCExpoDifference)
            {
                // ignore end on negative side
                return GetExponentialConstantCValueWithBothEndsPositive(start, (float)MovieConstants.MinConstantCExponent, imageNumber, numberOfImages);
            }

            var oddNumberOfImages = (numberOfImages % 2) == 1;
            if (oddNumberOfImages)
            {
                // positive exponent
                if (imageNumber < numberOfImages / 2 + 1)
                    return GetExponentialConstantCValueWithBothEndsPositive(start, (float)MovieConstants.MinConstantCExponent, imageNumber, numberOfImages/2 + 1);

                if (imageNumber == numberOfImages / 2 + 1)
                    return 0f;

                var rightImageNumber = imageNumber - numberOfImages / 2;
                return GetExponentialConstantCValueWithBothEndsNegative((float)-MovieConstants.MinConstantCExponent, end, rightImageNumber, numberOfImages/2 + 1);
            }
            else
            {
                // positive exponent
                if (imageNumber <= numberOfImages / 2)  // this will include the zero
                    return GetExponentialConstantCValueWithBothEndsPositive(start, (float)MovieConstants.LowConstantCExponent, imageNumber, numberOfImages / 2);

                var rightImageNumber = imageNumber - numberOfImages / 2; 
                return GetExponentialConstantCValueWithBothEndsNegative((float)-MovieConstants.LowConstantCExponent, end, rightImageNumber, numberOfImages / 2);
            }
        }
    }

    public static float? GetExponentialConstantCImageValue(float start, float end, int imageNumber, int numberOfImages)
    {
        if (Math.Abs(end - start) < MovieConstants.MinConstantCExpoDifference)
            return null;

        if (start >= 0 && end >= 0)
            return GetExponentialConstantCValueWithBothEndsPositive(start, end,imageNumber, numberOfImages);

        if (start <= 0 && end <= 0)
            return GetExponentialConstantCValueWithBothEndsNegative(start, end, imageNumber, numberOfImages);

        return GetExponentialConstantCValueOverPositiveNegativeRange(start, end, imageNumber, numberOfImages);
    }

    public static float? GetLinearConstantCImageValue(float start, float end, int imageNumber, int numberOfImages)
    {
        if (Math.Abs(end - start) < MovieConstants.MinConstantCDifference)
            return null;

        if (end > start)
        {
            var dif = (end - start) / (numberOfImages-1);
            return start + (imageNumber - 1) * dif;
        }
        else
        {
            var dif = (start - end) / (numberOfImages-1);
            return start - (imageNumber - 1) * dif;
        }
    }

    private static FractalParams CalculateConstantCParams(FractalParams fractalParams, MovieParams movieParams,
        int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();
        if (movieParams.NumberOfImages < 2)
            return newFractalParams;

        var minDif = movieParams.DistributionType == DistributionTypes.Exponential
            ? MovieConstants.MinConstantCExpoDifference
            : MovieConstants.MinConstantCDifference;

        if (Math.Abs(movieParams.ConstantCEndW - movieParams.ConstantCStartW) < minDif &&
           Math.Abs(movieParams.ConstantCEndX - movieParams.ConstantCStartX) < minDif &&
           Math.Abs(movieParams.ConstantCEndY - movieParams.ConstantCStartY) < minDif &&
           Math.Abs(movieParams.ConstantCEndZ - movieParams.ConstantCStartZ) < minDif)
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

