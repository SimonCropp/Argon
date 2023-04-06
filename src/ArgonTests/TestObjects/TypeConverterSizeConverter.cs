// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

public class TypeConverterSizeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is not string stringValue)
        {
            return base.ConvertFrom(context, InvariantCulture, value);
        }

        var trimmed = stringValue.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        var strArray = trimmed.Split(',');
        var numArray = new int[strArray.Length];
        var converter = TypeDescriptor.GetConverter(typeof(int));
        for (var i = 0; i < numArray.Length; i++)
        {
            numArray[i] = (int) converter.ConvertFromString(context, InvariantCulture, strArray[i]);
        }

        if (numArray.Length == 2)
        {
            return new TypeConverterSize(numArray[0], numArray[1]);
        }

        throw new ArgumentException("Bad format.");
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value is TypeConverterSize size)
        {
            if (destinationType == typeof(string))
            {
                var converter = TypeDescriptor.GetConverter(typeof(int));
                var strArray = new string[2];
                var num = 0;
                strArray[num++] = converter.ConvertToString(context, InvariantCulture, size.Width);
                strArray[num++] = converter.ConvertToString(context, InvariantCulture, size.Height);
                return string.Join(", ", strArray);
            }
        }
        return base.ConvertTo(context, InvariantCulture, value, destinationType);
    }
}