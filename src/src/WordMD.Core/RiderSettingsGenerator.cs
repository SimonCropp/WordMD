namespace WordMD.Core;

public class RiderSettingsGenerator
{
    public void GenerateDotSettings(string directory)
    {
        var dotSettingsPath = Path.Combine(directory, "Default.DotSettings");

        var content = """
            <wpf:ResourceDictionary xml:space="preserve" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:ss="urn:shemas-jetbrains-com:settings-storage-xaml" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <s:Boolean x:Key="/Default/Environment/AutoSave/@EntryValue">False</s:Boolean>
            </wpf:ResourceDictionary>
            """;

        File.WriteAllText(dotSettingsPath, content);
    }
}
