// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NET5_0_OR_GREATER

public class ReflectionUtilsTests : TestFixtureBase
{
    [Fact]
    public void GetTypeNameSimpleForGenericTypes()
    {
        var typeName = typeof(IList<Type>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib", typeName);

        typeName = typeof(IDictionary<IList<Type>, IList<Type>>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IDictionary`2[[System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib],[System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib]], mscorlib", typeName);

        typeName = typeof(IList<>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IList`1, mscorlib", typeName);

        typeName = typeof(IDictionary<,>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IDictionary`2, mscorlib", typeName);
    }
}
#endif