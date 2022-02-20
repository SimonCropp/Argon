using System.Xml.Linq;

class XAttributeWrapper : XObjectWrapper
{
    XAttribute Attribute => (XAttribute)WrappedNode!;

    public XAttributeWrapper(XAttribute attribute)
        : base(attribute)
    {
    }

    public override string? Value
    {
        get => Attribute.Value;
        set => Attribute.Value = value;
    }

    public override string? LocalName => Attribute.Name.LocalName;

    public override string? NamespaceUri => Attribute.Name.NamespaceName;

    public override IXmlNode? ParentNode
    {
        get
        {
            if (Attribute.Parent == null)
            {
                return null;
            }

            return XContainerWrapper.WrapNode(Attribute.Parent);
        }
    }
}