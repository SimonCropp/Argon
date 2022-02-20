namespace Argon.DataSetConverters;

public static class SettingsExtensions
{
    public static void AddDataSetConverters(this JsonSerializerSettings settings)
    {
        settings.Converters.Add(new DataSetConverter());
        settings.Converters.Add(new DataTableConverter());
    }
}