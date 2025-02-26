using Wox.Plugin;
using System.Windows.Controls;
using Microsoft.PowerToys.Settings.UI.Library;
using System.Windows.Media.Imaging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Net;
using System.Diagnostics;
using ManagedCommon;
using PowerToys_Run_Spotify.Properties;

namespace PowerToys_Run_Spotify;

public class Main : IPlugin, IContextMenu, ISettingProvider
{
    public static string PluginID => "BX1Z634F30489859A3671B4FQ7Y07193";
    public string Name => "Spotify";
    public string Description => Resources.PluginDescription;

    internal string ClientId { get; private set; }
    private string  _appDataPath;
    private string  _credentialsPath;
    private SpotifyClient _spotifyClient;
    private string _imageDirectory { get; set; }

    IEnumerable<PluginAdditionalOption> ISettingProvider.AdditionalOptions => new List<PluginAdditionalOption>()
    {
        new PluginAdditionalOption
        {
            Key = nameof(ClientId),
            DisplayLabel = "Client ID",
            DisplayDescription = Resources.PluginOptionClientIdDescription,
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox
        }
    };

    public void Init(PluginInitContext context)
    {
        _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PowerToys-Run-Spotify");
        _credentialsPath = Path.Combine(_appDataPath, "credentials.json");

        context.API.ThemeChanged += OnThemeChanged;
        OnThemeChanged(Theme.Light, context.API.GetCurrentTheme());
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

    private void OnThemeChanged(
        Theme pre,
        Theme now
    ) {
        _imageDirectory = (now == Theme.Light || now == Theme.HighContrastWhite) ? "images/light" : "images/dark";
    }

    public List<Result> Query(Query query)
    {
        if (string.IsNullOrEmpty(ClientId))
            return new List<Result>() {new Result
            {
                Title = Resources.ResultMissingClientIdTitle,
                SubTitle = Resources.ResultMissingClientIdSubTitle,
                Action = context => true
            }};

        if (!File.Exists(_credentialsPath))
            return new List<Result>() {new Result
                {
                    Title = Resources.ResultLoginTitle,
                    SubTitle = Resources.ResultLoginSubTitle,
                    Action = context =>
                    {
                        _ = LoginToSpotify(ClientId);
                        return true;
                    }
                }};

        var results = new List<Result>();

        if (_spotifyClient == null)
            _spotifyClient = GetSpotifyClient(ClientId).GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(query.Search?.Trim()))
            return GetBasicActions();

        var searchRequest = new SearchRequest(SearchRequest.Types.All, query.Search)
        {
            Limit = 5
        };

        var searchResponse = _spotifyClient.Search.Item(searchRequest).GetAwaiter().GetResult();

        // TODO: Result.TitleHighlightData

        if (searchResponse.Tracks.Items != null)
            results.AddRange(searchResponse.Tracks.Items.Select(track => new Result
            {
                Title = track.Name,
                SubTitle = $"{Resources.ResultSongSubTitle}{(track.Explicit ? $" • {Resources.ResultSongExplicitSubTitle}" : "")} • {Resources.ResultSongBySubTitle} {string.Join(", ", track.Artists.Select(x => x.Name))}",
                Icon = () => new BitmapImage(new Uri(track.Album.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                ContextData = new ContextData
                {
                    ResultType = ResultType.Song,
                    Uri = track.Uri
                },
                Action = context =>
                {
                    _ = EnsureActiveDevice(
                        async (player, request) => await player.ResumePlayback(request),
                        new PlayerResumePlaybackRequest { Uris = new List<string> { track.Uri } }
                    );
                    return true;
                }
            }));

        if (searchResponse.Albums.Items != null)
            results.AddRange(searchResponse.Albums.Items.Select(album => new Result
            {
                Title = album.Name,
                SubTitle = Resources.ResultAlbumSubTitle,
                Icon = () => new BitmapImage(new Uri(album.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                ContextData = new ContextData
                {
                    ResultType = ResultType.Album,
                    Uri = album.Uri
                },
                Action = context =>
                {
                    _ = EnsureActiveDevice(
                        async (player, request) => await player.ResumePlayback(request),
                        new PlayerResumePlaybackRequest { ContextUri = album.Uri }
                    );
                    return true;
                }
            }));


        if (searchResponse.Artists.Items != null)
            results.AddRange(searchResponse.Artists.Items.Select(artist => new Result
            {
                Title = artist.Name,
                SubTitle = Resources.ResultArtistSubTitle,
                Icon = () => new BitmapImage(new Uri(artist.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                ContextData = new ContextData
                {
                    ResultType = ResultType.Artist,
                    Uri = artist.Uri
                },
                Action = context =>
                {
                    _ = EnsureActiveDevice(
                        async (player, request) => await player.ResumePlayback(request),
                        new PlayerResumePlaybackRequest { ContextUri = artist.Uri }
                    );
                    return true;
                }
            }));

        if (searchResponse.Playlists.Items != null)
        {
            results.AddRange(searchResponse.Playlists.Items.Where(playlist => playlist != null).Select(playlist => new Result
            {
                    Title = playlist.Name,
                    SubTitle = "Playlist",
                    Icon = () => new BitmapImage(new Uri(playlist.Images.OrderBy(x => x.Width * x.Height).First().Url)),
                    ContextData = new ContextData
                    {
                        ResultType = ResultType.Playlist,
                        Uri = playlist.Uri
                    },
                    Action = context =>
                    {
                        _ = EnsureActiveDevice(
                            async (player, request) => await player.ResumePlayback(request),
                            new PlayerResumePlaybackRequest { ContextUri = playlist.Uri}
                        );
                        return true;
                    }
                
            }));
        }

        foreach (var result in results)
            result.Score = GetScore(result.Title, query.Search);

        return results;
    }

    private List<Result> GetBasicActions()
    {
        List<Result> results = new List<Result>();


        var previousTrack = new Result
        {
            Title = Resources.ResultPreviousTrackTitle,
            IcoPath = Path.Combine(_imageDirectory, "previous.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SkipPrevious(request),
                    new PlayerSkipPreviousRequest()
                );
                return true;
            },
            Score = 25
        };

        var nextTrack = new Result
        {
            Title = Resources.ResultNextTrackTitle,
            IcoPath = Path.Combine(_imageDirectory, "next.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SkipNext(request),
                    new PlayerSkipNextRequest()
                );
                return true;
            },
            Score = 50
        };

        var pausePlayback = new Result
        {
            Title = Resources.ResultPausePlaybackTitle,
            IcoPath = Path.Combine(_imageDirectory, "pause.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.PausePlayback(request),
                    new PlayerPausePlaybackRequest()
                );
                return true;
            },
            Score = 75
        };

        var resumePlayback = new Result
        {
            Title = Resources.ResultResumePlaybackTitle,
            IcoPath = Path.Combine(_imageDirectory, "play.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.ResumePlayback(request),
                    new PlayerResumePlaybackRequest()
                );
                return true;
            },
            Score = 0
        };


        var togglePlayback = new Result{
            Title = Resources.ResultTogglePlaybackTitle,
            IcoPath = Path.Combine(_imageDirectory, "play-pause.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.GetCurrentPlayback(new PlayerCurrentPlaybackRequest()).ContinueWith(async task =>
                    {
                        var playback = task.Result;
                        if (playback.IsPlaying)
                            await player.PausePlayback(new PlayerPausePlaybackRequest());
                        else
                            await player.ResumePlayback(new PlayerResumePlaybackRequest());
                    }),
                    new PlayerResumePlaybackRequest()
                );
                return true;
            },
            Score = 100
        };

        PlayerCurrentlyPlayingRequest req = new PlayerCurrentlyPlayingRequest{
            Market = "NL"
        };
        var currentlyPlaying = _spotifyClient.Player.GetCurrentlyPlaying(req).GetAwaiter().GetResult();
        var curTrack = currentlyPlaying.Item as FullTrack;

        var nowPlaying = new Result{
            Title = curTrack.Name,
            SubTitle = curTrack.Artists[0].Name,
            Icon = () => new BitmapImage(new Uri(curTrack.Album.Images.OrderBy(x => x.Width * x.Height).First().Url)),
            Score = 80
        };
        
        var turnOnShuffle = new Result
        {
            Title = Resources.ResultTurnOnShuffleTitle,
            IcoPath = Path.Combine(_imageDirectory, "shuffle.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SetShuffle(request),
                    new PlayerShuffleRequest(true)
                );
                return true;
            },
            Score = 0
        };

        var turnOffShuffle = new Result
        {
            Title = Resources.ResultTurnOffShuffleTitle,
            IcoPath = Path.Combine(_imageDirectory, "shuffle.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SetShuffle(request),
                    new PlayerShuffleRequest(false)
                );
                return true;
            },
            Score = 0
        };

        var setRepeatTrack = new Result
        {
            Title = Resources.ResultSetRepeatTrackTitle,
            IcoPath = Path.Combine(_imageDirectory, "repeat.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SetRepeat(request),
                    new PlayerSetRepeatRequest(PlayerSetRepeatRequest.State.Track)
                );
                return true;
            },
            Score = 0
        };

        var setRepeatContext = new Result
        {
            Title = Resources.ResultSetRepeatContextTitle,
            IcoPath = Path.Combine(_imageDirectory, "repeat.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SetRepeat(request),
                    new PlayerSetRepeatRequest(PlayerSetRepeatRequest.State.Context)
                );
                return true;
            },
            Score = 0
        };

        var setRepeatOff = new Result
        {
            Title = Resources.ResultSetRepeatOffTitle,
            IcoPath = Path.Combine(_imageDirectory, "repeat.png"),
            Action = context =>
            {
                _ = EnsureActiveDevice(
                    async (player, request) => await player.SetRepeat(request),
                    new PlayerSetRepeatRequest(PlayerSetRepeatRequest.State.Off)
                );
                return true;
            },
            Score = 0
        };

        results.Add(previousTrack);
        results.Add(nextTrack);
        results.Add(pausePlayback);
        results.Add(resumePlayback);
        results.Add(turnOnShuffle);
        results.Add(turnOffShuffle);
        results.Add(setRepeatTrack);
        results.Add(setRepeatContext);
        results.Add(setRepeatOff);
        results.Add(togglePlayback);
        results.Add(nowPlaying);

        return results;
    }

    private int GetScore(
        string str1,
        string str2
    )
    {
        // Levenshtein distance

        str1 = str1.ToLower();
        str2 = str2.ToLower();

        var m = str1.Length;
        var n = str2.Length;

        var dp = new int[m + 1, n + 1];

        for (int i = 0; i <= m; i++)
            dp[i, 0] = i;
        for (int j = 0; j <= n; j++)
            dp[0, j] = j;

        for (int i = 1; i <= m; i++)
            for (int j = 1; j <= n; j++)
            {
                var cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(
                    Math.Min(
                        dp[i - 1, j] + 1, // deletion
                        dp[i, j - 1] + 1 // insertion
                    ),
                    dp[i - 1, j - 1] + cost // substitution
                );
            }

        var d = dp[m, n];

        return 100 - d;
    }

    public List<ContextMenuResult> LoadContextMenus(Result result)
    {
        var results = new List<ContextMenuResult>();

        var data = result.ContextData as ContextData;

        switch (data?.ResultType)
        {
            case ResultType.Song:
                results.Add(new ContextMenuResult
                {
                    Title = Resources.ContextMenuResultAddToQueueTitle,
                    Glyph = "\xF8AA",
                    FontFamily = "Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Shift,
                    Action = context =>
                    {
                        _ = EnsureActiveDevice(
                            async (player, request) => await player.AddToQueue(request),
                            new PlayerAddToQueueRequest(data.Uri)
                        );
                        return true;
                    },
                });
                break;

            case ResultType.Album:
            case ResultType.Artist:
            case ResultType.Playlist:
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

            Directory.CreateDirectory(_appDataPath);
            File.WriteAllText(_credentialsPath, JsonConvert.SerializeObject(tokenResponse));

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

    private async Task<TResult> EnsureActiveDevice<T, TResult>(
        Func<IPlayerClient, T, Task<TResult>> callback,
        T request
    )
    {
        var requestType = request.GetType();
        var deviceIdProperty = requestType.GetProperty("DeviceId");
        if (deviceIdProperty == null)
            throw new InvalidOperationException ($"Request of type {requestType.Name} does not need an active device.");

        try
        {
            return await callback(_spotifyClient.Player, request);
        }
        catch (APIException exception)
        {
            if (exception.Response?.StatusCode != HttpStatusCode.NotFound)
                throw;

            var possiblePaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spotify", "Spotify.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Spotify", "Spotify.exe"),
            };

            var windowsAppsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps");

            if (Directory.Exists(windowsAppsPath))
            {
                var subDirectories = Directory.GetDirectories(windowsAppsPath, "SpotifyAB.SpotifyMusic_*");
                foreach (string subDirectory in subDirectories)
                {
                    var exePath = Path.Combine(subDirectory, "Spotify.exe");
                    if (File.Exists(exePath))
                        possiblePaths.Add(exePath);
                }
            }

            foreach (var path in possiblePaths)
            {
                if (!File.Exists(path))
                    continue;

                if (Process.Start(path) == null)
                    throw new ApplicationException($"Failed to start process {path}");

                Thread.Sleep(1000 * 10); // wait for Spotify to open

                var deviceResponse = await _spotifyClient.Player.GetAvailableDevices();
                var device = deviceResponse.Devices.FirstOrDefault(x => x.Name == Environment.MachineName);

                deviceIdProperty.SetValue(request, device?.Id);

                return await callback(_spotifyClient.Player, request);;
            }

            throw new ApplicationException("Could not find the Spotify executable");
        }
    }
}
