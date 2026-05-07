using System;
using System.Globalization;

public static class DateUtility
{
    private static readonly string[] SupportedFormats =
    {
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "yyyy/MM/dd"
    };

    public static bool TryParseFlexibleDate(string dateText, out DateTime dateTime)
    {
        dateTime = default(DateTime);

        if (string.IsNullOrWhiteSpace(dateText))
        {
            return false;
        }

        return DateTime.TryParseExact(
                   dateText.Trim(),
                   SupportedFormats,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                   out dateTime)
               || DateTime.TryParse(dateText.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTime)
               || DateTime.TryParse(dateText.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateTime);
    }

    public static DateTime ResolveReferenceDate(string referenceDateText)
    {
        if (TryParseFlexibleDate(referenceDateText, out var dateTime))
        {
            return dateTime.Date;
        }

        return DateTime.Now.Date;
    }
}
