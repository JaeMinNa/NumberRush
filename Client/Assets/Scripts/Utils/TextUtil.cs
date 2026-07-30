using System;

public static class TextUtil
{
    private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

    public static string ConvertKMB(long value, bool showDecimal = false)
    {
        int suffixIndex = 0;
        double decimalValue = value;
        while (decimalValue >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            decimalValue /= 1000;
            suffixIndex++;
        }

        return showDecimal
            ? $"{Math.Floor(decimalValue * 1000) / 1000:0.###}{Suffixes[suffixIndex]}"
            : $"{Math.Floor(decimalValue):0}{Suffixes[suffixIndex]}";
    }

    public static string AddComma_3digits(long value)
    {
        return value.ToString("#,##0");
    }

    // float를 00:00.0 초 형태로 나타내기
    public static string ConvertTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string formatted = string.Format("{0:00}:{1:00}.{2:0}",
                                          timeSpan.Minutes,
                                          timeSpan.Seconds,
                                          timeSpan.Milliseconds / 100);

        return formatted;
    }
}