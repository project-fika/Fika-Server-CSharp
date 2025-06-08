using FikaServer.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Dialogue;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Overrides.Services
{
    [Injectable]
    public class DialogueControllerOverrides(ISptLogger<DialogueController> _logger, TimeUtil _timeUtil, DialogueHelper _dialogueHelper,
        NotificationSendHelper _notificationSendHelper, ProfileHelper _profileHelper, ConfigServer _configServer, SaveServer _saveServer,
        LocalisationService _localisationService, MailSendService _mailSendService, IEnumerable<IDialogueChatBot> dialogueChatBots, 
        FikaDialogueController fikaDialogueController) 
        : DialogueController(_logger, _timeUtil, _dialogueHelper, _notificationSendHelper, _profileHelper, _configServer, _saveServer,
            _localisationService, _mailSendService, dialogueChatBots)
    {
        public override GetFriendListDataResponse GetFriendList(string sessionId)
        {
            return fikaDialogueController.GetFriendsList(sessionId);
        }

        public override string SendMessage(string sessionId, SendMessageRequest request)
        {
            return fikaDialogueController.SendMessage(sessionId, request);
        }
    }
}
