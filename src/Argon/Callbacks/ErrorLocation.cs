namespace Argon;

public readonly record struct ErrorLocation(object? Member, string Path)
{
    public override string ToString()
    {
        if (Member == null)
        {
            return Path;
        }

        return $"{Path} - {Member}";
    }
}