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

using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;

namespace Argon.Tests.Utilities;

public class ExpressionReflectionDelegateFactoryTests : TestFixtureBase
{
    [Fact]
    public void ConstructorWithInString()
    {
        var constructor = typeof(InTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 1);

        var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Value" };
        var o = (InTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Value", o.Value);
    }

    [Fact]
    public void ConstructorWithInStringAndBool()
    {
        var constructor = typeof(InTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 2);

        var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

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

        var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input" };
        var o = (OutAndRefTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndOutBool()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 2);

        var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input", null };
        var o = (OutAndRefTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndRefBoolAndRefBool()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 3);

        var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input", true, null };
        var o = (OutAndRefTestClass)creator(args);
        Xunit.Assert.NotNull(o);
        Assert.AreEqual("Input", o.Input);
        Assert.True( o.B1);
        Assert.False( o.B2);
    }

    [Fact]
    public void DefaultConstructor()
    {
        var create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Movie));

        var m = (Movie)create();
        Xunit.Assert.NotNull(m);
    }

    [Fact]
    public void DefaultConstructor_Struct()
    {
        var create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(StructTest));

        var m = (StructTest)create();
        Xunit.Assert.NotNull(m);
    }

    [Fact]
    public void DefaultConstructor_Abstract()
    {
        ExceptionAssert.Throws<Exception>(
            () =>
            {
                var create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Type));

                create();
            }, new[]
            {
                "Cannot create an abstract class.",
                "Cannot create an abstract class 'System.Type'." // mono
            });
    }

    [Fact]
    public void CreatePropertySetter()
    {
        var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

        var m = new Movie();

        setter(m, "OH HAI!");

        Assert.AreEqual("OH HAI!", m.Name);
    }

    [Fact]
    public void CreatePropertyGetter()
    {
        var getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

        var m = new Movie
        {
            Name = "OH HAI!"
        };

        var value = getter(m);

        Assert.AreEqual("OH HAI!", value);
    }

    [Fact]
    public void CreateMethodCall()
    {
        var method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetMethod("ToString"));

        var m = new Movie();
        var result = method(m);
        Assert.AreEqual("Argon.Tests.TestObjects.Movie", result);

        method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetMethod("Equals"));

        result = method(m, m);
        Assert.True( result);
    }

    [Fact]
    public void CreateMethodCall_Constructor()
    {
        var method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetConstructor(new Type[0]));

        var result = method(null);

        Xunit.Assert.True(result is Movie);
    }

    public static class StaticTestClass
    {
        public static string StringField;
        public static string StringProperty { get; set; }
    }

    [Fact]
    public void GetStatic()
    {
        StaticTestClass.StringField = "Field!";
        StaticTestClass.StringProperty = "Property!";

        var getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(StaticTestClass).GetProperty("StringProperty"));

        var v = getter(null);
        Assert.AreEqual(StaticTestClass.StringProperty, v);

        getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(StaticTestClass).GetField("StringField"));

        v = getter(null);
        Assert.AreEqual(StaticTestClass.StringField, v);
    }

    [Fact]
    public void SetStatic()
    {
        var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StaticTestClass).GetProperty("StringProperty"));

        setter(null, "New property!");
        Assert.AreEqual("New property!", StaticTestClass.StringProperty);

        setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StaticTestClass).GetField("StringField"));

        setter(null, "New field!");
        Assert.AreEqual("New field!", StaticTestClass.StringField);
    }

    public class FieldsTestClass
    {
        public string StringField;
        public bool BoolField;

        public readonly int IntReadOnlyField = int.MaxValue;
    }

    [Fact]
    public void CreateGetField()
    {
        var c = new FieldsTestClass
        {
            BoolField = true,
            StringField = "String!"
        };

        var getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(FieldsTestClass).GetField("StringField"));

        var value = getter(c);
        Assert.AreEqual("String!", value);

        getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(FieldsTestClass).GetField("BoolField"));

        value = getter(c);
        Assert.True( value);
    }

    [Fact]
    public void CreateSetField_ReadOnly()
    {
        var c = new FieldsTestClass();

        var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(FieldsTestClass).GetField("IntReadOnlyField"));

        setter(c, int.MinValue);
        Assert.AreEqual(int.MinValue, c.IntReadOnlyField);
    }

    [Fact]
    public void CreateSetField()
    {
        var c = new FieldsTestClass();

        var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(FieldsTestClass).GetField("StringField"));

        setter(c, "String!");
        Assert.AreEqual("String!", c.StringField);

        setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(FieldsTestClass).GetField("BoolField"));

        setter(c, true);
        Assert.True( c.BoolField);
    }

    [Fact]
    public void SetOnStruct()
    {
        object structTest = new StructTest();

        var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetProperty("StringProperty"));

        setter(structTest, "Hi1");
        Assert.AreEqual("Hi1", ((StructTest)structTest).StringProperty);

        setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetField("StringField"));

        setter(structTest, "Hi2");
        Assert.AreEqual("Hi2", ((StructTest)structTest).StringField);
    }

    [Fact]
    public void CreateGetWithBadObjectTarget()
    {
        ExceptionAssert.Throws<InvalidCastException>(
            () =>
            {
                var p = new Person
                {
                    Name = "Hi"
                };

                var setter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

                setter(p);
            },
            new[]
            {
                "Unable to cast object of type 'Argon.Tests.TestObjects.Organization.Person' to type 'Argon.Tests.TestObjects.Movie'.",
                "Cannot cast from source type to destination type." // mono
            });
    }

    [Fact]
    public void CreateSetWithBadObjectTarget()
    {
        ExceptionAssert.Throws<InvalidCastException>(
            () =>
            {
                var p = new Person();
                var m = new Movie();

                var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

                setter(m, "Hi");

                Assert.AreEqual(m.Name, "Hi");

                setter(p, "Hi");

                Assert.AreEqual(p.Name, "Hi");
            },
            new[]
            {
                "Unable to cast object of type 'Argon.Tests.TestObjects.Organization.Person' to type 'Argon.Tests.TestObjects.Movie'.",
                "Cannot cast from source type to destination type." // mono
            });
    }

    [Fact]
    public void CreateSetWithBadObjectValue()
    {
        ExceptionAssert.Throws<InvalidCastException>(
            () =>
            {
                var m = new Movie();

                var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

                setter(m, new Version("1.1.1.1"));
            }, new[]
            {
                "Unable to cast object of type 'System.Version' to type 'System.String'.",
                "Cannot cast from source type to destination type." //mono
            });
    }

    [Fact]
    public void CreateStaticMethodCall()
    {
        var castMethodInfo = typeof(DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

        Xunit.Assert.NotNull(castMethodInfo);

        var call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

        var result = call(null, "First!");
        Xunit.Assert.NotNull(result);

        var key = (DictionaryKey)result;
        Assert.AreEqual("First!", key.Value);
    }

    [Fact]
    public void ConstructorStruct()
    {
        var creator1 = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(MyStruct));
        var myStruct1 = (MyStruct)creator1.Invoke();
        Assert.AreEqual(0, myStruct1.IntProperty);

        var creator2 = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<MyStruct>(typeof(MyStruct));
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
        var methodInfo = typeof(ExpressionReflectionDelegateFactoryTests).GetMethod(nameof(StructMethod), new[] { typeof(TestStruct) });

        Xunit.Assert.NotNull(methodInfo);

        var call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(methodInfo);

        var result = call(null, new TestStruct(123));
        Xunit.Assert.NotNull(result);

        var s = (TestStruct)result;
        Assert.AreEqual(246, s.Value);
    }
}