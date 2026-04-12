using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalMusicPlayer.Services.OnlineLyrics.Helpers;

public static class StringHelper
{
    public static bool IsSame(this string str1, string str2)
    {
        if (str1 == str2) return true;
        if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2)) return true;
        return false;
    }

    public static bool IsSameWhiteSpace(this string str1, string str2)
    {
        if (str1 == str2) return true;
        if (string.IsNullOrWhiteSpace(str1) && string.IsNullOrWhiteSpace(str2)) return true;
        return false;
    }

    public static bool IsSameTrim(this string str1, string str2)
    {
        str1 = (str1 ?? string.Empty).Trim();
        str2 = (str2 ?? string.Empty).Trim();
        if (str1 == str2) return true;
        if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2)) return true;
        return false;
    }

    public static double ComputeTextSame(string textX, string textY, bool isCase = false)
    {
        if (textX.Length <= 0 || textY.Length <= 0)
        {
            return 0;
        }
        if (!isCase)
        {
            textX = textX.ToLower();
            textY = textY.ToLower();
        }
        int[,] dp = new int[Math.Max(textX.Length, textY.Length) + 1, Math.Max(textX.Length, textY.Length) + 1];
        for (int x = 0; x < textX.Length; x++)
        {
            for (int y = 0; y < textY.Length; y++)
            {
                if (textX[x] == textY[y])
                {
                    dp[x + 1, y + 1] = dp[x, y] + 1;
                }
                else
                {
                    dp[x + 1, y + 1] = Math.Max(dp[x, y + 1], dp[x + 1, y]);
                }
            }
        }
        return Math.Round((double)dp[textX.Length, textY.Length] / Math.Max(textX.Length, textY.Length) * 100, 2);
    }

    public static string RemoveDuoSpaces(this string str)
    {
        while (str.Contains("  "))
        {
            str = str.Replace("  ", " ");
        }
        return str;
    }

    public static string? RemoveDuoBackslashN(this string str)
    {
        if (str == null) return null;
        while (str.Contains("\n\n"))
        {
            str = str.Replace("\n\n", "\n");
        }
        return str;
    }

    public static string? RemoveBackslashR(this string str)
    {
        if (str == null) return null;
        str = str.Replace("\r", "");
        return str;
    }

    public static string FormatTimeMsToTimestampString(float time, bool millisecond = true)
    {
        if (time < 0)
        {
            return "0:00";
        }
        int second;
        int minute = 0;
        second = (int)time / 1000;
        if (second >= 60)
        {
            minute = second / 60;
            second %= 60;
        }
        return millisecond
            ? string.Format("{0:D2}:{1:D2}.{2:D3}", minute, second, (int)time % 1000)
            : string.Format("{0:D1}:{1:D2}", minute, second);
    }

    public static string? Between(this string str, string start, string end)
    {
        if (str.IndexOf(start) != -1)
        {
            str = str[(str.IndexOf(start) + start.Length)..];
            int _end = str.IndexOf(end);
            if (_end != -1)
            {
                str = str[.._end];
            }
            return str;
        }
        else
        {
            return null;
        }
    }

    public static string? Reverse(this string str)
    {
        if (str == null) return null;

        char[] arr = str.ToCharArray();
        Array.Reverse(arr);
        string reverse = new(arr);
        return reverse;
    }

    public static string? Remove(this string str, string substring)
    {
        if (str == null) return null;

        return str.Replace(substring, "");
    }

    public static string RemoveControlChars(this string value, params char[] excludeChars)
    {
        char[] excludeChars2 = excludeChars;
        if (value == null)
            throw new ArgumentNullException("value");

        excludeChars2 ??= Array.Empty<char>();

        return new string(value.Where((char c) => !char.IsControl(c) || excludeChars2.Contains(c)).ToArray());
    }

    public static string FixIWords(this string str)
    {
        str = str.Replace(" i ", " I ")
                 .Replace("i'd ", "I'd ")
                 .Replace("i'm", "I'm")
                 .Replace("i'll", "I'll")
                 .Replace("i've", "I've");
        return str;
    }

    public static string? RemoveFrontBackBrackets(this string str)
    {
        if (str == null) return null;

        str = str.Trim();
        if (str.Length == 0) return str;
        if (str[0] == '(' || str[0] == '（') str = str[1..];
        if (str[^1] == ')' || str[^1] == '）') str = str[..^1];
        return str.Trim();
    }

    public static bool CanStartNewLine(this string str)
    {
        if (str.EndsWith(' ') || str.EndsWith(',') || str.EndsWith('/'))
            return true;
        return false;
    }

    public static bool Contains(this string str, List<string> list)
    {
        foreach (var item in list)
        {
            if (str.Contains(item)) return true;
        }
        return false;
    }

    public static bool IsNumber(this string str)
    {
        return Regex.IsMatch(str, @"^\d+$", RegexOptions.Compiled);
    }

    public static bool HasCJK(this string str, bool includeColon = false)
    {
        if (includeColon && str.Contains('：')) return true;
        var symbolRegex = new Regex("[`~!@#$%^&*()+=|{}':;',\\[\\].<>/?~！@#￥%……&*（）——+|{}《》【】‘；：\"”“'。，、？]");
        str = symbolRegex.Replace(str, "");
        var cjkRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}|^[\u0800-\u4e00]|[\u4e00-\u9fa5]|[\uac00-\ud7ff]");
        var k = cjkRegex.Matches(str);
        return k.Count > 0;
    }

    public static bool IsCJK(this string str, bool includeColon = false)
    {
        if (includeColon && str == "：") return true;
        var cjkRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        return cjkRegex.IsMatch(str);
    }

    public static string OptimizeCJK(this string str)
    {
        StringBuilder text = new();

        var cjkRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        var symbolRegex = new Regex("[`~!@#$%^&*()+=|{}':;',\\[\\].<>/?~！@#￥%……&*（）——+|{}《》【】‘；：\"”“'。，、？]");

        bool isCJKPrev = false;
        bool isSymbolPrev = false;
        foreach (char ch in str)
        {
            var chStr = ch.ToString();

            bool isSymbol = symbolRegex.IsMatch(chStr);
            bool isCJK = cjkRegex.IsMatch(chStr);

            if (isCJK != isCJKPrev && !isSymbol && !isSymbolPrev)
            {
                _ = text.Append(' ');
            }

            _ = text.Append(ch);
            isCJKPrev = isCJK;
            isSymbolPrev = isSymbol;
        }
        return text.ToString().Replace("  ", " ").Trim();
    }

    public static bool IsChinese(this char ch)
    {
        return ch >= '\u4e00' && ch <= '\u9fff';
    }

    public static bool HasChinese(this string str)
    {
        if (str == null) return false;
        return Regex.IsMatch(str, "[\\u4e00-\\u9fff]");
    }

    public static double ChinesePercentage(this string text)
    {
        int count = 0;
        int spareCount = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i].IsChinese())
            {
                count++;
            }
            else if (text[i] == '\n' || text[i] == '\r' || text[i] == '\t')
            {
                spareCount++;
            }
        }
        return text.Length - spareCount > 0 ? (double)count / (text.Length - spareCount) : 0;
    }

    public static double TraditionalChineseConfidence(string text)
    {
        int n = 0, total = 0;
        string sc = text.ToSC();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i].IsChinese())
            {
                total++;
                if (text[i] != sc[i])
                {
                    n++;
                }
            }
        }
        return (double)n / total;
    }
}
