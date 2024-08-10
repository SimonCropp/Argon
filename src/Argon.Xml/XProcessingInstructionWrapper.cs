class XProcessingInstructionWrapper(XProcessingInstruction processingInstruction) :
    XObjectWrapper(processingInstruction)
{
    XProcessingInstruction ProcessingInstruction => (XProcessingInstruction) WrappedNode!;

    public override string LocalName => ProcessingInstruction.Target;

    public override string Value
    {
        get => ProcessingInstruction.Data;
        set => ProcessingInstruction.Data = value;
    }
}