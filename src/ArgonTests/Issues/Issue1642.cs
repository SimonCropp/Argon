// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NET5_0_OR_GREATER
using System.Reflection.Emit;

public class Issue1642 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var currentDomain = AppDomain.CurrentDomain;

        var aName = new AssemblyName("TempAssembly");
        var ab = currentDomain.DefineDynamicAssembly(
            aName, AssemblyBuilderAccess.RunAndSave);

        var mb = ab.DefineDynamicModule(aName.Name, $"{aName.Name}.dll");

        var typeBuilder = mb.DefineType("TestEnum", TypeAttributes.NotPublic | TypeAttributes.Sealed, typeof(Enum));
        typeBuilder.DefineField("value__", typeof(int), FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

        var fieldBuilder = typeBuilder.DefineField("TestValue", typeBuilder, FieldAttributes.Family | FieldAttributes.Static | FieldAttributes.Literal);
        fieldBuilder.SetConstant(0);

        var enumType = typeBuilder.CreateType();

        var o = Activator.CreateInstance(enumType);

        var json = JsonConvert.SerializeObject(o, new JsonSerializerSettings { Converters = { new StringEnumConverter() } });
        Assert.Equal(
            """
            "TestValue"
            """,
            json);
    }

}
#endif