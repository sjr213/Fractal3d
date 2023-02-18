namespace BasicWpfLibrary.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    using System.ComponentModel;
    using System.Reflection;

    public static class EnumExtensions
    {
        public static string GetFriendlyDescription(this Enum enumValue) =>
            TryGetDescriptionAttribute(enumValue)?.Description ?? enumValue.ToString();

        public static DescriptionAttribute TryGetDescriptionAttribute(this Enum enumValue)
        {
            return enumValue.GetType().GetField(enumValue.ToString())
                ?.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
        }
    }

    [MarkupExtensionReturnType(typeof(EnumToDescriptionConverter))]
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToDescriptionConverter : MarkupExtension, IValueConverter
    {
        public bool ConvertToLower { get; set; } = false;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Enum @enum)
                return DependencyProperty.UnsetValue;
            var description = @enum.GetFriendlyDescription();
            return ConvertToLower ? description.ToLower() : description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!targetType.IsEnum)
                return DependencyProperty.UnsetValue;

            var name = value as string;

            if(name == null)
                return DependencyProperty.UnsetValue;

            Enum? matchingNameEnum = null;

            foreach (var @enum in Enum.GetValues(targetType).Cast<Enum>())
            {
                var descriptionAttribute = @enum.TryGetDescriptionAttribute();
                if (descriptionAttribute is not null)
                {
                    if (DoStringsMatch(descriptionAttribute.Description, name))
                        return @enum;
                    continue;
                }

                if (DoStringsMatch(@enum.ToString(), name))
                    matchingNameEnum = @enum;
            }

            return matchingNameEnum ?? DependencyProperty.UnsetValue;

            bool DoStringsMatch(string l, string r) =>
                l.Equals(r, ConvertToLower ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
    }
}
