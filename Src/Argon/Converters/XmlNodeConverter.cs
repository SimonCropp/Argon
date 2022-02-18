#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Xml;
using System.Xml.Linq;
using Argon;

#region XmlNodeWrappers
class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument
{
    readonly XmlDocument _document;

    public XmlDocumentWrapper(XmlDocument document)
        : base(document)
    {
        _document = document;
    }

    public IXmlNode CreateComment(string? data)
    {
        return new XmlNodeWrapper(_document.CreateComment(data));
    }

    public IXmlNode CreateTextNode(string? text)
    {
        return new XmlNodeWrapper(_document.CreateTextNode(text));
    }

    public IXmlNode CreateCDataSection(string? data)
    {
        return new XmlNodeWrapper(_document.CreateCDataSection(data));
    }

    public IXmlNode CreateWhitespace(string? text)
    {
        return new XmlNodeWrapper(_document.CreateWhitespace(text));
    }

    public IXmlNode CreateSignificantWhitespace(string? text)
    {
        return new XmlNodeWrapper(_document.CreateSignificantWhitespace(text));
    }

    public IXmlNode CreateXmlDeclaration(string? version, string? encoding, string? standalone)
    {
        return new XmlDeclarationWrapper(_document.CreateXmlDeclaration(version, encoding, standalone));
    }

    public IXmlNode CreateXmlDocumentType(string? name, string? publicId, string? systemId, string? internalSubset)
    {
        return new XmlDocumentTypeWrapper(_document.CreateDocumentType(name, publicId, systemId, null));
    }

    public IXmlNode CreateProcessingInstruction(string target, string? data)
    {
        return new XmlNodeWrapper(_document.CreateProcessingInstruction(target, data));
    }

    public IXmlElement CreateElement(string elementName)
    {
        return new XmlElementWrapper(_document.CreateElement(elementName));
    }

    public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
    {
        return new XmlElementWrapper(_document.CreateElement(qualifiedName, namespaceUri));
    }

    public IXmlNode CreateAttribute(string name, string? value)
    {
        var attribute = new XmlNodeWrapper(_document.CreateAttribute(name))
        {
            Value = value
        };

        return attribute;
    }

    public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string? value)
    {
        var attribute = new XmlNodeWrapper(_document.CreateAttribute(qualifiedName, namespaceUri))
        {
            Value = value
        };

        return attribute;
    }

    public IXmlElement? DocumentElement
    {
        get
        {
            if (_document.DocumentElement == null)
            {
                return null;
            }

            return new XmlElementWrapper(_document.DocumentElement);
        }
    }
}

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

class XmlNodeWrapper : IXmlNode
{
    readonly XmlNode _node;
    List<IXmlNode>? _childNodes;
    List<IXmlNode>? _attributes;

    public XmlNodeWrapper(XmlNode node)
    {
        _node = node;
    }

    public object? WrappedNode => _node;

    public XmlNodeType NodeType => _node.NodeType;

    public virtual string? LocalName => _node.LocalName;

    public List<IXmlNode> ChildNodes
    {
        get
        {
            // childnodes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (_childNodes == null)
            {
                if (!_node.HasChildNodes)
                {
                    _childNodes = XmlNodeConverter.EmptyChildNodes;
                }
                else
                {
                    _childNodes = new List<IXmlNode>(_node.ChildNodes.Count);
                    foreach (XmlNode childNode in _node.ChildNodes)
                    {
                        _childNodes.Add(WrapNode(childNode));
                    }
                }
            }

            return _childNodes;
        }
    }

    protected virtual bool HasChildNodes => _node.HasChildNodes;

    internal static IXmlNode WrapNode(XmlNode node)
    {
        switch (node.NodeType)
        {
            case XmlNodeType.Element:
                return new XmlElementWrapper((XmlElement)node);
            case XmlNodeType.XmlDeclaration:
                return new XmlDeclarationWrapper((XmlDeclaration)node);
            case XmlNodeType.DocumentType:
                return new XmlDocumentTypeWrapper((XmlDocumentType)node);
            default:
                return new XmlNodeWrapper(node);
        }
    }

    public List<IXmlNode> Attributes
    {
        get
        {
            // attributes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (_attributes == null)
            {
                if (!HasAttributes)
                {
                    _attributes = XmlNodeConverter.EmptyChildNodes;
                }
                else
                {
                    _attributes = new List<IXmlNode>(_node.Attributes.Count);
                    foreach (XmlAttribute attribute in _node.Attributes)
                    {
                        _attributes.Add(WrapNode(attribute));
                    }
                }
            }

            return _attributes;
        }
    }

    bool HasAttributes
    {
        get
        {
            if (_node is XmlElement element)
            {
                return element.HasAttributes;
            }

            return _node.Attributes?.Count > 0;
        }
    }

    public IXmlNode? ParentNode
    {
        get
        {
            var node = _node is XmlAttribute attribute ? attribute.OwnerElement : _node.ParentNode;

            if (node == null)
            {
                return null;
            }

            return WrapNode(node);
        }
    }

    public string? Value
    {
        get => _node.Value;
        set => _node.Value = value;
    }

    public IXmlNode AppendChild(IXmlNode newChild)
    {
        var xmlNodeWrapper = (XmlNodeWrapper)newChild;
        _node.AppendChild(xmlNodeWrapper._node);
        _childNodes = null;
        _attributes = null;

        return newChild;
    }

    public string? NamespaceUri => _node.NamespaceURI;
}
#endregion

#region Interfaces
interface IXmlDocument : IXmlNode
{
    IXmlNode CreateComment(string? text);
    IXmlNode CreateTextNode(string? text);
    IXmlNode CreateCDataSection(string? data);
    IXmlNode CreateWhitespace(string? text);
    IXmlNode CreateSignificantWhitespace(string? text);
    IXmlNode CreateXmlDeclaration(string? version, string? encoding, string? standalone);
    IXmlNode CreateXmlDocumentType(string? name, string? publicId, string? systemId, string? internalSubset);
    IXmlNode CreateProcessingInstruction(string target, string? data);
    IXmlElement CreateElement(string elementName);
    IXmlElement CreateElement(string qualifiedName, string namespaceUri);
    IXmlNode CreateAttribute(string name, string? value);
    IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string? value);

    IXmlElement? DocumentElement { get; }
}

interface IXmlDeclaration : IXmlNode
{
    string Version { get; }
    string Encoding { get; set; }
    string Standalone { get; set; }
}

interface IXmlDocumentType : IXmlNode
{
    string Name { get; }
    string System { get; }
    string Public { get; }
    string InternalSubset { get; }
}

interface IXmlElement : IXmlNode
{
    void SetAttributeNode(IXmlNode attribute);
    string GetPrefixOfNamespace(string namespaceUri);
    bool IsEmpty { get; }
}

interface IXmlNode
{
    XmlNodeType NodeType { get; }
    string? LocalName { get; }
    List<IXmlNode> ChildNodes { get; }
    List<IXmlNode> Attributes { get; }
    IXmlNode? ParentNode { get; }
    string? Value { get; set; }
    IXmlNode AppendChild(IXmlNode newChild);
    string? NamespaceUri { get; }
    object? WrappedNode { get; }
}
#endregion

#region XNodeWrappers
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

class XDocumentWrapper : XContainerWrapper, IXmlDocument
{
    XDocument Document => (XDocument)WrappedNode!;

    public XDocumentWrapper(XDocument document)
        : base(document)
    {
    }

    public override List<IXmlNode> ChildNodes
    {
        get
        {
            var childNodes = base.ChildNodes;
            if (Document.Declaration != null && (childNodes.Count == 0 || childNodes[0].NodeType != XmlNodeType.XmlDeclaration))
            {
                childNodes.Insert(0, new XDeclarationWrapper(Document.Declaration));
            }

            return childNodes;
        }
    }

    protected override bool HasChildNodes
    {
        get
        {
            if (base.HasChildNodes)
            {
                return true;
            }

            return Document.Declaration != null;
        }
    }

    public IXmlNode CreateComment(string? text)
    {
        return new XObjectWrapper(new XComment(text));
    }

    public IXmlNode CreateTextNode(string? text)
    {
        return new XObjectWrapper(new XText(text));
    }

    public IXmlNode CreateCDataSection(string? data)
    {
        return new XObjectWrapper(new XCData(data));
    }

    public IXmlNode CreateWhitespace(string? text)
    {
        return new XObjectWrapper(new XText(text));
    }

    public IXmlNode CreateSignificantWhitespace(string? text)
    {
        return new XObjectWrapper(new XText(text));
    }

    public IXmlNode CreateXmlDeclaration(string? version, string? encoding, string? standalone)
    {
        return new XDeclarationWrapper(new XDeclaration(version, encoding, standalone));
    }

    public IXmlNode CreateXmlDocumentType(string? name, string? publicId, string? systemId, string? internalSubset)
    {
        return new XDocumentTypeWrapper(new XDocumentType(name, publicId, systemId, internalSubset));
    }

    public IXmlNode CreateProcessingInstruction(string target, string? data)
    {
        return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
    }

    public IXmlElement CreateElement(string elementName)
    {
        return new XElementWrapper(new XElement(elementName));
    }

    public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
    {
        var localName = MiscellaneousUtils.GetLocalName(qualifiedName);
        return new XElementWrapper(new XElement(XName.Get(localName, namespaceUri)));
    }

    public IXmlNode CreateAttribute(string name, string? value)
    {
        return new XAttributeWrapper(new XAttribute(name, value));
    }

    public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string? value)
    {
        var localName = MiscellaneousUtils.GetLocalName(qualifiedName);
        return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceUri), value));
    }

    public IXmlElement? DocumentElement
    {
        get
        {
            if (Document.Root == null)
            {
                return null;
            }

            return new XElementWrapper(Document.Root);
        }
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
        if (newChild is XDeclarationWrapper declarationWrapper)
        {
            Document.Declaration = declarationWrapper.Declaration;
            return declarationWrapper;
        }
        else
        {
            return base.AppendChild(newChild);
        }
    }
}

class XTextWrapper : XObjectWrapper
{
    XText Text => (XText)WrappedNode!;

    public XTextWrapper(XText text)
        : base(text)
    {
    }

    public override string? Value
    {
        get => Text.Value;
        set => Text.Value = value;
    }

    public override IXmlNode? ParentNode
    {
        get
        {
            if (Text.Parent == null)
            {
                return null;
            }

            return XContainerWrapper.WrapNode(Text.Parent);
        }
    }
}

class XCommentWrapper : XObjectWrapper
{
    XComment Text => (XComment)WrappedNode!;

    public XCommentWrapper(XComment text)
        : base(text)
    {
    }

    public override string? Value
    {
        get => Text.Value;
        set => Text.Value = value;
    }

    public override IXmlNode? ParentNode
    {
        get
        {
            if (Text.Parent == null)
            {
                return null;
            }

            return XContainerWrapper.WrapNode(Text.Parent);
        }
    }
}

class XProcessingInstructionWrapper : XObjectWrapper
{
    XProcessingInstruction ProcessingInstruction => (XProcessingInstruction)WrappedNode!;

    public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction)
        : base(processingInstruction)
    {
    }

    public override string? LocalName => ProcessingInstruction.Target;

    public override string? Value
    {
        get => ProcessingInstruction.Data;
        set => ProcessingInstruction.Data = value;
    }
}

class XContainerWrapper : XObjectWrapper
{
    List<IXmlNode>? _childNodes;

    XContainer Container => (XContainer)WrappedNode!;

    public XContainerWrapper(XContainer container)
        : base(container)
    {
    }

    public override List<IXmlNode> ChildNodes
    {
        get
        {
            // childnodes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (_childNodes == null)
            {
                if (!HasChildNodes)
                {
                    _childNodes = XmlNodeConverter.EmptyChildNodes;
                }
                else
                {
                    _childNodes = new List<IXmlNode>();
                    foreach (var node in Container.Nodes())
                    {
                        _childNodes.Add(WrapNode(node));
                    }
                }
            }

            return _childNodes;
        }
    }

    protected virtual bool HasChildNodes => Container.LastNode != null;

    public override IXmlNode? ParentNode
    {
        get
        {
            if (Container.Parent == null)
            {
                return null;
            }

            return WrapNode(Container.Parent);
        }
    }

    internal static IXmlNode WrapNode(XObject node)
    {
        if (node is XDocument document)
        {
            return new XDocumentWrapper(document);
        }

        if (node is XElement element)
        {
            return new XElementWrapper(element);
        }

        if (node is XContainer container)
        {
            return new XContainerWrapper(container);
        }

        if (node is XProcessingInstruction pi)
        {
            return new XProcessingInstructionWrapper(pi);
        }

        if (node is XText text)
        {
            return new XTextWrapper(text);
        }

        if (node is XComment comment)
        {
            return new XCommentWrapper(comment);
        }

        if (node is XAttribute attribute)
        {
            return new XAttributeWrapper(attribute);
        }

        if (node is XDocumentType type)
        {
            return new XDocumentTypeWrapper(type);
        }

        return new XObjectWrapper(node);
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
        Container.Add(newChild.WrappedNode);
        _childNodes = null;

        return newChild;
    }
}

class XObjectWrapper : IXmlNode
{
    readonly XObject? _xmlObject;

    public XObjectWrapper(XObject? xmlObject)
    {
        _xmlObject = xmlObject;
    }

    public object? WrappedNode => _xmlObject;

    public virtual XmlNodeType NodeType => _xmlObject?.NodeType ?? XmlNodeType.None;

    public virtual string? LocalName => null;

    public virtual List<IXmlNode> ChildNodes => XmlNodeConverter.EmptyChildNodes;

    public virtual List<IXmlNode> Attributes => XmlNodeConverter.EmptyChildNodes;

    public virtual IXmlNode? ParentNode => null;

    public virtual string? Value
    {
        get => null;
        set => throw new InvalidOperationException();
    }

    public virtual IXmlNode AppendChild(IXmlNode newChild)
    {
        throw new InvalidOperationException();
    }

    public virtual string? NamespaceUri => null;
}

class XAttributeWrapper : XObjectWrapper
{
    XAttribute Attribute => (XAttribute)WrappedNode!;

    public XAttributeWrapper(XAttribute attribute)
        : base(attribute)
    {
    }

    public override string? Value
    {
        get => Attribute.Value;
        set => Attribute.Value = value;
    }

    public override string? LocalName => Attribute.Name.LocalName;

    public override string? NamespaceUri => Attribute.Name.NamespaceName;

    public override IXmlNode? ParentNode
    {
        get
        {
            if (Attribute.Parent == null)
            {
                return null;
            }

            return XContainerWrapper.WrapNode(Attribute.Parent);
        }
    }
}

class XElementWrapper : XContainerWrapper, IXmlElement
{
    List<IXmlNode>? _attributes;

    XElement Element => (XElement)WrappedNode!;

    public XElementWrapper(XElement element)
        : base(element)
    {
    }

    public void SetAttributeNode(IXmlNode attribute)
    {
        var wrapper = (XObjectWrapper)attribute;
        Element.Add(wrapper.WrappedNode);
        _attributes = null;
    }

    public override List<IXmlNode> Attributes
    {
        get
        {
            // attributes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (_attributes == null)
            {
                if (!Element.HasAttributes && !HasImplicitNamespaceAttribute(NamespaceUri!))
                {
                    _attributes = XmlNodeConverter.EmptyChildNodes;
                }
                else
                {
                    _attributes = new List<IXmlNode>();
                    foreach (var attribute in Element.Attributes())
                    {
                        _attributes.Add(new XAttributeWrapper(attribute));
                    }

                    // ensure elements created with a namespace but no namespace attribute are converted correctly
                    // e.g. new XElement("{http://example.com}MyElement");
                    var namespaceUri = NamespaceUri!;
                    if (HasImplicitNamespaceAttribute(namespaceUri))
                    {
                        _attributes.Insert(0, new XAttributeWrapper(new XAttribute("xmlns", namespaceUri)));
                    }
                }
            }

            return _attributes;
        }
    }

    bool HasImplicitNamespaceAttribute(string namespaceUri)
    {
        if (!StringUtils.IsNullOrEmpty(namespaceUri) && namespaceUri != ParentNode?.NamespaceUri)
        {
            if (StringUtils.IsNullOrEmpty(GetPrefixOfNamespace(namespaceUri)))
            {
                var namespaceDeclared = false;

                if (Element.HasAttributes)
                {
                    foreach (var attribute in Element.Attributes())
                    {
                        if (attribute.Name.LocalName == "xmlns" && StringUtils.IsNullOrEmpty(attribute.Name.NamespaceName) && attribute.Value == namespaceUri)
                        {
                            namespaceDeclared = true;
                        }
                    }
                }

                if (!namespaceDeclared)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
        var result = base.AppendChild(newChild);
        _attributes = null;
        return result;
    }

    public override string? Value
    {
        get => Element.Value;
        set => Element.Value = value;
    }

    public override string? LocalName => Element.Name.LocalName;

    public override string? NamespaceUri => Element.Name.NamespaceName;

    public string GetPrefixOfNamespace(string namespaceUri)
    {
        return Element.GetPrefixOfNamespace(namespaceUri);
    }

    public bool IsEmpty => Element.IsEmpty;
}
#endregion

/// <summary>
/// Converts XML to and from JSON.
/// </summary>
public class XmlNodeConverter : JsonConverter
{
    internal static readonly List<IXmlNode> EmptyChildNodes = new();

    const string TextName = "#text";
    const string CommentName = "#comment";
    const string CDataName = "#cdata-section";
    const string WhitespaceName = "#whitespace";
    const string SignificantWhitespaceName = "#significant-whitespace";
    const string DeclarationName = "?xml";
    const string JsonNamespaceUri = "http://james.newtonking.com/projects/json";

    /// <summary>
    /// Gets or sets the name of the root element to insert when deserializing to XML if the JSON structure has produced multiple root elements.
    /// </summary>
    /// <value>The name of the deserialized root element.</value>
    public string? DeserializeRootElementName { get; set; }

    /// <summary>
    /// Gets or sets a value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </summary>
    /// <value><c>true</c> if the array attribute is written to the XML; otherwise, <c>false</c>.</value>
    public bool WriteArrayAttribute { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to write the root JSON object.
    /// </summary>
    /// <value><c>true</c> if the JSON root object is omitted; otherwise, <c>false</c>.</value>
    public bool OmitRootObject { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to encode special characters when converting JSON to XML.
    /// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
    /// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
    /// as part of the XML element name.
    /// </summary>
    /// <value><c>true</c> if special characters are encoded; otherwise, <c>false</c>.</value>
    public bool EncodeSpecialCharacters { get; set; }

    #region Writing
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <param name="value">The value.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var node = WrapXml(value);

        var manager = new XmlNamespaceManager(new NameTable());
        PushParentNamespaces(node, manager);

        if (!OmitRootObject)
        {
            writer.WriteStartObject();
        }

        SerializeNode(writer, node, manager, !OmitRootObject);

        if (!OmitRootObject)
        {
            writer.WriteEndObject();
        }
    }

    IXmlNode WrapXml(object value)
    {
        if (value is XObject xObject)
        {
            return XContainerWrapper.WrapNode(xObject);
        }
        if (value is XmlNode node)
        {
            return XmlNodeWrapper.WrapNode(node);
        }

        throw new ArgumentException("Value must be an XML object.", nameof(value));
    }

    void PushParentNamespaces(IXmlNode node, XmlNamespaceManager manager)
    {
        List<IXmlNode>? parentElements = null;

        var parent = node;
        while ((parent = parent.ParentNode) != null)
        {
            if (parent.NodeType == XmlNodeType.Element)
            {
                if (parentElements == null)
                {
                    parentElements = new List<IXmlNode>();
                }

                parentElements.Add(parent);
            }
        }

        if (parentElements != null)
        {
            parentElements.Reverse();

            foreach (var parentElement in parentElements)
            {
                manager.PushScope();
                foreach (var attribute in parentElement.Attributes)
                {
                    if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/" && attribute.LocalName != "xmlns")
                    {
                        manager.AddNamespace(attribute.LocalName, attribute.Value);
                    }
                }
            }
        }
    }

    string ResolveFullName(IXmlNode node, XmlNamespaceManager manager)
    {
        var prefix = node.NamespaceUri == null || (node.LocalName == "xmlns" && node.NamespaceUri == "http://www.w3.org/2000/xmlns/")
            ? null
            : manager.LookupPrefix(node.NamespaceUri);

        if (StringUtils.IsNullOrEmpty(prefix))
        {
            return XmlConvert.DecodeName(node.LocalName);
        }

        return $"{prefix}:{XmlConvert.DecodeName(node.LocalName)}";
    }

    string GetPropertyName(IXmlNode node, XmlNamespaceManager manager)
    {
        switch (node.NodeType)
        {
            case XmlNodeType.Attribute:
                if (node.NamespaceUri == JsonNamespaceUri)
                {
                    return $"${node.LocalName}";
                }
                else
                {
                    return $"@{ResolveFullName(node, manager)}";
                }
            case XmlNodeType.CDATA:
                return CDataName;
            case XmlNodeType.Comment:
                return CommentName;
            case XmlNodeType.Element:
                if (node.NamespaceUri == JsonNamespaceUri)
                {
                    return $"${node.LocalName}";
                }
                else
                {
                    return ResolveFullName(node, manager);
                }
            case XmlNodeType.ProcessingInstruction:
                return $"?{ResolveFullName(node, manager)}";
            case XmlNodeType.DocumentType:
                return $"!{ResolveFullName(node, manager)}";
            case XmlNodeType.XmlDeclaration:
                return DeclarationName;
            case XmlNodeType.SignificantWhitespace:
                return SignificantWhitespaceName;
            case XmlNodeType.Text:
                return TextName;
            case XmlNodeType.Whitespace:
                return WhitespaceName;
            default:
                throw new JsonSerializationException($"Unexpected XmlNodeType when getting node name: {node.NodeType}");
        }
    }

    bool IsArray(IXmlNode node)
    {
        foreach (var attribute in node.Attributes)
        {
            if (attribute.LocalName == "Array" && attribute.NamespaceUri == JsonNamespaceUri)
            {
                return XmlConvert.ToBoolean(attribute.Value);
            }
        }

        return false;
    }

    void SerializeGroupedNodes(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
    {
        switch (node.ChildNodes.Count)
        {
            case 0:
            {
                // nothing to serialize
                break;
            }
            case 1:
            {
                // avoid grouping when there is only one node
                var nodeName = GetPropertyName(node.ChildNodes[0], manager);
                WriteGroupedNodes(writer, manager, writePropertyName, node.ChildNodes, nodeName);
                break;
            }
            default:
            {
                // check whether nodes have the same name
                // if they don't then group into dictionary together by name

                // value of dictionary will be a single IXmlNode when there is one for a name,
                // or a List<IXmlNode> when there are multiple
                Dictionary<string, object>? nodesGroupedByName = null;

                string? nodeName = null;

                for (var i = 0; i < node.ChildNodes.Count; i++)
                {
                    var childNode = node.ChildNodes[i];
                    var currentNodeName = GetPropertyName(childNode, manager);

                    if (nodesGroupedByName == null)
                    {
                        if (nodeName == null)
                        {
                            nodeName = currentNodeName;
                        }
                        else if (currentNodeName == nodeName)
                        {
                            // current node name matches others
                        }
                        else
                        {
                            nodesGroupedByName = new Dictionary<string, object>();
                            if (i > 1)
                            {
                                var nodes = new List<IXmlNode>(i);
                                for (var j = 0; j < i; j++)
                                {
                                    nodes.Add(node.ChildNodes[j]);
                                }
                                nodesGroupedByName.Add(nodeName, nodes);
                            }
                            else
                            {
                                nodesGroupedByName.Add(nodeName, node.ChildNodes[0]);
                            }
                            nodesGroupedByName.Add(currentNodeName, childNode);
                        }
                    }
                    else
                    {
                        if (!nodesGroupedByName.TryGetValue(currentNodeName, out var value))
                        {
                            nodesGroupedByName.Add(currentNodeName, childNode);
                        }
                        else
                        {
                            if (!(value is List<IXmlNode> nodes))
                            {
                                nodes = new List<IXmlNode> {(IXmlNode)value!};
                                nodesGroupedByName[currentNodeName] = nodes;
                            }

                            nodes.Add(childNode);
                        }
                    }
                }

                if (nodesGroupedByName == null)
                {
                    WriteGroupedNodes(writer, manager, writePropertyName, node.ChildNodes, nodeName!);
                }
                else
                {
                    // loop through grouped nodes. write single name instances as normal,
                    // write multiple names together in an array
                    foreach (var nodeNameGroup in nodesGroupedByName)
                    {
                        if (nodeNameGroup.Value is List<IXmlNode> nodes)
                        {
                            WriteGroupedNodes(writer, manager, writePropertyName, nodes, nodeNameGroup.Key);
                        }
                        else
                        {
                            WriteGroupedNodes(writer, manager, writePropertyName, (IXmlNode)nodeNameGroup.Value, nodeNameGroup.Key);
                        }
                    }
                }
                break;
            }
        }
    }

    void WriteGroupedNodes(JsonWriter writer, XmlNamespaceManager manager, bool writePropertyName, List<IXmlNode> groupedNodes, string elementNames)
    {
        var writeArray = groupedNodes.Count != 1 || IsArray(groupedNodes[0]);

        if (!writeArray)
        {
            SerializeNode(writer, groupedNodes[0], manager, writePropertyName);
        }
        else
        {
            if (writePropertyName)
            {
                writer.WritePropertyName(elementNames);
            }

            writer.WriteStartArray();

            for (var i = 0; i < groupedNodes.Count; i++)
            {
                SerializeNode(writer, groupedNodes[i], manager, false);
            }

            writer.WriteEndArray();
        }
    }

    void WriteGroupedNodes(JsonWriter writer, XmlNamespaceManager manager, bool writePropertyName, IXmlNode node, string elementNames)
    {
        var writeArray = IsArray(node);

        if (!writeArray)
        {
            SerializeNode(writer, node, manager, writePropertyName);
        }
        else
        {
            if (writePropertyName)
            {
                writer.WritePropertyName(elementNames);
            }

            writer.WriteStartArray();

            SerializeNode(writer, node, manager, false);

            writer.WriteEndArray();
        }
    }

    void SerializeNode(JsonWriter writer, IXmlNode node, XmlNamespaceManager manager, bool writePropertyName)
    {
        switch (node.NodeType)
        {
            case XmlNodeType.Document:
            case XmlNodeType.DocumentFragment:
                SerializeGroupedNodes(writer, node, manager, writePropertyName);
                break;
            case XmlNodeType.Element:
                if (IsArray(node) && AllSameName(node) && node.ChildNodes.Count > 0)
                {
                    SerializeGroupedNodes(writer, node, manager, false);
                }
                else
                {
                    manager.PushScope();

                    foreach (var attribute in node.Attributes)
                    {
                        if (attribute.NamespaceUri == "http://www.w3.org/2000/xmlns/")
                        {
                            var namespacePrefix = attribute.LocalName != "xmlns"
                                ? XmlConvert.DecodeName(attribute.LocalName)
                                : string.Empty;
                            var namespaceUri = attribute.Value;
                            if (namespaceUri == null)
                            {
                                throw new JsonSerializationException("Namespace attribute must have a value.");
                            }

                            manager.AddNamespace(namespacePrefix, namespaceUri);
                        }
                    }

                    if (writePropertyName)
                    {
                        writer.WritePropertyName(GetPropertyName(node, manager));
                    }

                    if (!ValueAttributes(node.Attributes) && node.ChildNodes.Count == 1
                                                          && node.ChildNodes[0].NodeType == XmlNodeType.Text)
                    {
                        // write elements with a single text child as a name value pair
                        writer.WriteValue(node.ChildNodes[0].Value);
                    }
                    else if (node.ChildNodes.Count == 0 && node.Attributes.Count == 0)
                    {
                        var element = (IXmlElement)node;

                        // empty element
                        if (element.IsEmpty)
                        {
                            writer.WriteNull();
                        }
                        else
                        {
                            writer.WriteValue(string.Empty);
                        }
                    }
                    else
                    {
                        writer.WriteStartObject();

                        for (var i = 0; i < node.Attributes.Count; i++)
                        {
                            SerializeNode(writer, node.Attributes[i], manager, true);
                        }

                        SerializeGroupedNodes(writer, node, manager, true);

                        writer.WriteEndObject();
                    }

                    manager.PopScope();
                }

                break;
            case XmlNodeType.Comment:
                if (writePropertyName)
                {
                    writer.WriteComment(node.Value);
                }
                break;
            case XmlNodeType.Attribute:
            case XmlNodeType.Text:
            case XmlNodeType.CDATA:
            case XmlNodeType.ProcessingInstruction:
            case XmlNodeType.Whitespace:
            case XmlNodeType.SignificantWhitespace:
                if (node.NamespaceUri == "http://www.w3.org/2000/xmlns/" && node.Value == JsonNamespaceUri)
                {
                    return;
                }

                if (node.NamespaceUri == JsonNamespaceUri)
                {
                    if (node.LocalName == "Array")
                    {
                        return;
                    }
                }

                if (writePropertyName)
                {
                    writer.WritePropertyName(GetPropertyName(node, manager));
                }
                writer.WriteValue(node.Value);
                break;
            case XmlNodeType.XmlDeclaration:
                var declaration = (IXmlDeclaration)node;
                writer.WritePropertyName(GetPropertyName(node, manager));
                writer.WriteStartObject();

                if (!StringUtils.IsNullOrEmpty(declaration.Version))
                {
                    writer.WritePropertyName("@version");
                    writer.WriteValue(declaration.Version);
                }
                if (!StringUtils.IsNullOrEmpty(declaration.Encoding))
                {
                    writer.WritePropertyName("@encoding");
                    writer.WriteValue(declaration.Encoding);
                }
                if (!StringUtils.IsNullOrEmpty(declaration.Standalone))
                {
                    writer.WritePropertyName("@standalone");
                    writer.WriteValue(declaration.Standalone);
                }

                writer.WriteEndObject();
                break;
            case XmlNodeType.DocumentType:
                var documentType = (IXmlDocumentType)node;
                writer.WritePropertyName(GetPropertyName(node, manager));
                writer.WriteStartObject();

                if (!StringUtils.IsNullOrEmpty(documentType.Name))
                {
                    writer.WritePropertyName("@name");
                    writer.WriteValue(documentType.Name);
                }
                if (!StringUtils.IsNullOrEmpty(documentType.Public))
                {
                    writer.WritePropertyName("@public");
                    writer.WriteValue(documentType.Public);
                }
                if (!StringUtils.IsNullOrEmpty(documentType.System))
                {
                    writer.WritePropertyName("@system");
                    writer.WriteValue(documentType.System);
                }
                if (!StringUtils.IsNullOrEmpty(documentType.InternalSubset))
                {
                    writer.WritePropertyName("@internalSubset");
                    writer.WriteValue(documentType.InternalSubset);
                }

                writer.WriteEndObject();
                break;
            default:
                throw new JsonSerializationException($"Unexpected XmlNodeType when serializing nodes: {node.NodeType}");
        }
    }

    static bool AllSameName(IXmlNode node)
    {
        foreach (var childNode in node.ChildNodes)
        {
            if (childNode.LocalName != node.LocalName)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Reading
    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Null:
                return null;
            case JsonToken.StartObject:
                break;
            default:
                throw JsonSerializationException.Create(reader, "XmlNodeConverter can only convert JSON that begins with an object.");
        }

        var manager = new XmlNamespaceManager(new NameTable());
        IXmlDocument? document = null;
        IXmlNode? rootNode = null;

        if (typeof(XObject).IsAssignableFrom(objectType))
        {
            if (objectType != typeof(XContainer)
                && objectType != typeof(XDocument)
                && objectType != typeof(XElement)
                && objectType != typeof(XNode)
                && objectType != typeof(XObject))
            {
                throw JsonSerializationException.Create(reader, "XmlNodeConverter only supports deserializing XDocument, XElement, XContainer, XNode or XObject.");
            }

            var d = new XDocument();
            document = new XDocumentWrapper(d);
            rootNode = document;
        }
        if (typeof(XmlNode).IsAssignableFrom(objectType))
        {
            if (objectType != typeof(XmlDocument)
                && objectType != typeof(XmlElement)
                && objectType != typeof(XmlNode))
            {
                throw JsonSerializationException.Create(reader, "XmlNodeConverter only supports deserializing XmlDocument, XmlElement or XmlNode.");
            }

            var d = new XmlDocument
            {
                // prevent http request when resolving any DTD references
                XmlResolver = null
            };

            document = new XmlDocumentWrapper(d);
            rootNode = document;
        }

        if (document == null || rootNode == null)
        {
            throw JsonSerializationException.Create(reader, $"Unexpected type when converting XML: {objectType}");
        }

        if (!StringUtils.IsNullOrEmpty(DeserializeRootElementName))
        {
            ReadElement(reader, document, rootNode, DeserializeRootElementName, manager);
        }
        else
        {
            reader.ReadAndAssert();
            DeserializeNode(reader, document, manager, rootNode);
        }

        if (objectType == typeof(XElement))
        {
            var element = (XElement)document.DocumentElement!.WrappedNode!;
            element.Remove();

            return element;
        }
        if (objectType == typeof(XmlElement))
        {
            return document.DocumentElement!.WrappedNode;
        }

        return document.WrappedNode;
    }

    void DeserializeValue(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, string propertyName, IXmlNode currentNode)
    {
        if (!EncodeSpecialCharacters)
        {
            switch (propertyName)
            {
                case TextName:
                    currentNode.AppendChild(document.CreateTextNode(ConvertTokenToXmlValue(reader)));
                    return;
                case CDataName:
                    currentNode.AppendChild(document.CreateCDataSection(ConvertTokenToXmlValue(reader)));
                    return;
                case WhitespaceName:
                    currentNode.AppendChild(document.CreateWhitespace(ConvertTokenToXmlValue(reader)));
                    return;
                case SignificantWhitespaceName:
                    currentNode.AppendChild(document.CreateSignificantWhitespace(ConvertTokenToXmlValue(reader)));
                    return;
                default:
                    // processing instructions and the xml declaration start with ?
                    if (!StringUtils.IsNullOrEmpty(propertyName) && propertyName[0] == '?')
                    {
                        CreateInstruction(reader, document, currentNode, propertyName);
                        return;
                    }
                    else if (string.Equals(propertyName, "!DOCTYPE", StringComparison.OrdinalIgnoreCase))
                    {
                        CreateDocumentType(reader, document, currentNode);
                        return;
                    }
                    break;
            }
        }

        if (reader.TokenType == JsonToken.StartArray)
        {
            // handle nested arrays
            ReadArrayElements(reader, document, propertyName, currentNode, manager);
            return;
        }

        // have to wait until attributes have been parsed before creating element
        // attributes may contain namespace info used by the element
        ReadElement(reader, document, currentNode, propertyName, manager);
    }

    void ReadElement(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, XmlNamespaceManager manager)
    {
        if (StringUtils.IsNullOrEmpty(propertyName))
        {
            throw JsonSerializationException.Create(reader, "XmlNodeConverter cannot convert JSON with an empty property name to XML.");
        }

        Dictionary<string, string?>? attributeNameValues = null;
        string? elementPrefix = null;

        if (!EncodeSpecialCharacters)
        {
            attributeNameValues = ShouldReadInto(reader)
                ? ReadAttributeElements(reader, manager)
                : null;
            elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

            if (propertyName.StartsWith('@'))
            {
                var attributeName = propertyName.Substring(1);
                var attributePrefix = MiscellaneousUtils.GetPrefix(attributeName);

                AddAttribute(reader, document, currentNode, propertyName, attributeName, manager, attributePrefix);
                return;
            }

            if (propertyName.StartsWith('$'))
            {
                switch (propertyName)
                {
                    case JsonTypeReflector.ArrayValuesPropertyName:
                        propertyName = propertyName.Substring(1);
                        elementPrefix = manager.LookupPrefix(JsonNamespaceUri);
                        CreateElement(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues);
                        return;
                    case JsonTypeReflector.IdPropertyName:
                    case JsonTypeReflector.RefPropertyName:
                    case JsonTypeReflector.TypePropertyName:
                    case JsonTypeReflector.ValuePropertyName:
                        var attributeName = propertyName.Substring(1);
                        var attributePrefix = manager.LookupPrefix(JsonNamespaceUri);
                        AddAttribute(reader, document, currentNode, propertyName, attributeName, manager, attributePrefix);
                        return;
                }
            }
        }
        else
        {
            if (ShouldReadInto(reader))
            {
                reader.ReadAndAssert();
            }
        }

        CreateElement(reader, document, currentNode, propertyName, manager, elementPrefix, attributeNameValues);
    }

    void CreateElement(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string elementName, XmlNamespaceManager manager, string? elementPrefix, Dictionary<string, string?>? attributeNameValues)
    {
        var element = CreateElement(elementName, document, elementPrefix, manager);

        currentNode.AppendChild(element);

        if (attributeNameValues != null)
        {
            // add attributes to newly created element
            foreach (var nameValue in attributeNameValues)
            {
                var encodedName = XmlConvert.EncodeName(nameValue.Key);
                var attributePrefix = MiscellaneousUtils.GetPrefix(nameValue.Key);

                var attribute = !StringUtils.IsNullOrEmpty(attributePrefix) ? document.CreateAttribute(encodedName, manager.LookupNamespace(attributePrefix) ?? string.Empty, nameValue.Value) : document.CreateAttribute(encodedName, nameValue.Value);

                element.SetAttributeNode(attribute);
            }
        }

        switch (reader.TokenType)
        {
            case JsonToken.String:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Boolean:
            case JsonToken.Date:
            case JsonToken.Bytes:
                var text = ConvertTokenToXmlValue(reader);
                if (text != null)
                {
                    element.AppendChild(document.CreateTextNode(text));
                }
                break;
            case JsonToken.Null:

                // empty element. do nothing
                break;
            case JsonToken.EndObject:

                // finished element will have no children to deserialize
                manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
                break;
            default:
                manager.PushScope();
                DeserializeNode(reader, document, manager, element);
                manager.PopScope();
                manager.RemoveNamespace(string.Empty, manager.DefaultNamespace);
                break;
        }
    }

    static void AddAttribute(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName, string attributeName, XmlNamespaceManager manager, string? attributePrefix)
    {
        if (currentNode.NodeType == XmlNodeType.Document)
        {
            throw JsonSerializationException.Create(reader, $"JSON root object has property '{propertyName}' that will be converted to an attribute. A root object cannot have any attribute properties. Consider specifying a DeserializeRootElementName.");
        }

        var encodedName = XmlConvert.EncodeName(attributeName);
        var attributeValue = ConvertTokenToXmlValue(reader);

        var attribute = !StringUtils.IsNullOrEmpty(attributePrefix)
            ? document.CreateAttribute(encodedName, manager.LookupNamespace(attributePrefix), attributeValue)
            : document.CreateAttribute(encodedName, attributeValue);

        ((IXmlElement)currentNode).SetAttributeNode(attribute);
    }

    static string? ConvertTokenToXmlValue(JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
                return reader.Value?.ToString();
            case JsonToken.Integer:
                if (reader.Value is BigInteger i)
                {
                    return i.ToString(CultureInfo.InvariantCulture);
                }
                return XmlConvert.ToString(Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture));
            case JsonToken.Float:
            {
                if (reader.Value is decimal d)
                {
                    return XmlConvert.ToString(d);
                }

                if (reader.Value is float f)
                {
                    return XmlConvert.ToString(f);
                }

                return XmlConvert.ToString(Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture));
            }
            case JsonToken.Boolean:
                return XmlConvert.ToString(Convert.ToBoolean(reader.Value, CultureInfo.InvariantCulture));
            case JsonToken.Date:
            {
                if (reader.Value is DateTimeOffset offset)
                {
                    return XmlConvert.ToString(offset);
                }

                var d = Convert.ToDateTime(reader.Value, CultureInfo.InvariantCulture);
                return XmlConvert.ToString(d, DateTimeUtils.ToSerializationMode(d.Kind));
            }
            case JsonToken.Bytes:
                return Convert.ToBase64String((byte[])reader.Value!);
            case JsonToken.Null:
                return null;
            default:
                throw JsonSerializationException.Create(reader, $"Cannot get an XML string value from token type '{reader.TokenType}'.");
        }
    }

    void ReadArrayElements(JsonReader reader, IXmlDocument document, string propertyName, IXmlNode currentNode, XmlNamespaceManager manager)
    {
        var elementPrefix = MiscellaneousUtils.GetPrefix(propertyName);

        var nestedArrayElement = CreateElement(propertyName, document, elementPrefix, manager);

        currentNode.AppendChild(nestedArrayElement);

        var count = 0;
        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
        {
            DeserializeValue(reader, document, manager, propertyName, nestedArrayElement);
            count++;
        }

        if (WriteArrayAttribute)
        {
            AddJsonArrayAttribute(nestedArrayElement, document);
        }

        if (count == 1 && WriteArrayAttribute)
        {
            foreach (var childNode in nestedArrayElement.ChildNodes)
            {
                if (childNode is IXmlElement element && element.LocalName == propertyName)
                {
                    AddJsonArrayAttribute(element, document);
                    break;
                }
            }
        }
    }

    void AddJsonArrayAttribute(IXmlElement element, IXmlDocument document)
    {
        element.SetAttributeNode(document.CreateAttribute("json:Array", JsonNamespaceUri, "true"));

        // linq to xml doesn't automatically include prefixes via the namespace manager
        if (element is XElementWrapper)
        {
            if (element.GetPrefixOfNamespace(JsonNamespaceUri) == null)
            {
                element.SetAttributeNode(document.CreateAttribute("xmlns:json", "http://www.w3.org/2000/xmlns/", JsonNamespaceUri));
            }
        }
    }

    bool ShouldReadInto(JsonReader reader)
    {
        // a string token means the element only has a single text child
        switch (reader.TokenType)
        {
            case JsonToken.String:
            case JsonToken.Null:
            case JsonToken.Boolean:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Date:
            case JsonToken.Bytes:
            case JsonToken.StartConstructor:
                return false;
        }

        return true;
    }

    Dictionary<string, string?>? ReadAttributeElements(JsonReader reader, XmlNamespaceManager manager)
    {
        Dictionary<string, string?>? attributeNameValues = null;
        var finished = false;

        // read properties until first non-attribute is encountered
        while (!finished && reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var attributeName = reader.Value!.ToString();

                    if (!StringUtils.IsNullOrEmpty(attributeName))
                    {
                        var firstChar = attributeName[0];
                        string? attributeValue;

                        switch (firstChar)
                        {
                            case '@':
                                if (attributeNameValues == null)
                                {
                                    attributeNameValues = new Dictionary<string, string?>();
                                }

                                attributeName = attributeName.Substring(1);
                                reader.ReadAndAssert();
                                attributeValue = ConvertTokenToXmlValue(reader);
                                attributeNameValues.Add(attributeName, attributeValue);

                                if (IsNamespaceAttribute(attributeName, out var namespacePrefix))
                                {
                                    manager.AddNamespace(namespacePrefix, attributeValue);
                                }
                                break;
                            case '$':
                                switch (attributeName)
                                {
                                    case JsonTypeReflector.ArrayValuesPropertyName:
                                    case JsonTypeReflector.IdPropertyName:
                                    case JsonTypeReflector.RefPropertyName:
                                    case JsonTypeReflector.TypePropertyName:
                                    case JsonTypeReflector.ValuePropertyName:
                                        // check that JsonNamespaceUri is in scope
                                        // if it isn't then add it to document and namespace manager
                                        var jsonPrefix = manager.LookupPrefix(JsonNamespaceUri);
                                        if (jsonPrefix == null)
                                        {
                                            if (attributeNameValues == null)
                                            {
                                                attributeNameValues = new Dictionary<string, string?>();
                                            }

                                            // ensure that the prefix used is free
                                            int? i = null;
                                            while (manager.LookupNamespace($"json{i}") != null)
                                            {
                                                i = i.GetValueOrDefault() + 1;
                                            }
                                            jsonPrefix = $"json{i}";

                                            attributeNameValues.Add($"xmlns:{jsonPrefix}", JsonNamespaceUri);
                                            manager.AddNamespace(jsonPrefix, JsonNamespaceUri);
                                        }

                                        // special case $values, it will have a non-primitive value
                                        if (attributeName == JsonTypeReflector.ArrayValuesPropertyName)
                                        {
                                            finished = true;
                                            break;
                                        }

                                        attributeName = attributeName.Substring(1);
                                        reader.ReadAndAssert();

                                        if (!JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
                                        {
                                            throw JsonSerializationException.Create(reader, $"Unexpected JsonToken: {reader.TokenType}");
                                        }

                                        if (attributeNameValues == null)
                                        {
                                            attributeNameValues = new Dictionary<string, string?>();
                                        }

                                        attributeValue = reader.Value?.ToString();
                                        attributeNameValues.Add($"{jsonPrefix}:{attributeName}", attributeValue);
                                        break;
                                    default:
                                        finished = true;
                                        break;
                                }
                                break;
                            default:
                                finished = true;
                                break;
                        }
                    }
                    else
                    {
                        finished = true;
                    }

                    break;
                case JsonToken.EndObject:
                case JsonToken.Comment:
                    finished = true;
                    break;
                default:
                    throw JsonSerializationException.Create(reader, $"Unexpected JsonToken: {reader.TokenType}");
            }
        }

        return attributeNameValues;
    }

    void CreateInstruction(JsonReader reader, IXmlDocument document, IXmlNode currentNode, string propertyName)
    {
        if (propertyName == DeclarationName)
        {
            string? version = null;
            string? encoding = null;
            string? standalone = null;
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.Value?.ToString())
                {
                    case "@version":
                        reader.ReadAndAssert();
                        version = ConvertTokenToXmlValue(reader);
                        break;
                    case "@encoding":
                        reader.ReadAndAssert();
                        encoding = ConvertTokenToXmlValue(reader);
                        break;
                    case "@standalone":
                        reader.ReadAndAssert();
                        standalone = ConvertTokenToXmlValue(reader);
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, $"Unexpected property name encountered while deserializing XmlDeclaration: {reader.Value}");
                }
            }

            var declaration = document.CreateXmlDeclaration(version, encoding, standalone);
            currentNode.AppendChild(declaration);
        }
        else
        {
            var instruction = document.CreateProcessingInstruction(propertyName.Substring(1), ConvertTokenToXmlValue(reader));
            currentNode.AppendChild(instruction);
        }
    }

    void CreateDocumentType(JsonReader reader, IXmlDocument document, IXmlNode currentNode)
    {
        string? name = null;
        string? publicId = null;
        string? systemId = null;
        string? internalSubset = null;
        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
        {
            switch (reader.Value?.ToString())
            {
                case "@name":
                    reader.ReadAndAssert();
                    name = ConvertTokenToXmlValue(reader);
                    break;
                case "@public":
                    reader.ReadAndAssert();
                    publicId = ConvertTokenToXmlValue(reader);
                    break;
                case "@system":
                    reader.ReadAndAssert();
                    systemId = ConvertTokenToXmlValue(reader);
                    break;
                case "@internalSubset":
                    reader.ReadAndAssert();
                    internalSubset = ConvertTokenToXmlValue(reader);
                    break;
                default:
                    throw JsonSerializationException.Create(reader, $"Unexpected property name encountered while deserializing XmlDeclaration: {reader.Value}");
            }
        }

        var documentType = document.CreateXmlDocumentType(name, publicId, systemId, internalSubset);
        currentNode.AppendChild(documentType);
    }

    IXmlElement CreateElement(string elementName, IXmlDocument document, string? elementPrefix, XmlNamespaceManager manager)
    {
        var encodeName = EncodeSpecialCharacters ? XmlConvert.EncodeLocalName(elementName) : XmlConvert.EncodeName(elementName);
        var ns = StringUtils.IsNullOrEmpty(elementPrefix) ? manager.DefaultNamespace : manager.LookupNamespace(elementPrefix);

        var element = !StringUtils.IsNullOrEmpty(ns) ? document.CreateElement(encodeName, ns) : document.CreateElement(encodeName);

        return element;
    }

    void DeserializeNode(JsonReader reader, IXmlDocument document, XmlNamespaceManager manager, IXmlNode currentNode)
    {
        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    if (currentNode.NodeType == XmlNodeType.Document && document.DocumentElement != null)
                    {
                        throw JsonSerializationException.Create(reader, "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName.");
                    }

                    var propertyName = reader.Value!.ToString();
                    reader.ReadAndAssert();

                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        var count = 0;
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            DeserializeValue(reader, document, manager, propertyName, currentNode);
                            count++;
                        }

                        if (count == 1 && WriteArrayAttribute)
                        {
                            MiscellaneousUtils.GetQualifiedNameParts(propertyName, out var elementPrefix, out var localName);
                            var ns = StringUtils.IsNullOrEmpty(elementPrefix) ? manager.DefaultNamespace : manager.LookupNamespace(elementPrefix);

                            foreach (var childNode in currentNode.ChildNodes)
                            {
                                if (childNode is IXmlElement element && element.LocalName == localName && element.NamespaceUri == ns)
                                {
                                    AddJsonArrayAttribute(element, document);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        DeserializeValue(reader, document, manager, propertyName, currentNode);
                    }
                    continue;
                case JsonToken.StartConstructor:
                    var constructorName = reader.Value!.ToString();

                    while (reader.Read() && reader.TokenType != JsonToken.EndConstructor)
                    {
                        DeserializeValue(reader, document, manager, constructorName, currentNode);
                    }
                    break;
                case JsonToken.Comment:
                    currentNode.AppendChild(document.CreateComment((string)reader.Value!));
                    break;
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    return;
                default:
                    throw JsonSerializationException.Create(reader, $"Unexpected JsonToken when deserializing node: {reader.TokenType}");
            }
        } while (reader.Read());
        // don't read if current token is a property. token was already read when parsing element attributes
    }

    /// <summary>
    /// Checks if the <paramref name="attributeName"/> is a namespace attribute.
    /// </summary>
    /// <param name="attributeName">Attribute name to test.</param>
    /// <param name="prefix">The attribute name prefix if it has one, otherwise an empty string.</param>
    /// <returns><c>true</c> if attribute name is for a namespace attribute, otherwise <c>false</c>.</returns>
    bool IsNamespaceAttribute(string attributeName, [NotNullWhen(true)]out string? prefix)
    {
        if (attributeName.StartsWith("xmlns", StringComparison.Ordinal))
        {
            if (attributeName.Length == 5)
            {
                prefix = string.Empty;
                return true;
            }
            else if (attributeName[5] == ':')
            {
                prefix = attributeName.Substring(6, attributeName.Length - 6);
                return true;
            }
        }
        prefix = null;
        return false;
    }

    bool ValueAttributes(List<IXmlNode> c)
    {
        foreach (var xmlNode in c)
        {
            if (xmlNode.NamespaceUri == JsonNamespaceUri)
            {
                continue;
            }

            if (xmlNode.NamespaceUri == "http://www.w3.org/2000/xmlns/" && xmlNode.Value == JsonNamespaceUri)
            {
                continue;
            }

            return true;
        }

        return false;
    }
    #endregion

    /// <summary>
    /// Determines whether this instance can convert the specified value type.
    /// </summary>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type valueType)
    {
        if (valueType.AssignableToTypeName("System.Xml.Linq.XObject", false))
        {
            return IsXObject(valueType);
        }
        if (valueType.AssignableToTypeName("System.Xml.XmlNode", false))
        {
            return IsXmlNode(valueType);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    bool IsXObject(Type valueType)
    {
        return typeof(XObject).IsAssignableFrom(valueType);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    bool IsXmlNode(Type valueType)
    {
        return typeof(XmlNode).IsAssignableFrom(valueType);
    }
}