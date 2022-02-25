using System.Xml.Linq;

class XCommentWrapper : XObjectWrapper
{
    XComment Text => (XComment)WrappedNode!;

    public XCommentWrapper(XComment text)
        : base(text)
    {
    }

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