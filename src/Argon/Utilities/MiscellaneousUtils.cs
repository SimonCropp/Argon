// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Text.RegularExpressions;
// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable RedundantSuppressNullableWarningExpression

static class MiscellaneousUtils
{
    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition, string? message = null) =>
        Debug.Assert(condition, message);

    public static bool ValueEquals(object? objA, object? objB)
    {
        if (objA == objB)
        {
            return true;
        }

        if (objA == null || objB == null)
        {
            return false;
        }

        // comparing an Int32 and Int64 both of the same value returns false
        // make types the same then compare
        if (objA.GetType() != objB.GetType())
        {
            if (ConvertUtils.IsInteger(objA) && ConvertUtils.IsInteger(objB))
            {
                return Convert.ToDecimal(objA, CultureInfo.CurrentCulture).Equals(Convert.ToDecimal(objB, CultureInfo.CurrentCulture));
            }

            if (objA is double or float or decimal && objB is double or float or decimal)
            {
                return MathUtils.ApproxEquals(Convert.ToDouble(objA, CultureInfo.CurrentCulture), Convert.ToDouble(objB, CultureInfo.CurrentCulture));
            }

            return false;
        }

        return objA.Equals(objB);
    }

    public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string paramName, object actualValue, string message)
    {
        var newMessage = $@"{message}{Environment.NewLine}Actual value was {actualValue}.";

        return new(paramName, newMessage);
    }

    public static string ToString(object? value)
    {
        if (value == null)
        {
            return "{null}";
        }

        if (value is string s)
        {
            return $@"""{s}""";
        }

        return value.ToString()!;
    }

    public static int ByteArrayCompare(byte[] a1, byte[] a2)
    {
        var lengthCompare = a1.Length.CompareTo(a2.Length);
        if (lengthCompare != 0)
        {
            return lengthCompare;
        }

        for (var i = 0; i < a1.Length; i++)
        {
            var valueCompare = a1[i].CompareTo(a2[i]);
            if (valueCompare != 0)
            {
                return valueCompare;
            }
        }

        return 0;
    }

    public static string? GetPrefix(string qualifiedName)
    {
        GetQualifiedNameParts(qualifiedName, out var prefix, out _);

        return prefix;
    }

    public static string GetLocalName(string qualifiedName)
    {
        GetQualifiedNameParts(qualifiedName, out _, out var localName);

        return localName;
    }

    public static void GetQualifiedNameParts(string qualifiedName, out string? prefix, out string localName)
    {
        var colonPosition = qualifiedName.IndexOf(':');

        if (colonPosition is -1 or 0 || qualifiedName.Length - 1 == colonPosition)
        {
            prefix = null;
            localName = qualifiedName;
        }
        else
        {
            prefix = qualifiedName.Substring(0, colonPosition);
            localName = qualifiedName.Substring(colonPosition + 1);
        }
    }

    internal static RegexOptions GetRegexOptions(string optionsText)
    {
        var options = RegexOptions.None;

        foreach (var ch in optionsText)
        {
            switch (ch)
            {
                case 'i':
                    options |= RegexOptions.IgnoreCase;
                    break;
                case 'm':
                    options |= RegexOptions.Multiline;
                    break;
                case 's':
                    options |= RegexOptions.Singleline;
                    break;
                case 'x':
                    options |= RegexOptions.ExplicitCapture;
                    break;
            }
        }

        return options;
    }
}