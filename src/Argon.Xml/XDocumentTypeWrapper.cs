using System.Xml.Linq;

class XDocumentTypeWrapper : XObjectWrapper, IXmlDocumentType
{
    readonly XDocumentType _documentType;

    public XDocumentTypeWrapper(XDocumentType documentType)
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