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
    }
}