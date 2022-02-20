namespace Argon.DataSetConverters;

public static class SettingsExtensions
{
    /// <summary>
    /// Add <see cref="DataSetConverter"/> and <see cref="DataTableConverter"/> to <paramref name="settings"/>
    /// </summary>
    public static void AddDataSetConverters(this JsonSerializerSettings settings)
    {
        settings.Converters.Add(new DataSetConverter());
        settings.Converters.Add(new DataTableConverter());
    }
}