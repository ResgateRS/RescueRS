using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Apps4Business.Core.Extensions;

public static class StringExtensions
{
    public static decimal? TryParseDecimal(this string? decmaybe)
    {
        if (decimal.TryParse(decmaybe, out var result))
        {
            return result;
        }

        return null;
    }

    public static int? TryParseInt32(this string? intmaybe)
    {
        if (int.TryParse(intmaybe, out var result))
        {
            return result;
        }

        return null;
    }

    public static float? TryParseFloat(this string? val)
    {
        if (float.TryParse(val, out var result))
        {
            return result;
        }

        return null;
    }

    public static double? TryParseDouble(this string? val)
    {
        if (double.TryParse(val, out var result))
        {
            return result;
        }

        return null;
    }

    public static long? TryParseInt64(this string? value)
    {
        if (value == null)
        {
            return null;
        }

        if (long.TryParse(value, out var result))
        {
            return result;
        }

        return null;
    }
}