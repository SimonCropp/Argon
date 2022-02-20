using System.Xml;

class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration
{
    readonly XmlDeclaration _declaration;

    public XmlDeclarationWrapper(XmlDeclaration declaration)
        : base(declaration)
    {
        _declaration = declaration;
    }

    public string Version => _declaration.Version;

    public string Encoding
    {
        get => _declaration.Encoding;
        set => _declaration.Encoding = value;
    }

    public string Standalone
    {
        get => _declaration.Standalone;
        set => _declaration.Standalone = value;
    }
}