class XCommentWrapper(XComment text) :
    XObjectWrapper(text)
{
    XComment Text => (XComment) WrappedNode!;

    public override string Value
    {
        get => Text.Value;
        set => Text.Value = value;
    }

    public override IXmlNode? ParentNode
    {
        get
        {
            if (Text.Parent == null)
            {
                return null;
            }

            return XContainerWrapper.WrapNode(Text.Parent);
        }
    }
}