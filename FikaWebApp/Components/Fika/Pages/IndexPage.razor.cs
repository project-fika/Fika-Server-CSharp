using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class IndexPage
    {
        [Inject]
        private HeartbeatService HeartbeatService { get; set; } = default!;

        private Color StatusColor
        {
            get
            {
                return HeartbeatService.IsRunning ? Color.Success : Color.Error;
            }
        }

        private string StatusText
        {
            get
            {
                return HeartbeatService.IsRunning ? "Running" : "Not running";
            }
        }

        private string LastRefreshMinutes
        {
            get
            {
                var timeSpan = DateTime.Now - HeartbeatService.LastRefresh;
                if (timeSpan.TotalMinutes < 1)
                {
                    return "Last update was less than a minute ago";
                }

                return $"Last update was {(int)timeSpan.TotalMinutes} minute(s) ago";
            }
        }
    }
}