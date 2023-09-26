using System.Xml.Linq;

class XContainerWrapper(XContainer container) :
    XObjectWrapper(container)
{
    List<IXmlNode>? childNodes;

    XContainer Container => (XContainer) WrappedNode!;

    public override List<IXmlNode> ChildNodes
    {
        get
        {
            // childnodes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (childNodes == null)
            {
                if (HasChildNodes)
                {
                    childNodes = [];
                    foreach (var node in Container.Nodes())
                    {
                        childNodes.Add(WrapNode(node));
                    }
                }
                else
                {
                    childNodes = XmlNodeConverter.EmptyChildNodes;
                }
            }

            return childNodes;
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
        childNodes = null;

        return newChild;
    }
}