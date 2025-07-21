using FikaShared;
using FikaShared.Requests;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class HeadlessPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        List<OnlineHeadless> _headlessClients = [];

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

#if DEBUG
            _headlessClients.Add(new()
            {
                ProfileId = "TEST",
                State = Enums.EHeadlessState.Ready,
                Players = Random.Shared.Next(0, 5)
            });
            _headlessClients.Add(new()
            {
                ProfileId = "TEST2",
                State = Enums.EHeadlessState.InRaid,
                Players = Random.Shared.Next(0, 5)
            });
            _headlessClients.Add(new()
            {
                ProfileId = "TEST3",
                State = Enums.EHeadlessState.InRaid,
                Players = Random.Shared.Next(0, 5)
            });
            _headlessClients.Add(new()
            {
                ProfileId = "TEST4",
                State = Enums.EHeadlessState.Ready,
                Players = Random.Shared.Next(0, 5)
            });
#else
            try
            {
                var clients = await HttpClient.GetFromJsonAsync<GetHeadlessResponse>("get/headless");
                _headlessClients.AddRange(clients.HeadlessClients);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"There was an error retrieving the data: {ex.Message}", Severity.Error);
            }
#endif
        }

        public async Task RestartHeadless(OnlineHeadless headless)
        {
            try
            {
                var request = new ProfileIdRequest()
                {
                    ProfileId = headless.ProfileId
                };
                var result = await HttpClient.PostAsJsonAsync<ProfileIdRequest>("post/restartheadless", request);

                if (!result.IsSuccessStatusCode)
                {
                    Snackbar.Add($"There was an error returned from the server: StatusCode {result.StatusCode}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"There was an error when sending the request: {ex.Message}", Severity.Error);
            }
        }
    }
}