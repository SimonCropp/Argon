using System.Xml;

class XmlElementWrapper : XmlNodeWrapper, IXmlElement
{
    readonly XmlElement _element;

    public XmlElementWrapper(XmlElement element)
        : base(element)
    {
        _element = element;
    }

    public void SetAttributeNode(IXmlNode attribute)
    {
        var xmlAttributeWrapper = (XmlNodeWrapper)attribute;

        _element.SetAttributeNode((XmlAttribute)xmlAttributeWrapper.WrappedNode!);
    }

    public string GetPrefixOfNamespace(string namespaceUri)
    {
        return _element.GetPrefixOfNamespace(namespaceUri);
    }

    public bool IsEmpty => _element.IsEmpty;
}