namespace Argon;

public static class DataSetSettingsExtensions
{
    /// <summary>
    /// Add <see cref="DataSetConverter" /> and <see cref="DataTableConverter" /> to <paramref name="settings" />
    /// </summary>
    public static void AddDataSetConverters(this JsonSerializerSettings settings)
    {
        settings.Converters.Add(new DataSetConverter());
        settings.Converters.Add(new DataTableConverter());
    }
}