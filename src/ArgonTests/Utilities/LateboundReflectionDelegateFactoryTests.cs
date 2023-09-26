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

public class InTestClass(in string value)
{
    public string Value { get; } = value;
    public bool B1 { get; }

    public InTestClass(in string value, in bool b1)
        : this(in value) =>
        B1 = b1;
}

public class LateboundReflectionDelegateFactoryTests : TestFixtureBase
{
    [Fact]
    public void ConstructorWithInString()
    {
        var constructor = typeof(InTestClass).GetConstructors().Single(_ => _.GetParameters().Length == 1);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var o = (InTestClass) creator(["Value"]);
        Assert.NotNull(o);
        Assert.Equal("Value", o.Value);
    }

    [Fact]
    public void ConstructorWithInStringAndBool()
    {
        var constructor = typeof(InTestClass).GetConstructors().Single(_ => _.GetParameters().Length == 2);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var o = (InTestClass) creator(["Value", true]);
        Assert.NotNull(o);
        Assert.Equal("Value", o.Value);
        XUnitAssert.True(o.B1);
    }

    [Fact]
    public void ConstructorWithRefString()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(_ => _.GetParameters().Length == 1);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var o = (OutAndRefTestClass) creator(["Input"]);
        Assert.NotNull(o);
        Assert.Equal("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndOutBool()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(_ => _.GetParameters().Length == 2);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var o = (OutAndRefTestClass) creator(["Input", null]);
        Assert.NotNull(o);
        Assert.Equal("Input", o.Input);
    }

    [Fact]
    public void ConstructorWithRefStringAndRefBoolAndRefBool()
    {
        var constructor = typeof(OutAndRefTestClass).GetConstructors().Single(_ => _.GetParameters().Length == 3);

        var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

        var o = (OutAndRefTestClass) creator(["Input", true, null]);
        Assert.NotNull(o);
        Assert.Equal("Input", o.Input);
        XUnitAssert.True(o.B1);
        XUnitAssert.False(o.B2);
    }
}

public struct MyStruct
{
    public int IntProperty { get; set; }
}