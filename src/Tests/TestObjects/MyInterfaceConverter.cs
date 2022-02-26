// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

class MyInterfaceConverter : TypeConverter
{
    readonly List<IMyInterface> _writers = new()
    {
        new ConsoleWriter(),
        new TraceWriter()
    };

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        return destinationType == typeof(string);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value == null)
        {
            return null;
        }

        return (from w in _writers where w.Name == value.ToString() select w).FirstOrDefault();
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
        Type destinationType)
    {
        if (value == null)
        {
            return null;
        }
        return ((IMyInterface)value).Name;
    }
}