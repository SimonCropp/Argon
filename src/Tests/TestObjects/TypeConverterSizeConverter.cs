// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

public class TypeConverterSizeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        var str = value as string;
        if (str == null)
        {
            return base.ConvertFrom(context, culture, value);
        }
        var str2 = str.Trim();
        if (str2.Length == 0)
        {
            return null;
        }
        //TODO: debug this
        if (culture == null)
        {
            culture = CultureInfo.CurrentCulture;
        }
        var strArray = str2.Split(',');
        var numArray = new int[strArray.Length];
        var converter = TypeDescriptor.GetConverter(typeof(int));
        for (var i = 0; i < numArray.Length; i++)
        {
            numArray[i] = (int)converter.ConvertFromString(context, culture, strArray[i]);
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
                culture ??= CultureInfo.CurrentCulture;
                var converter = TypeDescriptor.GetConverter(typeof(int));
                var strArray = new string[2];
                var num = 0;
                strArray[num++] = converter.ConvertToString(context, culture, size.Width);
                strArray[num++] = converter.ConvertToString(context, culture, size.Height);
                return string.Join(", ", strArray);
            }
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
}