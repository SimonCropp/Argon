interface IWrappedCollection : IList
{
    object UnderlyingCollection { get; }
}