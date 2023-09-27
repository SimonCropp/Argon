using System.Xml.Linq;

class XDocumentTypeWrapper(XDocumentType type) :
    XObjectWrapper(type),
    IXmlDocumentType
{
    public string Name => type.Name;

    public string? System => type.SystemId;

    public string? Public => type.PublicId;

    public string? InternalSubset => type.InternalSubset;

    public override string LocalName => "DOCTYPE";
}