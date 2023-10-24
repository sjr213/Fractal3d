namespace Fractal3d;

using System;

public enum MovieChangeType { ImageCountChange }

public class MovieChangedEventArgs : EventArgs
{
    public MovieChangeType ChangeType { get; set; }
}

