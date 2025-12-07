using System.Net;
using FikaShared;
using FikaShared.Requests;
#if RELEASE
using FikaShared.Responses; 
#endif
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages;

public partial class HeadlessPage
{
    [Inject]
    public HttpClient HttpClient { get; set; } = default!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<HeadlessPage> Logger { get; set; } = default!;

    private readonly List<OnlineHeadless> _headlessClients = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        await base.OnInitializedAsync();

#if DEBUG
        await Task.Delay(TimeSpan.FromSeconds(1));

        _headlessClients.Add(new()
        {
            ProfileId = "TEST",
            Nickname = "TEST",
            State = Enums.EHeadlessState.Ready,
            Players = Random.Shared.Next(0, 5)
        });
        _headlessClients.Add(new()
        {
            ProfileId = "TEST2",
            Nickname = "TEST2",
            State = Enums.EHeadlessState.NotReady,
            Players = Random.Shared.Next(0, 5)
        });
        _headlessClients.Add(new()
        {
            ProfileId = "TEST3",
            Nickname = "TEST3",
            State = Enums.EHeadlessState.NotReady,
            Players = Random.Shared.Next(0, 5)
        });
        _headlessClients.Add(new()
        {
            ProfileId = "TEST4",
            Nickname = "TEST4",
            State = Enums.EHeadlessState.Ready,
            Players = Random.Shared.Next(0, 5)
        });
#else
        try
        {
            var clients = await HttpClient.GetFromJsonAsync<GetHeadlessResponse>("fika/api/headless");
            _headlessClients.AddRange(clients.HeadlessClients);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"There was an error retrieving the data: {ex.Message}", Severity.Error);
        }
#endif
        _loading = false;
    }

    public async Task RestartHeadless(OnlineHeadless headless)
    {
        _loading = true;
        try
        {
            var request = new ProfileIdRequest()
            {
                ProfileId = headless.ProfileId
            };
            var result = await HttpClient.PostAsJsonAsync("fika/api/restartheadless", request);

            if (!result.IsSuccessStatusCode)
            {
                Snackbar.Add($"There was an error returned from the server: StatusCode {result.StatusCode}", Severity.Error);
            }
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.Forbidden)
            {
                Snackbar.Add("Something went wrong when sending the restart request: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                Logger.LogError("Something went wrong when sending the restart request: [403 Forbidden]. Are you using the wrong API key?");
            }
            else if (httpEx.StatusCode is HttpStatusCode.NotFound)
            {
                Snackbar.Add("Something went wrong when sending the restart request: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                Logger.LogError("Something went wrong when sending the restart request: [404 NotFound]. Are you missing the Fika server mod?");
            }
            else
            {
                Snackbar.Add($"There was a HttpRequestException caught when when sending the restart request:\n{httpEx.Message}", Severity.Error);
                Logger.LogError("There was a HttpRequestException caught when when sending the restart request: {HttpException}", httpEx.Message);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"There was an error when sending the request: {ex.Message}", Severity.Error);
        }
        _loading = false;
    }
}