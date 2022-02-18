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
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;

namespace Argon.Tests.Utilities;

public class DynamicReflectionDelegateFactoryTests : TestFixtureBase
{
    [Fact]
    public void ConstructorWithInString()
    {
        var constructor = typeof(InTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 1);

        var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Value" };
        var o = (InTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Value", o.Value);
    }

    [Fact]
    public void ConstructorWithInStringAndBool()
    {
        var constructor = typeof(InTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 2);

        var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Value", true };
        var o = (InTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Value", o.Value);
        Assert.True( o.B1);
    }

    [Fact]
    public void ConstructorWithRefString()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 1);

        var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input" };
        var o = (OutAndRefTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndOutBool()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 2);

        var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input", false };
        var o = (OutAndRefTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Input", o.Input);
        Assert.True( o.B1);
    }

    [Fact]
    public void ConstructorWithRefStringAndRefBoolAndRefBool()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 3);

        var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input", true, null };
        var o = (OutAndRefTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Input", o.Input);
        Assert.True( o.B1);
        Assert.False( o.B2);
    }

    [Fact]
    public void CreateGetWithBadObjectTarget()
    {
        ExceptionAssert.Throws<InvalidCastException>(() =>
        {
            var p = new Person
            {
                Name = "Hi"
            };

            var setter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

            setter(p);
        }, "Unable to cast object of type 'Argon.Tests.TestObjects.Organization.Person' to type 'Argon.Tests.TestObjects.Movie'.");
    }

    [Fact]
    public void CreateSetWithBadObjectTarget()
    {
        ExceptionAssert.Throws<InvalidCastException>(() =>
        {
            var p = new Person();
            var m = new Movie();

            var setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

            setter(m, "Hi");

            Assert.AreEqual(m.Name, "Hi");

            setter(p, "Hi");

            Assert.AreEqual(p.Name, "Hi");
        }, "Unable to cast object of type 'Argon.Tests.TestObjects.Organization.Person' to type 'Argon.Tests.TestObjects.Movie'.");
    }

    [Fact]
    public void CreateSetWithBadTarget()
    {
        ExceptionAssert.Throws<InvalidCastException>(() =>
        {
            object structTest = new StructTest();

            var setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetProperty("StringProperty"));

            setter(structTest, "Hi");

            Assert.AreEqual("Hi", ((StructTest)structTest).StringProperty);

            setter(new TimeSpan(), "Hi");
        }, "Specified cast is not valid.");
    }

    [Fact]
    public void CreateSetWithBadObjectValue()
    {
        ExceptionAssert.Throws<InvalidCastException>(() =>
        {
            var m = new Movie();

            var setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

            setter(m, new Version("1.1.1.1"));
        }, "Unable to cast object of type 'System.Version' to type 'System.String'.");
    }

    [Fact]
    public void CreateStaticMethodCall()
    {
        var castMethodInfo = typeof(DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

        Xunit.Assert.NotNull(castMethodInfo);

        var call = DynamicReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

        var result = call(null, "First!");
        Xunit.Assert.NotNull(result);

        var key = (DictionaryKey)result;
        Assert.AreEqual("First!", key.Value);
    }

    [Fact]
    public void CreatePropertyGetter()
    {
        var namePropertyInfo = typeof(Person).GetProperty(nameof(Person.Name));

        Xunit.Assert.NotNull(namePropertyInfo);

        var call = DynamicReflectionDelegateFactory.Instance.CreateGet<Person>(namePropertyInfo);

        var p = new Person
        {
            Name = "Name!"
        };

        var result = call(p);
        Xunit.Assert.NotNull(result);

        Assert.AreEqual("Name!", (string)result);
    }

    [Fact]
    public void ConstructorStruct()
    {
        var creator1 = DynamicReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(MyStruct));
        var myStruct1 = (MyStruct)creator1.Invoke();
        Assert.AreEqual(0, myStruct1.IntProperty);

        var creator2 = DynamicReflectionDelegateFactory.Instance.CreateDefaultConstructor<MyStruct>(typeof(MyStruct));
        var myStruct2 = creator2.Invoke();
        Assert.AreEqual(0, myStruct2.IntProperty);
    }

    public struct TestStruct
    {
        public TestStruct(int i)
        {
            Value = i;
        }

        public int Value { get; }
    }

    public static TestStruct StructMethod(TestStruct s)
    {
        return new TestStruct(s.Value + s.Value);
    }

    [Fact]
    public void CreateStructMethodCall()
    {
        var methodInfo = typeof(DynamicReflectionDelegateFactoryTests).GetMethod(nameof(StructMethod), new[] { typeof(TestStruct) });

        Xunit.Assert.NotNull(methodInfo);

        var call = DynamicReflectionDelegateFactory.Instance.CreateMethodCall<object>(methodInfo);

        var result = call(null, new TestStruct(123));
        Xunit.Assert.NotNull(result);

        var s = (TestStruct)result;
        Assert.AreEqual(246, s.Value);
    }
}

#endif