using System.Xml.Linq;

class XDocumentTypeWrapper : XObjectWrapper, IXmlDocumentType
{
    readonly XDocumentType documentType;

    public XDocumentTypeWrapper(XDocumentType documentType)
        : base(documentType)
    {
        this.documentType = documentType;
    }

    public string Name => documentType.Name;

    public string? System => documentType.SystemId;

    public string? Public => documentType.PublicId;

    public string? InternalSubset => documentType.InternalSubset;

    public override string LocalName => "DOCTYPE";
}