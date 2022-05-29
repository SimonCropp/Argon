using System.Xml;

class XmlElementWrapper : XmlNodeWrapper, IXmlElement
{
    readonly XmlElement element;

    public XmlElementWrapper(XmlElement element)
        : base(element)
    {
        this.element = element;
    }

    public void SetAttributeNode(IXmlNode attribute)
    {
        var xmlAttributeWrapper = (XmlNodeWrapper) attribute;

        element.SetAttributeNode((XmlAttribute) xmlAttributeWrapper.WrappedNode);
    }

    public string GetPrefixOfNamespace(string namespaceUri)
    {
        return element.GetPrefixOfNamespace(namespaceUri);
    }

    public bool IsEmpty => element.IsEmpty;
}