class FSharpFunction
{
    readonly object? instance;
    readonly MethodCall<object?, object> invoker;

    public FSharpFunction(object? instance, MethodCall<object?, object> invoker)
    {
        this.instance = instance;
        this.invoker = invoker;
    }

    public object Invoke(params object[] args) =>
        invoker(instance, args);
}