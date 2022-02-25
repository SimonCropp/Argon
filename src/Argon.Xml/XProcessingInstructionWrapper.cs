using System.Xml.Linq;

class XProcessingInstructionWrapper : XObjectWrapper
{
    XProcessingInstruction ProcessingInstruction => (XProcessingInstruction)WrappedNode!;

    public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction)
        : base(processingInstruction)
    {
    }

    public override string LocalName => ProcessingInstruction.Target;

    public override string Value
    {
        get => ProcessingInstruction.Data;
        set => ProcessingInstruction.Data = value;
    }
}