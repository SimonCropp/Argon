using System.Xml;
using System.Xml.Linq;

class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration
{
    internal XDeclaration Declaration { get; }

    public XDeclarationWrapper(XDeclaration declaration)
        : base(null)
    {
        Declaration = declaration;
    }

    public override XmlNodeType NodeType => XmlNodeType.XmlDeclaration;

    public string Version => Declaration.Version;

    public string Encoding
    {
        get => Declaration.Encoding;
        set => Declaration.Encoding = value;
    }

    public string Standalone
    {
        get => Declaration.Standalone;
        set => Declaration.Standalone = value;
    }
}