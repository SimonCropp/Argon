using System.Xml;

class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration
{
    readonly XmlDeclaration declaration;

    public XmlDeclarationWrapper(XmlDeclaration declaration)
        : base(declaration) =>
        this.declaration = declaration;

    public string Version => declaration.Version;

    public string? Encoding
    {
        get => declaration.Encoding;
        set => declaration.Encoding = value;
    }

    public string? Standalone
    {
        get => declaration.Standalone;
        set => declaration.Standalone = value;
    }
}