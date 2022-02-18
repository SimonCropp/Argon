using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Utilities;

public class OutAndRefTestClass
{
    public string Input { get; set; }
    public bool B1 { get; set; }
    public bool B2 { get; set; }

    public OutAndRefTestClass(ref string value)
    {
        Input = value;
        value = "Output";
    }

    public OutAndRefTestClass(ref string value, out bool b1)
        : this(ref value)
    {
        b1 = true;
        B1 = true;
    }

    public OutAndRefTestClass(ref string value, ref bool b1, ref bool b2)
        : this(ref value)
    {
        B1 = b1;
        B2 = b2;
    }
}

public class InTestClass
{
    public string Value { get; }
    public bool B1 { get; }

    public InTestClass(in string value)
    {
        Value = value;
    }

    public InTestClass(in string value, in bool b1)
        : this(in value)
    {
        B1 = b1;
    }
}

[TestFixture]
public class LateboundReflectionDelegateFactoryTests : TestFixtureBase
{
    [Fact]
    public void ConstructorWithInString()
    {
        var constructor = TestReflectionUtils.GetConstructors(typeof(InTestClass)).Single(c => c.GetParameters().Count() == 1);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Value" };
        var o = (InTestClass)creator(args);
        Assert.IsNotNull(o);
        Assert.AreEqual("Value", o.Value);
    }

    [Fact]
    public void ConstructorWithInStringAndBool()
    {
        var constructor = TestReflectionUtils.GetConstructors(typeof(InTestClass)).Single(c => c.GetParameters().Count() == 2);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Value", true };
        var o = (InTestClass)creator(args);
        Assert.IsNotNull(o);
        Assert.AreEqual("Value", o.Value);
        Assert.True( o.B1);
    }

    [Fact]
    public void ConstructorWithRefString()
    {
        var constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 1);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input" };
        var o = (OutAndRefTestClass)creator(args);
        Assert.IsNotNull(o);
        Assert.AreEqual("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndOutBool()
    {
        var constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 2);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input", null };
        var o = (OutAndRefTestClass)creator(args);
        Assert.IsNotNull(o);
        Assert.AreEqual("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndRefBoolAndRefBool()
    {
        var constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 3);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var args = new object[] { "Input", true, null };
        var o = (OutAndRefTestClass)creator(args);
        Assert.IsNotNull(o);
        Assert.AreEqual("Input", o.Input);
        Assert.True( o.B1);
        Assert.False( o.B2);
    }
}

public struct MyStruct
{
    int _intProperty;

    public int IntProperty
    {
        get => _intProperty;
        set => _intProperty = value;
    }
}