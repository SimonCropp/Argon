using System.Xml.Linq;

class XElementWrapper : XContainerWrapper, IXmlElement
{
    List<IXmlNode>? attributes;

    XElement Element => (XElement) WrappedNode!;

    public XElementWrapper(XElement element)
        : base(element)
    {
    }

    public void SetAttributeNode(IXmlNode attribute)
    {
        var wrapper = (XObjectWrapper) attribute;
        Element.Add(wrapper.WrappedNode);
        attributes = null;
    }

    public override List<IXmlNode> Attributes
    {
        get
        {
            // attributes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (attributes == null)
            {
                if (Element.HasAttributes || HasImplicitNamespaceAttribute(NamespaceUri))
                {
                    attributes = new();
                    foreach (var attribute in Element.Attributes())
                    {
                        attributes.Add(new XAttributeWrapper(attribute));
                    }

                    // ensure elements created with a namespace but no namespace attribute are converted correctly
                    // e.g. new XElement("{http://example.com}MyElement");
                    var namespaceUri = NamespaceUri;
                    if (HasImplicitNamespaceAttribute(namespaceUri))
                    {
                        attributes.Insert(0, new XAttributeWrapper(new("xmlns", namespaceUri)));
                    }
                }
                else
                {
                    attributes = XmlNodeConverter.EmptyChildNodes;
                }
            }

            return attributes;
        }
    }

    bool HasImplicitNamespaceAttribute(string namespaceUri)
    {
        if (namespaceUri.IsNullOrEmpty() ||
            namespaceUri == ParentNode?.NamespaceUri)
        {
            return false;
        }

        if (GetPrefixOfNamespace(namespaceUri).IsNullOrEmpty())
        {
            var namespaceDeclared = false;

            if (Element.HasAttributes)
            {
                foreach (var attribute in Element.Attributes())
                {
                    if (attribute.Name.LocalName == "xmlns" && attribute.Name.NamespaceName.IsNullOrEmpty() && attribute.Value == namespaceUri)
                    {
                        namespaceDeclared = true;
                    }
                }
            }

            if (!namespaceDeclared)
            {
                return true;
            }
        }

        return false;
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
        var result = base.AppendChild(newChild);
        attributes = null;
        return result;
    }

    public override string Value
    {
        get => Element.Value;
        set => Element.Value = value;
    }

    public override string LocalName => Element.Name.LocalName;

    public override string NamespaceUri => Element.Name.NamespaceName;

    public string? GetPrefixOfNamespace(string namespaceUri) =>
        Element.GetPrefixOfNamespace(namespaceUri);

    public bool IsEmpty => Element.IsEmpty;
}