using Microsoft.Win32;

namespace Fractal3d;

public static class RegistryUtil
{
    public const string AppKey = @"SOFTWARE\Fractal3d";
    public const string PalettePathKey = @"PalettePath";

    public static void WriteToRegistry(string subKey, string value)
    {
        RegistryKey key = Registry.CurrentUser.CreateSubKey(AppKey);
        key.SetValue(subKey, value);
        key.Close();
    }

    public static string ReadStringFromRegistry(string subKeyString)
    {
        var currentUser = Registry.CurrentUser;
        if (currentUser == null)
            return "";

        RegistryKey? key = currentUser.OpenSubKey(AppKey);
        if (key != null)
        {
            var subKey = key.GetValue(subKeyString);
            if (subKey == null)
                return "";

            var valString = subKey.ToString();
            return valString ?? "";
        }

        return "";
    }
}
