// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Microsoft.FSharp.Core;

namespace TestObjects;

[Serializable, DebuggerDisplay("{__DebugDisplay(),nq}"), CompilationMapping(SourceConstructFlags.SumType)]
public class Shape
{
    // Fields
    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal readonly int _tag;

    [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
    internal static readonly Shape _unique_Empty;

    static Shape()
    {
        _unique_Empty = new Shape(3);
    }

    [CompilerGenerated]
    internal Shape(int _tag)
    {
        this._tag = _tag;
    }

    [CompilationMapping(SourceConstructFlags.UnionCase, 1)]
    public static Shape NewCircle(double _radius)
    {
        return new Circle(_radius);
    }

    [CompilationMapping(SourceConstructFlags.UnionCase, 2)]
    public static Shape NewPrism(double _width, double item2, double _height)
    {
        return new Prism(_width, item2, _height);
    }

    [CompilationMapping(SourceConstructFlags.UnionCase, 0)]
    public static Shape NewRectangle(double _width, double _length)
    {
        return new Rectangle(_width, _length);
    }

    [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public static Shape Empty
    {
        [CompilationMapping(SourceConstructFlags.UnionCase, 3)]
        get => _unique_Empty;
    }

    [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsCircle
    {
        [CompilerGenerated]
        get => Tag == 1;
    }

    [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsEmpty
    {
        [CompilerGenerated]
        get => Tag == 3;
    }

    [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsPrism
    {
        [CompilerGenerated]
        get => Tag == 2;
    }

    [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsRectangle
    {
        [CompilerGenerated]
        get => Tag == 0;
    }

    [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Tag
    {
        [CompilerGenerated]
        get => _tag;
    }

    [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}")]
    public class Circle : Shape
    {
        // Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly double _radius;

        // Methods
        [CompilerGenerated, DebuggerNonUserCode]
        internal Circle(double _radius) : base(1)
        {
            this._radius = _radius;
        }

        // Properties
        [CompilationMapping(SourceConstructFlags.Field, 1, 0), CompilerGenerated, DebuggerNonUserCode]
        public double radius
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get => _radius;
        }
    }

    [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}")]
    public class Prism : Shape
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly double _height;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly double _width;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly double item2;

        [CompilerGenerated, DebuggerNonUserCode]
        internal Prism(double _width, double item2, double _height) : base(2)
        {
            this._width = _width;
            this.item2 = item2;
            this._height = _height;
        }

        [CompilationMapping(SourceConstructFlags.Field, 2, 2), CompilerGenerated, DebuggerNonUserCode]
        public double height
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get => _height;
        }

        [CompilationMapping(SourceConstructFlags.Field, 2, 1), CompilerGenerated, DebuggerNonUserCode]
        public double Item2
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get => item2;
        }

        [CompilationMapping(SourceConstructFlags.Field, 2, 0), CompilerGenerated, DebuggerNonUserCode]
        public double width
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get => _width;
        }
    }

    [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}")]
    public class Rectangle : Shape
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly double _length;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly double _width;

        [CompilerGenerated, DebuggerNonUserCode]
        internal Rectangle(double _width, double _length) : base(0)
        {
            this._width = _width;
            this._length = _length;
        }

        [CompilationMapping(SourceConstructFlags.Field, 0, 1), CompilerGenerated, DebuggerNonUserCode]
        public double length
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get => _length;
        }

        [CompilationMapping(SourceConstructFlags.Field, 0, 0), CompilerGenerated, DebuggerNonUserCode]
        public double width
        {
            [CompilerGenerated, DebuggerNonUserCode]
            get => _width;
        }
    }
}