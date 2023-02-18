namespace BasicWpfLibrary;

using System;
using System.ComponentModel;

public static class EnumHelper
{
    public static string GetDescription<T>(this T enumValue)
        where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            return string.Empty;

        var description = enumValue.ToString();
        var name = enumValue.ToString();
        if (name == null)
            return string.Empty;
        var fieldInfo = enumValue.GetType().GetField(name);

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if ( attrs.Length > 0)
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return description ?? string.Empty;
    }

}


