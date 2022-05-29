using System.Xml;

class XmlDocumentTypeWrapper : XmlNodeWrapper, IXmlDocumentType
{
    readonly XmlDocumentType documentType;

    public XmlDocumentTypeWrapper(XmlDocumentType documentType)
        : base(documentType) =>
        this.documentType = documentType;

    public string Name => documentType.Name;

    public string? System => documentType.SystemId;

    public string? Public => documentType.PublicId;

    public string? InternalSubset => documentType.InternalSubset;

    public override string LocalName => "DOCTYPE";
}