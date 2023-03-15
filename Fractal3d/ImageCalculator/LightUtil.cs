namespace ImageCalculator;

using System.Numerics;

public static class LightUtil
{
    public static Lighting GetPointLight(PointLight light, Vector3 pos3D, Vector3 viewPos, Vector3 normal)
    {
        Lighting lighting = new Lighting();
        if (light.DiffusePower <= 0)
            return lighting;

        Vector3 lightDir = light.Position - pos3D; //3D position in space of the surface
        float distance = lightDir.Length();
        lightDir /= distance; // = normalize(lightDir);
        distance *= distance; //This line may be optimized using Inverse square root

        //Intensity of the diffuse light. Saturate to keep within the 0-1 range.
        float ndotL = Vector3.Dot(normal, lightDir);
        float intensity = Math.Min(Math.Max(ndotL, 0f), 1.0f);

        // up to here * note we need to multiply both colors by the light.Color too

        // Calculate the diffuse light factoring in light color, power and the attenuation
        var diffuse = intensity * light.DiffuseColor * light.DiffusePower / distance;
        lighting.Diffuse = Vector3.Clamp(diffuse, Vector3.Zero, Vector3.One);

        Vector3 viewDir = viewPos - pos3D;

        if (light.LightingType == LightingType.BlinnPhong)
        {
            // Calculate the half vector between the light vector and the view vector.
            // This is typically slower than calculating the actual reflection vector
            // due to the normalize function's reciprocal square root
            Vector3 h = Vector3.Normalize(lightDir + viewDir);

            //Intensity of the specular light
            float ndotH = Vector3.Dot(normal, h);
            intensity = (float)Math.Pow(Math.Min(Math.Max(ndotH, 0f), 1.0f), light.Shininess);
        }
        else if (light.LightingType == LightingType.Phong)
        {
            Vector3 reflectDir = Vector3.Reflect(-lightDir, normal);
            float specAngle = Math.Max(Vector3.Dot(reflectDir, viewDir), 0.0f);
            intensity = (float)Math.Pow(specAngle, light.Shininess / 4.0f);
        }

        //Sum up the specular light factoring
        var specular = intensity * light.SpecularColor * light.SpecularPower / distance;
        lighting.Specular = Vector3.Clamp(specular, Vector3.Zero, Vector3.One);

        return lighting;
    }

    public static Lighting GetPointLight(List<PointLight> lights, LightCombinationMode lightComboMode, Vector3 pos3D, Vector3 viewPos, Vector3 normal)
    {
        Lighting lighting = new Lighting();

        int numberOfLights = lights.Count;
        foreach (var light in lights)
        {
            var singleLight = GetPointLight(light, pos3D, viewPos, normal);
            if (lightComboMode == LightCombinationMode.Average)
            {
                lighting.Diffuse += singleLight.Diffuse / numberOfLights;
                lighting.Specular += singleLight.Specular / numberOfLights;
            }
            else
            {
                lighting.Diffuse += singleLight.Diffuse;
                lighting.Specular += singleLight.Specular;
            }
        }

        lighting.Diffuse = Vector3.Clamp(lighting.Diffuse, Vector3.Zero, Vector3.One);
        lighting.Specular = Vector3.Clamp(lighting.Specular, Vector3.Zero, Vector3.One);

        return lighting;
    }
}
