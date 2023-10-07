namespace ImageCalculator.Movie;

public static class FractalParamCalculator
{
    public static FractalParams CalculateFractalParams(FractalParams fractalParams, MovieParams movieParams, int imageNumber)
    {
        var newFractalParams = (FractalParams)fractalParams.Clone();

        if (movieParams.MovieType == MovieTypes.Angles)
            return CalculateFractalAngleParams(fractalParams, movieParams, imageNumber);

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
}

