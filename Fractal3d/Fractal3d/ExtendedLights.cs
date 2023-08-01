using ImageCalculator;
using System;
using System.Collections.Generic;

namespace Fractal3d
{
    [Serializable]
    internal class ExtendedLights
    {
        public ExtendedLights(List<Light> lights)
        {
            foreach (var light in lights)
                Lights.Add((Light)light.Clone());
        }

        public float NormalDistance { get; set; } = 0.01f;
        public float AmbientPower { get; set; } = 0.5f;
        public LightCombinationMode LightComboMode { get; set; } = LightCombinationMode.Average;
        public List<Light> Lights { get; set; } = new List<Light>();
    }
}
