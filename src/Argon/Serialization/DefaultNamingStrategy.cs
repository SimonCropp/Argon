namespace Argon;

/// <summary>
/// The default naming strategy. Property names and dictionary keys are unchanged.
/// </summary>
public class DefaultNamingStrategy : NamingStrategy
{
    /// <summary>
    /// Resolves the specified property name.
    /// </summary>
    protected override string ResolvePropertyName(string name) =>
        name;
}