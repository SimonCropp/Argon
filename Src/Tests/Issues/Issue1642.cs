#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !NET5_0_OR_GREATER
using System.Reflection.Emit;
using Xunit;

namespace Argon.Tests.Issues;

public class Issue1642 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var currentDomain = AppDomain.CurrentDomain;

        var aName = new AssemblyName("TempAssembly");
        var ab = currentDomain.DefineDynamicAssembly(
            aName, AssemblyBuilderAccess.RunAndSave);

        var mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

        var typeBuilder = mb.DefineType("TestEnum", TypeAttributes.NotPublic | TypeAttributes.Sealed, typeof(Enum));
        typeBuilder.DefineField("value__", typeof(int), FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

        var fieldBuilder = typeBuilder.DefineField("TestValue", typeBuilder, FieldAttributes.Family | FieldAttributes.Static | FieldAttributes.Literal);
        fieldBuilder.SetConstant(0);

        var enumType = typeBuilder.CreateType();

        var o = Activator.CreateInstance(enumType);

        var json = JsonConvert.SerializeObject(o, new JsonSerializerSettings { Converters = { new StringEnumConverter() } });
        Xunit.Assert.Equal(@"""TestValue""", json);
    }

}
#endif