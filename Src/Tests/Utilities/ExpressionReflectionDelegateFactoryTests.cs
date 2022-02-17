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

using System.Linq;
using System;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Utilities;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;

namespace Argon.Tests.Utilities
{
    [TestFixture]
    public class ExpressionReflectionDelegateFactoryTests : TestFixtureBase
    {
        [Fact]
        public void ConstructorWithInString()
        {
            var constructor = TestReflectionUtils.GetConstructors(typeof(InTestClass)).Single(c => c.GetParameters().Count() == 1);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            var args = new object[] { "Value" };
            var o = (InTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Value", o.Value);
        }

        [Fact]
        public void ConstructorWithInStringAndBool()
        {
            var constructor = TestReflectionUtils.GetConstructors(typeof(InTestClass)).Single(c => c.GetParameters().Count() == 2);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            var args = new object[] { "Value", true };
            var o = (InTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Value", o.Value);
            Assert.AreEqual(true, o.B1);
        }

        [Fact]
        public void ConstructorWithRefString()
        {
            var constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 1);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            var args = new object[] { "Input" };
            var o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Fact]
        public void ConstructorWithRefStringAndOutBool()
        {
            var constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 2);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            var args = new object[] { "Input", null };
            var o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Fact]
        public void ConstructorWithRefStringAndRefBoolAndRefBool()
        {
            var constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 3);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            var args = new object[] { "Input", true, null };
            var o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
            Assert.AreEqual(true, o.B1);
            Assert.AreEqual(false, o.B2);
        }

        [Fact]
        public void DefaultConstructor()
        {
            var create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Movie));

            var m = (Movie)create();
            Assert.IsNotNull(m);
        }

        [Fact]
        public void DefaultConstructor_Struct()
        {
            var create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(StructTest));

            var m = (StructTest)create();
            Assert.IsNotNull(m);
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
            var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

            var m = new Movie();

            setter(m, "OH HAI!");

            Assert.AreEqual("OH HAI!", m.Name);
        }

        [Fact]
        public void CreatePropertyGetter()
        {
            var getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

            var m = new Movie();
            m.Name = "OH HAI!";

            var value = getter(m);

            Assert.AreEqual("OH HAI!", value);
        }

        [Fact]
        public void CreateMethodCall()
        {
            var method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(TestReflectionUtils.GetMethod(typeof(Movie), "ToString"));

            var m = new Movie();
            var result = method(m);
            Assert.AreEqual("Argon.Tests.TestObjects.Movie", result);

            method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(TestReflectionUtils.GetMethod(typeof(Movie), "Equals"));

            result = method(m, m);
            Assert.AreEqual(true, result);
        }

        [Fact]
        public void CreateMethodCall_Constructor()
        {
            var method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetConstructor(new Type[0]));

            var result = method(null);

            Assert.IsTrue(result is Movie);
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

            var getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetProperty(typeof(StaticTestClass), "StringProperty"));

            var v = getter(null);
            Assert.AreEqual(StaticTestClass.StringProperty, v);

            getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetField(typeof(StaticTestClass), "StringField"));

            v = getter(null);
            Assert.AreEqual(StaticTestClass.StringField, v);
        }

        [Fact]
        public void SetStatic()
        {
            var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(StaticTestClass), "StringProperty"));

            setter(null, "New property!");
            Assert.AreEqual("New property!", StaticTestClass.StringProperty);

            setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(StaticTestClass), "StringField"));

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

            var getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "StringField"));

            var value = getter(c);
            Assert.AreEqual("String!", value);

            getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "BoolField"));

            value = getter(c);
            Assert.AreEqual(true, value);
        }

        [Fact]
        public void CreateSetField_ReadOnly()
        {
            var c = new FieldsTestClass();

            var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "IntReadOnlyField"));

            setter(c, int.MinValue);
            Assert.AreEqual(int.MinValue, c.IntReadOnlyField);
        }

        [Fact]
        public void CreateSetField()
        {
            var c = new FieldsTestClass();

            var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "StringField"));

            setter(c, "String!");
            Assert.AreEqual("String!", c.StringField);

            setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "BoolField"));

            setter(c, true);
            Assert.AreEqual(true, c.BoolField);
        }

        [Fact]
        public void SetOnStruct()
        {
            object structTest = new StructTest();

            var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(StructTest), "StringProperty"));

            setter(structTest, "Hi1");
            Assert.AreEqual("Hi1", ((StructTest)structTest).StringProperty);

            setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(StructTest), "StringField"));

            setter(structTest, "Hi2");
            Assert.AreEqual("Hi2", ((StructTest)structTest).StringField);
        }

        [Fact]
        public void CreateGetWithBadObjectTarget()
        {
            ExceptionAssert.Throws<InvalidCastException>(
                () =>
                {
                    var p = new Person();
                    p.Name = "Hi";

                    var setter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

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

                    var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

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

                    var setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

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

            Assert.IsNotNull(castMethodInfo);

            var call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

            var result = call(null, "First!");
            Assert.IsNotNull(result);

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

            Assert.IsNotNull(methodInfo);

            var call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(methodInfo);

            var result = call(null, new TestStruct(123));
            Assert.IsNotNull(result);

            var s = (TestStruct)result;
            Assert.AreEqual(246, s.Value);
        }
    }
}
