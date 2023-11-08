namespace ImageCalculator.Movie
{
    [Serializable]
    public record MovieParams
    {
        public int CurrentImage { get; set; } = 1;
        public int NumberOfImages { get; set; } = 20;
        public int FramesPerSecond { get; set; } = 5;
        public DistributionTypes DistributionType { get; set; } = DistributionTypes.Linear;

        public MovieTypes MovieType { get; set; } = MovieTypes.Angles;

        public float FromAngleX { get; set; }
        public float ToAngleX { get; set; }
        public float FromAngleY { get; set; }
        public float ToAngleY { get; set;}
        public float FromAngleZ { get; set;}
        public float ToAngleZ { get; set;}

        public float StartBailout { get; set; }
        public float EndBailout { get; set; }
    }
}
