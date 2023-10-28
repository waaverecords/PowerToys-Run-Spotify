using System.Windows.Input;
using Wox.Plugin;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;

namespace PowerToys_Run_Spotify;

public class Main : IPlugin, IContextMenu, ISettingProvider
{
    public string Name => "Spotify";
    public string Description => "Searches and controls Spotify.";

    internal string ClientId { get; private set; }

    IEnumerable<PluginAdditionalOption> ISettingProvider.AdditionalOptions => new List<PluginAdditionalOption>()
    {
        new PluginAdditionalOption
        {
            Key = nameof(ClientId),
            DisplayLabel = "Client ID",
            DisplayDescription = "Your Spotify's app client id.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox
        }
    };

    public void Init(PluginInitContext context)
    {
    }

    public Control CreateSettingPanel()
    {
        throw new NotImplementedException();
    }

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        ClientId = (string)GetSettingOrDefault(settings, nameof(ClientId));
    }

    private object GetSettingOrDefault(PowerLauncherPluginSettings settings, string key)
    {
        var defaultOptions = ((ISettingProvider)this).AdditionalOptions;
        var defaultOption = defaultOptions.First(x => x.Key == key);
        var option = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == key);
        
        switch(defaultOption.PluginOptionType)
        {
            case PluginAdditionalOption.AdditionalOptionType.Textbox:
                return option?.TextValue ?? defaultOption.TextValue;
        }

        throw new NotSupportedException();
    }

    public List<Result> Query(Query query)
    {
        return new List<Result>
        {
            new Result
            {
                Title = "r1",
                Action = e =>
                {
                    return true;
                }
            },
            new Result
            {
                Title = "r2",
                Action = e =>
                {
                    return true;
                }
            }
        };
    }

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        return new List<ContextMenuResult>
        {
            new ContextMenuResult
            {
                Title = "Play",
                Glyph = "\xE768",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.P,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = e => true,
            },
            new ContextMenuResult
            {
                Title = "Pause",
                Glyph = "\xE769",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.C,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = e => true,
            }
        };
    }
}