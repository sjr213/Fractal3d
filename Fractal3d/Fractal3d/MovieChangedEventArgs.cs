namespace Fractal3d;

using System;

public enum MovieChangeType { ImageCountChange, CurrentImageChanged }

public class MovieChangedEventArgs : EventArgs
{
    public MovieChangeType ChangeType { get; set; }
    public int CurrentImageIndex { get; set; }
}

