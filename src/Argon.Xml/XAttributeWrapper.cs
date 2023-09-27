using System.Xml.Linq;

class XAttributeWrapper(XAttribute attribute) : XObjectWrapper(attribute)
{
    XAttribute Attribute => (XAttribute) WrappedNode!;

    public override string Value
    {
        get => Attribute.Value;
        set => Attribute.Value = value;
    }

    public override string LocalName => Attribute.Name.LocalName;

    public override string NamespaceUri => Attribute.Name.NamespaceName;

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