using Wox.Plugin;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Windows.Media.Imaging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Input;

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

    private object GetSettingOrDefault(
        PowerLauncherPluginSettings settings,
        string key
    )
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
        if (string.IsNullOrEmpty(ClientId))
            return new List<Result>() {new Result
            {
                Title = "Spotify - Missing client ID",
                SubTitle = "Set your client ID in the plugin's settings",
                Action = context => true
            }};

        if (!File.Exists(_credentialsPath))
            return new List<Result>() {new Result
                {
                    Title = "Spotify - Login",
                    SubTitle = "Login to authorize the use of the Spotify API",
                    Action = context =>
                    {
                        LoginToSpotify(ClientId);
                        return true;
                    }
                }};

        var results = new List<Result>();
        
        if (_spotifyClient == null)
            _spotifyClient = GetSpotifyClient(ClientId).GetAwaiter().GetResult();

        // var playback = _spotifyClient.Player.GetCurrentPlayback().GetAwaiter().GetResult();
        // if (playback?.Item is FullTrack track)
        //     results.Add(new Result
        //     {
        //         Title = track.Name,
        //         SubTitle = string.Join(", ", track.Artists.Select(x => x.Name)),
        //         Icon = () => new BitmapImage(new Uri(track.Album.Images.OrderBy(x => x.Width * x.Height).First().Url))
        //     });

        var searchRequest = new SearchRequest(SearchRequest.Types.All, query.Search);
        var searchResponse = _spotifyClient.Search.Item(searchRequest).GetAwaiter().GetResult();

        // TODO: Result.TitleHighlightData

        if (searchResponse.Tracks.Items != null)
            results.AddRange(searchResponse.Tracks.Items.Select(track => new Result
            {
                Title = track.Name,
                SubTitle = $"Song • By {string.Join(", ", track.Artists.Select(x => x.Name))}",
                Icon = () => new BitmapImage(new Uri(track.Album.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                ContextData = new ContextData
                {
                    ResultType = ResultType.Song,
                    Uri = track.Uri
                },
                Action = context =>
                {
                    _spotifyClient.Player.ResumePlayback(new PlayerResumePlaybackRequest{
                        Uris = new List<string> { track.Uri }
                    });
                    return true;
                }
            }));

        if (searchResponse.Artists.Items != null)
            results.AddRange(searchResponse.Artists.Items.Select(artist => new Result
            {
                Title = artist.Name,
                SubTitle = "Artist",
                Icon = () => new BitmapImage(new Uri(artist.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                ContextData = new ContextData
                {
                    ResultType = ResultType.Artist,
                    Uri = artist.Uri
                },
                Action = context =>
                {
                    _spotifyClient.Player.ResumePlayback(new PlayerResumePlaybackRequest{
                        ContextUri = artist.Uri
                    });
                    return true;
                }
            }));

        if (searchResponse.Playlists.Items != null)
            results.AddRange(searchResponse.Playlists.Items.Select(playList => new Result
            {
                Title = playList.Name,
                SubTitle = "Playlist",
                Icon = () => new BitmapImage(new Uri(playList.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                ContextData = new ContextData
                {
                    ResultType = ResultType.Playlist,
                    Uri = playList.Uri
                },
                Action = context =>
                {
                    _spotifyClient.Player.ResumePlayback(new PlayerResumePlaybackRequest{
                        ContextUri = playList.Uri
                    });
                    return true;
                }
            }));

        return results;
    }

    public List<ContextMenuResult> LoadContextMenus(Result result)
    {
        var results = new List<ContextMenuResult>();

        var data = result.ContextData as ContextData;

        switch (data.ResultType)
        {
            case ResultType.Song:
                results.Add(new ContextMenuResult
                {
                    Title = $"Add to queue (Shift+Enter)",
                    Glyph = "\xF8AA",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Shift,
                    Action = context =>
                    {
                        _spotifyClient.Player.AddToQueue(new PlayerAddToQueueRequest(data.Uri));
                        return true;
                    },
                });
            break;

            case ResultType.Artist:
            default:
            break;
        }

        return results;
    }

    private async Task LoginToSpotify(string clientId)
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        var tcs = new TaskCompletionSource();

        var callbackUri = new Uri("http://localhost:5543/callback");
        var authServer = new EmbedIOAuthServer(callbackUri, 5543);

        authServer.AuthorizationCodeReceived += async (sender, response) =>
        {
            await authServer.Stop();
            var tokenRequest = new PKCETokenRequest(clientId, response.Code, authServer.BaseUri, verifier);
            var client = new OAuthClient();
            var tokenResponse = await client.RequestToken(tokenRequest);
            await File.WriteAllTextAsync(_credentialsPath, JsonConvert.SerializeObject(tokenResponse));
            tcs.SetResult();
        };

        await authServer.Start();

        var loginRequest = new LoginRequest(authServer.BaseUri, clientId, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string>
            {
                Scopes.UserReadPlaybackState,
                Scopes.UserModifyPlaybackState
            }
        };

        try
        {
            BrowserUtil.Open(loginRequest.ToUri());
        }
        catch (Exception)
        {
            // TODO: notify user somehow?
            return;
        }

        await tcs.Task;
    }

    private async Task<SpotifyClient> GetSpotifyClient(string clientId)
    {
        var json = await File.ReadAllTextAsync(_credentialsPath);
        var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

        var authenticator = new PKCEAuthenticator(clientId!, token!);
        authenticator.TokenRefreshed += (sender, token) => File.WriteAllText(_credentialsPath, JsonConvert.SerializeObject(token));

        var config = SpotifyClientConfig.CreateDefault()
            .WithAuthenticator(authenticator);

        return new SpotifyClient(config);
    }
}
