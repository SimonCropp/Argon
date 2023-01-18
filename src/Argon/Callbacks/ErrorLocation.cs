namespace Argon;

public readonly record struct ErrorLocation(string Path, object? Member)
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