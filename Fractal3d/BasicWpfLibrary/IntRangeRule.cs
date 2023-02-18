namespace BasicWpfLibrary;

using System;
using System.Windows.Controls;
using System.Globalization;

public class IntRangeRule : ValidationRule
{
    public int Min { get; set; }
    public int Max { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        int val = 0;

        try
        {
            if (((string)value).Length > 0)
                val = int.Parse((string)value);
        }
        catch (Exception e)
        {
            return new ValidationResult(false, $"Illegal characters or {e.Message}");
        }

        if ((val < Min) || (val > Max))
        {
            return new ValidationResult(false,
                $"Please enter an value in the range: {Min}-{Max}.");
        }
        return ValidationResult.ValidResult;
    }
}
