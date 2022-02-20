using System.Xml.Linq;

class XTextWrapper : XObjectWrapper
{
    XText Text => (XText)WrappedNode!;

    public XTextWrapper(XText text)
        : base(text)
    {
    }

    public override string? Value
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