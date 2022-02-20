using System.Xml;

class XmlDocumentTypeWrapper : XmlNodeWrapper, IXmlDocumentType
{
    readonly XmlDocumentType _documentType;

    public XmlDocumentTypeWrapper(XmlDocumentType documentType)
        : base(documentType)
    {
        _documentType = documentType;
    }

    public string Name => _documentType.Name;

    public string System => _documentType.SystemId;

    public string Public => _documentType.PublicId;

    public string InternalSubset => _documentType.InternalSubset;

    public override string? LocalName => "DOCTYPE";
}