using FikaServer.Helpers;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Controllers
{
    [Injectable]
    public class FikaDialogueController(ISptLogger<FikaDialogueController> logger, PlayerRelationsService playerRelationsService, DialogueController dialogueController,
        ProfileHelper profileHelper, PlayerRelationsHelper playerRelationsHelper, SaveServer saveServer,
        HashUtil hashUtil, TimeUtil timeUtil, DialogueHelper dialogueHelper, SptWebSocketConnectionHandler socketConnectionHandler)
    {
        /// <summary>
        /// Gets a list of all friends for the specified profileId
        /// </summary>
        /// <param name="sessionId">The profile id to get the list for</param>
        /// <returns>A new <see cref="GetFriendListDataResponse"/></returns>
        public GetFriendListDataResponse GetFriendsList(string sessionId)
        {
            List<UserDialogInfo> botsAndFriends = dialogueController.GetActiveChatBots();
            List<string> friends = playerRelationsHelper.GetFriendsList(sessionId);

            foreach (string friend in friends)
            {
                PmcData? profile = profileHelper.GetPmcProfile(friend);
                if (profile == null)
                {
                    playerRelationsHelper.RemoveFriend(sessionId, friend);
                    continue;
                }

                botsAndFriends.Add(new()
                {
                    Id = profile.Id,
                    Aid = profile.Aid,
                    Info = new()
                    {
                        Nickname = profile.Info.Nickname,
                        Level = profile.Info.Level,
                        Side = profile.Info.Side,
                        MemberCategory = profile.Info.MemberCategory,
                        SelectedMemberCategory = profile.Info.MemberCategory
                    }
                });
            }

            return new()
            {
                Friends = botsAndFriends,
                Ignore = playerRelationsHelper.GetIgnoreList(sessionId),
                InIgnoreList = playerRelationsHelper.GetInIgnoreList(sessionId)
            };
        }

        public void SendFriendRequest(string from, string to)
        {

        }

        /// <summary>
        /// Sends a message to another player
        /// </summary>
        /// <param name="sessionId">The profile id to send from</param>
        /// <param name="request">The request to handle</param>
        /// <returns>The id of the message sent</returns>
        public string SendMessage(string sessionId, SendMessageRequest request, Dictionary<string, SptProfile> profiles)
        {
            SptProfile receiverProfile = profiles[request.DialogId];
            SptProfile senderProfile = profiles[sessionId];

            if (!senderProfile.DialogueRecords.ContainsKey(request.DialogId))
            {
                senderProfile.DialogueRecords.Add(request.DialogId, new()
                {
                    AttachmentsNew = 0,
                    New = 0,
                    Pinned = false,
                    Type = SPTarkov.Server.Core.Models.Enums.MessageType.UserMessage,
                    Messages = [],
                    Users = [],
                    Id = request.DialogId
                });
            }

            Dialogue senderDialog = senderProfile.DialogueRecords[request.DialogId];
            senderDialog.Users = [
                new()
                {
                    Id = receiverProfile.ProfileInfo.ProfileId,
                    Aid = receiverProfile.ProfileInfo.Aid,
                    Info = new()
                    {
                        Nickname = receiverProfile.CharacterData.PmcData.Info.Nickname,
                        Side = receiverProfile.CharacterData.PmcData.Info.Side,
                        Level = receiverProfile.CharacterData.PmcData.Info.Level,
                        MemberCategory = receiverProfile.CharacterData.PmcData.Info.MemberCategory,
                        SelectedMemberCategory = receiverProfile.CharacterData.PmcData.Info.SelectedMemberCategory
                    }
                },
                new()
                {
                    Id = senderProfile.ProfileInfo.ProfileId,
                    Aid = senderProfile.ProfileInfo.Aid,
                    Info = new()
                    {
                        Nickname = senderProfile.CharacterData.PmcData.Info.Nickname,
                        Side = senderProfile.CharacterData.PmcData.Info.Side,
                        Level = senderProfile.CharacterData.PmcData.Info.Level,
                        MemberCategory = senderProfile.CharacterData.PmcData.Info.MemberCategory,
                        SelectedMemberCategory = senderProfile.CharacterData.PmcData.Info.SelectedMemberCategory
                    }
                }
            ];

            if (!receiverProfile.DialogueRecords.ContainsKey(sessionId))
            {
                receiverProfile.DialogueRecords.Add(sessionId, new()
                {
                    AttachmentsNew = 0,
                    New = 0,
                    Pinned = false,
                    Type = SPTarkov.Server.Core.Models.Enums.MessageType.UserMessage,
                    Messages = [],
                    Users = [],
                    Id = sessionId
                });
            }

            Dialogue receiverDialog = receiverProfile.DialogueRecords[sessionId];
            receiverDialog.New++;
            receiverDialog.Users = [
                new()
                {
                    Id = senderProfile.ProfileInfo.ProfileId,
                    Aid = senderProfile.ProfileInfo.Aid,
                    Info = new()
                    {
                        Nickname = senderProfile.CharacterData.PmcData.Info.Nickname,
                        Side = senderProfile.CharacterData.PmcData.Info.Side,
                        Level = senderProfile.CharacterData.PmcData.Info.Level,
                        MemberCategory = senderProfile.CharacterData.PmcData.Info.MemberCategory,
                        SelectedMemberCategory = senderProfile.CharacterData.PmcData.Info.SelectedMemberCategory
                    }
                },
                new()
                {
                    Id = receiverProfile.ProfileInfo.ProfileId,
                    Aid = receiverProfile.ProfileInfo.Aid,
                    Info = new()
                    {
                        Nickname = receiverProfile.CharacterData.PmcData.Info.Nickname,
                        Side = receiverProfile.CharacterData.PmcData.Info.Side,
                        Level = receiverProfile.CharacterData.PmcData.Info.Level,
                        MemberCategory = receiverProfile.CharacterData.PmcData.Info.MemberCategory,
                        SelectedMemberCategory = receiverProfile.CharacterData.PmcData.Info.SelectedMemberCategory
                    }
                }
            ];

            Message message = new()
            {
                Id = hashUtil.Generate(),
                UserId = sessionId,
                MessageType = request.Type,
                Member = new()
                {
                    Nickname = senderProfile.CharacterData.PmcData.Info.Nickname,
                    Side = senderProfile.CharacterData.PmcData.Info.Side,
                    Level = senderProfile.CharacterData.PmcData.Info.Level,
                    MemberCategory = senderProfile.CharacterData.PmcData.Info.MemberCategory,
                    IsIgnored = playerRelationsHelper
                        .GetInIgnoreList(sessionId)
                        .Contains(request.DialogId),
                    IsBanned = false
                },
                DateTime = timeUtil.GetTimeStamp(),
                Text = request.Text,
                RewardCollected = false
            };

            if (!string.IsNullOrEmpty(request.ReplyTo))
            {
                ReplyTo? replyMessage = GetMessageToReplyTo(request.DialogId, request.ReplyTo, sessionId);
                if (replyMessage != null)
                {
                    message.ReplyTo = replyMessage;
                }
            }

            senderDialog.Messages?.Add(message);
            receiverDialog.Messages?.Add(message);

            // TODO: is this correct?
            socketConnectionHandler.SendMessage(receiverProfile.ProfileInfo.ProfileId, new()
            {
                EventIdentifier = "new_message",
                EventType = NotificationEventType.new_message
            });

            return message.Id;
        }

        /// <summary>
        /// Checks if there is a message to reply to
        /// </summary>
        /// <param name="recipientId">The id of the recipient</param>
        /// <param name="replyToId">The id of the message to reply to</param>
        /// <param name="dialogueId">The id of the dialogue, should be the other sender's profile id</param>
        /// <returns>A new <see cref="ReplyTo"/> containing data for the message to reply to</returns>
        private ReplyTo? GetMessageToReplyTo(string recipientId, string replyToId, string dialogueId)
        {
            Dialogue? currentDialogue = dialogueHelper.GetDialogueFromProfile(recipientId, dialogueId);
            if (currentDialogue == null)
            {
                logger.Warning($"Could not find dialogue {dialogueId} from sender");
                return null;
            }

            Message? message = currentDialogue.Messages
                .Where(x => x.Id == replyToId)
                .FirstOrDefault();

            if (message != null)
            {
                return new()
                {
                    Id = message.Id,
                    UserId = message.UserId,
                    DateTime = message.DateTime,
                    MessageType = message.MessageType,
                    Text = message.Text
                };
            }

            return null;
        }
    }
}
