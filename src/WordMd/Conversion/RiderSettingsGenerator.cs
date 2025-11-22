public static class RiderSettingsGenerator
{
    public static Task GenerateSettings(string directory)
    {
        Log.Information("Generating Rider .DotSettings for {Directory}", directory);

        var path = Path.Combine(directory, "Default.DotSettings");

        Log.Information("Generating {RiderSettingsPath}", path);
        return File.WriteAllTextAsync(
            path,
            """
            <wpf:ResourceDictionary xml:space="preserve" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnApplicationIdle/@EntryValue">Skip</s:String>
                <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnEditorFocusLoss/@EntryValue">Skip</s:String>
                <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnFrameDeactivation/@EntryValue">Skip</s:String>
                <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnBuild/@EntryValue">Skip</s:String>
            </wpf:ResourceDictionary>
            """);
    }
}