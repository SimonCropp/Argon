// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Microsoft.FSharp.Core;

namespace TestObjects;

[Serializable, DebuggerDisplay("{__DebugDisplay(),nq}"), CompilationMapping(SourceConstructFlags.SumType)]
public class Currency
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal readonly int _tag;

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Currency _unique_AUD = new(1);

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Currency _unique_EUR = new(4);

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Currency _unique_JPY = new(5);

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Currency _unique_LocalCurrency = new(0);

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Currency _unique_NZD = new(2);

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Currency _unique_USD = new(3);

    [CompilerGenerated, DebuggerNonUserCode]
    internal Currency(int _tag)
    {
        this._tag = _tag;
    }

    [CompilerGenerated, DebuggerNonUserCode]
    internal object __DebugDisplay()
    {
        return ExtraTopLevelOperators.PrintFormatToString(new PrintfFormat<FSharpFunc<Currency, string>, Unit, string, string, string>("%+0.8A")).Invoke(this);
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Currency AUD
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 1)]
        get => _unique_AUD;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Currency EUR
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 4)]
        get => _unique_EUR;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsAUD
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => Tag == 1;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsEUR
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => Tag == 4;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsJPY
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => Tag == 5;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsLocalCurrency
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => Tag == 0;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsNZD
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => Tag == 2;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsUSD
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => Tag == 3;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Currency JPY
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 5)]
        get => _unique_JPY;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Currency LocalCurrency
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 0)]
        get => _unique_LocalCurrency;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Currency NZD
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 2)]
        get => _unique_NZD;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Tag
    {
        [CompilerGenerated, DebuggerNonUserCode]
        get => _tag;
    }

    [CompilerGenerated, DebuggerNonUserCode, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Currency USD
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 3)]
        get => _unique_USD;
    }
}