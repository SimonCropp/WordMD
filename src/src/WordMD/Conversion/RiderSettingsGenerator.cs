public class RiderSettingsGenerator
{
    public static void GenerateSettings(string directory)
    {
        Log.Information("Generating Rider .DotSettings for {Directory}", directory);

        var settingsPath = Path.Combine(directory, "Default.DotSettings");
        var settingsContent = """
                              <wpf:ResourceDictionary xml:space="preserve" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                              <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnApplicationIdle/@EntryValue">Skip</s:String>
                              <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnEditorFocusLoss/@EntryValue">Skip</s:String>
                              <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnFrameDeactivation/@EntryValue">Skip</s:String>
                              <s:String x:Key="/Default/Environment/GeneralSettings/SaveSettings/GeneralSettings/SaveSettingsOnBuild/@EntryValue">Skip</s:String>
                              </wpf:ResourceDictionary>
                              """;

        File.WriteAllText(settingsPath, settingsContent);
        Log.Information("Generated {SettingsPath}", settingsPath);
    }
}