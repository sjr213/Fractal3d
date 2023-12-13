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
        public bool LoopAngleX { get; set; }
        public float FromAngleY { get; set; }
        public float ToAngleY { get; set;}
        public bool LoopAngleY { get; set; }
        public float FromAngleZ { get; set;}
        public float ToAngleZ { get; set;}
        public bool LoopAngleZ { get; set; }

        public float StartBailout { get; set; } = 10;
        public float EndBailout { get; set; } = 10;

        public float ConstantCStartW { get; set; } = 0f;
        public float ConstantCEndW { get; set; } = 0f;
        public float ConstantCStartX { get; set; } = 0f;
        public float ConstantCEndX { get; set; } = 0f;
        public float ConstantCStartY { get; set; } = 0f;
        public float ConstantCEndY { get; set; } = 0f;
        public float ConstantCStartZ { get; set; } = 0f;
        public float ConstantCEndZ { get; set; } = 0f;

        public bool Alternate { get; set; } = false;
        public int StepsW { get; set; } = 5;
        public int StepsX { get; set; } = 5;
        public int StepsY { get; set; } = 5;
        public int StepsZ { get; set; } = 5;
    }
}
