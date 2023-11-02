using System.Windows.Input;
using Wox.Plugin;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Windows.Media.Imaging;
using SpotifyAPI.Web;
using System.IO;
using Newtonsoft.Json;

namespace PowerToys_Run_Spotify;

public class Main : IPlugin, IContextMenu, ISettingProvider
{
    public static string PluginID => "BX1Z634F30489859A3671B4FQ7Y07193";
    public string Name => "Spotify";
    public string Description => "Searches and controls Spotify.";

    internal string ClientId { get; private set; }
    private string  _credentialsPath = "credentials.json";
    private SpotifyClient _spotifyClient;

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
        var results = new List<Result>();

        results.Add(new Result
        {
            Title = "Client Id",
            SubTitle = "test",
            Icon = () => new BitmapImage(new Uri("https://i.scdn.co/image/ab67616d0000485163ff604088796d31f7ebef33"))
        });

        // if (_spotifyClient  == null && string.IsNullOrEmpty(ClientId))
        //     return results;
        
        // if (_spotifyClient == null)
        //     _spotifyClient = GetSpotifyClient(ClientId).GetAwaiter().GetResult();

        // var playback = _spotifyClient.Player.GetCurrentPlayback().GetAwaiter().GetResult();
        // if (playback?.Item is FullTrack track)
        //     results.Add(new Result
        //     {
        //         Title = track.Name,
        //         SubTitle = string.Join(", ", track.Artists.Select(x => x.Name)),
        //         Icon = () => new BitmapImage(new Uri(track.Album.Images.OrderBy(x => x.Width * x.Height).First().Url))
        //     });

        return results;
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

    // private async Task LoginToSpotify(string clientId)
    // {
        // var (verifier, challenge) = PKCEUtil.GenerateCodes();

        // var tcs = new TaskCompletionSource();

        // var callbackUri = new Uri("http://localhost:5543/callback");
        // var authServer = new EmbedIOAuthServer(callbackUri, 5543);

        // authServer.AuthorizationCodeReceived += async (sender, response) =>
        // {
        //     await authServer.Stop();
        //     // var a = new PKCETokenRequest(clientId, response.Code, authServer.BaseUri, verifier);
        //     // var o = new OAuthClient();
        //     //var tokenResponse = await o.RequestToken(a);// this line is causing the error
        //     // await File.WriteAllTextAsync(_credentialsPath, JsonConvert.SerializeObject(tokenResponse));
        //     // tcs.SetResult();
        // };

        // await authServer.Start();

        // // var request = new LoginRequest(authServer.BaseUri, clientId, LoginRequest.ResponseType.Code)
        // // {
        // //     CodeChallenge = challenge,
        // //     CodeChallengeMethod = "S256",
        // //     Scope = new List<string>
        // //     {
        // //         Scopes.UserReadPlaybackState,
        // //         Scopes.UserModifyPlaybackState
        // //     }
        // // };

        // // var uri = request.ToUri();
        // // try
        // // {
        // //     BrowserUtil.Open(uri);
        // // }
        // // catch (Exception)
        // // {
        // //     // TODO: notify user somehow?
        // // }

        // await tcs.Task;
    //}

    // private async Task<SpotifyClient> GetSpotifyClient(string clientId)
    // {
        // var json = await File.ReadAllTextAsync(_credentialsPath);
        // var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

        // var authenticator = new PKCEAuthenticator(clientId!, token!);
        // authenticator.TokenRefreshed += (sender, token) => File.WriteAllText(_credentialsPath, JsonConvert.SerializeObject(token));

        // var config = SpotifyClientConfig.CreateDefault()
        //     .WithAuthenticator(authenticator);

        // return new SpotifyClient(config);
        // return null;
    // }
}