#if NET6_0

public class DocsTests
{
    [Fact]
    public void Foo()
    {
        var solutionDirectory = AttributeReader.GetSolutionDirectory();
        var docsDirectory = Path.Combine(solutionDirectory, "../docs");
        docsDirectory = Path.GetFullPath(docsDirectory);
        var includeFile = Path.Combine(docsDirectory, "index.include.md");
        var samplesDirectory = Path.Combine(docsDirectory, "Samples");
        File.Delete(includeFile);

        var builder = new StringBuilder();
        var level = 0;
        AddDirectory(ref level, builder, docsDirectory, docsDirectory);

        File.WriteAllText(includeFile, builder.ToString());
    }

    static void AddDirectory(ref int level, StringBuilder builder, string directory, string docsDirectory)
    {
        level++;

        var directoryIndent = new string(' ', level*2);
        var url = GetUrl(docsDirectory, directory);
        builder.AppendLine($"{directoryIndent} * [{Path.GetFileName(directory)}]({url})");
        foreach (var nestedDirectory in Directory.EnumerateDirectories(directory))
        {
            AddDirectory(ref level, builder, nestedDirectory, docsDirectory);
        }

        AddFiles(builder, directory, docsDirectory, directoryIndent);

        level--;
    }

    static void AddFiles(StringBuilder builder, string directory, string docsDirectory, string directoryIndent)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.md"))
        {
            AddFile(builder, docsDirectory, file, directoryIndent);
        }
    }

    static void AddFile(StringBuilder builder, string docsDirectory, string file, string directoryIndent)
    {
        var url = GetUrl(docsDirectory, file);
        builder.AppendLine($"{directoryIndent}  * [{Path.GetFileNameWithoutExtension(file)}]({url})");
    }

    static string GetUrl(string docsDirectory, string file)
    {
        var fullPath = Path.GetFullPath(file);
        var suffix = fullPath.Replace(docsDirectory, "")
            .Replace('\\', '/');
        return $"/docs{suffix}";
    }
}

#endif