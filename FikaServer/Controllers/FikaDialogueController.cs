﻿using FikaServer.Helpers;
using FikaServer.Models.Fika.WebSocket;
using FikaServer.Services;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Dialogue;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Utils;
using static FikaServer.Helpers.PlayerRelationsHelper;

namespace FikaServer.Controllers;

[Injectable]
public class FikaDialogueController(ISptLogger<FikaDialogueController> logger, DialogueController dialogueController,
    ProfileHelper profileHelper, PlayerRelationsHelper playerRelationsHelper, SaveServer saveServer,
    TimeUtil timeUtil, DialogueHelper dialogueHelper, SptWebSocketConnectionHandler socketConnectionHandler,
    FriendRequestsService friendRequestsService, HttpResponseUtil httpResponseUtil, ConfigService configService,
    IEnumerable<IDialogueChatBot> dialogueChatBots)
{
    protected readonly List<IDialogueChatBot> _dialogueChatBots = [.. dialogueChatBots];
    private bool _filtered;

    /// <summary>
    /// Gets a list of all friends for the specified profileId
    /// </summary>
    /// <param name="sessionId">The profile id to get the list for</param>
    /// <returns>A new <see cref="GetFriendListDataResponse"/></returns>
    public GetFriendListDataResponse GetFriendsList(MongoId sessionId)
    {
        if (!_filtered && configService.Config.Server.SPT.DisableSPTChatBots)
        {
            var sptBots = dialogueController.GetActiveChatBots();
            for (var i = _dialogueChatBots.Count - 1; i >= 0; i--)
            {
                var bot = _dialogueChatBots[i];
                var chatBot = _dialogueChatBots[i].GetChatBot();
                if (sptBots.Any(x => x.Id == chatBot.Id))
                {
                    _dialogueChatBots.RemoveAt(i);
                }
            }

            _filtered = true;
        }

        List<UserDialogInfo> botsAndFriends = [];
        foreach (var item in _dialogueChatBots)
        {
            botsAndFriends.Add(item.GetChatBot());
        }

        foreach (var friend in playerRelationsHelper.GetFriendsList(sessionId))
        {
            var profile = profileHelper.GetPmcProfile(friend);
            if (profile == null)
            {
                playerRelationsHelper.RemoveFriend(sessionId, friend);
                continue;
            }

            botsAndFriends.Add(new()
            {
                Id = profile.Id.Value,
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

    public FriendRequestSendResponse? AddFriendRequest(MongoId from, MongoId to)
    {
        if (from == default || to == default)
        {
            throw new NullReferenceException($"From or to was null! [From]: {from}, [To]: {to}");
        }

        if (friendRequestsService.HasFriendRequest(from, to))
        {
            logger.Error($"{from} has already sent a request to {to}");
            return null;
        }

        if (!saveServer.ProfileExists(to))
        {
            logger.Error($"{from} tried to send a friend request to {to} who doesn't exist");
            return null;
        }

        // todo: fix async
        friendRequestsService.AddFriendRequest(new()
        {
            Id = new MongoId(),
            From = from,
            To = to,
            Date = timeUtil.GetTimeStamp()
        }).GetAwaiter().GetResult();

        var fromProfile = saveServer.GetProfile(from)
            ?? throw new NullReferenceException($"{from} did not exist in the database");

        socketConnectionHandler.SendMessage(to, new WsFriendListAdd()
        {
            EventIdentifier = new(),
            EventType = NotificationEventType.friendListNewRequest,
            Id = from,
            Profile = fromProfile.ToFriendData()
        });

        return new FriendRequestSendResponse
        {
            Status = BackendErrorCodes.None,
            RequestId = from,
            RetryAfter = 0
        };
    }

    /// <summary>
    /// Sends a message to another player
    /// </summary>
    /// <param name="sessionId">The profile id to send from</param>
    /// <param name="request">The request to handle</param>
    /// <returns>The id of the message sent</returns>
    public string SendMessage(MongoId sessionId, SendMessageRequest request, Dictionary<MongoId, SptProfile> profiles)
    {
        var receiverProfile = profiles[request.DialogId];
        var senderProfile = profiles[sessionId];

        if (!senderProfile.DialogueRecords.ContainsKey(request.DialogId))
        {
            senderProfile.DialogueRecords.Add(request.DialogId, new()
            {
                AttachmentsNew = 0,
                New = 0,
                Pinned = false,
                Type = MessageType.UserMessage,
                Messages = [],
                Users = [],
                Id = request.DialogId
            });
        }

        var senderDialog = senderProfile.DialogueRecords[request.DialogId];
        senderDialog.Users = [
            new()
            {
                Id = receiverProfile.ProfileInfo.ProfileId.Value,
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
                Id = senderProfile.ProfileInfo.ProfileId.Value,
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
                Type = MessageType.UserMessage,
                Messages = [],
                Users = [],
                Id = sessionId
            });
        }

        var receiverDialog = receiverProfile.DialogueRecords[sessionId];
        receiverDialog.New++;
        receiverDialog.Users = [
            new()
            {
                Id = senderProfile.ProfileInfo.ProfileId.Value,
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
                Id = receiverProfile.ProfileInfo.ProfileId.Value,
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
            Id = new MongoId(),
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
            var replyMessage = GetMessageToReplyTo(request.DialogId, request.ReplyTo, sessionId);
            if (replyMessage != null)
            {
                message.ReplyTo = replyMessage;
            }
        }

        senderDialog.Messages?.Add(message);
        receiverDialog.Messages?.Add(message);

        socketConnectionHandler.SendMessage(receiverProfile.ProfileInfo.ProfileId.Value, new WsChatMessageReceived()
        {
            EventIdentifier = new(),
            EventType = NotificationEventType.new_message,
            DialogId = sessionId,
            Message = message
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
        var currentDialogue = dialogueHelper.GetDialogueFromProfile(recipientId, dialogueId);
        if (currentDialogue == null)
        {
            logger.Warning($"Could not find dialogue {dialogueId} from sender");
            return null;
        }

        var message = currentDialogue.Messages
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

    public ValueTask<string> ListInbox(MongoId sessionID)
    {
        var receivedFriendRequests = friendRequestsService.GetReceivedFriendRequests(sessionID);

        foreach (var receivedFriendRequest in receivedFriendRequests)
        {
            var profile = profileHelper.GetPmcProfile(receivedFriendRequest.From);

            if (profile == null)
            {
                continue;
            }

            receivedFriendRequest.Profile = profile.ToFriendData();
        }

        return new(httpResponseUtil.GetBody(receivedFriendRequests));
    }

    public ValueTask<string> ListOutBox(MongoId sessionID)
    {
        var sentFriendRequests = friendRequestsService.GetSentFriendRequests(sessionID);

        foreach (var sentFriendRequest in sentFriendRequests)
        {
            var profile = profileHelper.GetPmcProfile(sentFriendRequest.To);

            if (profile == null)
            {
                continue;
            }

            sentFriendRequest.Profile = profile.ToFriendData();
        }

        return new(httpResponseUtil.GetBody(sentFriendRequests));
    }

    public ValueTask<string> SendFriendRequest(MongoId fromProfileId, MongoId toProfileId)
    {
        return new(httpResponseUtil.GetBody(AddFriendRequest(fromProfileId, toProfileId)));
    }

    public ValueTask<string> AcceptAllFriendRequests(MongoId sessionID)
    {
        var receivedFriendRequests = friendRequestsService.GetReceivedFriendRequests(sessionID);

        foreach (var receivedFriendRequest in receivedFriendRequests)
        {
            if (playerRelationsHelper.RemoveFriendRequest(receivedFriendRequest.From, receivedFriendRequest.To, ERemoveFriendReason.Accept))
            {
                playerRelationsHelper.AddFriend(receivedFriendRequest.From, receivedFriendRequest.To);
            }
        }

        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    /// Accepts a friend request
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public ValueTask<string> AcceptFriendRequest(MongoId sessionID, AcceptFriendRequestData request)
    {
        if (playerRelationsHelper.RemoveFriendRequest(request.ProfileId, sessionID, ERemoveFriendReason.Accept))
        {
            playerRelationsHelper.AddFriend(sessionID, request.ProfileId);
        }

        return new(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> CancelFriendRequest(MongoId fromProfileId, string toProfileId)
    {
        if (toProfileId == null)
        {
            throw new NullReferenceException("Request.ProfileId was null!");
        }

        if (!playerRelationsHelper.RemoveFriendRequest(fromProfileId, toProfileId, ERemoveFriendReason.Cancel))
        {
            logger.Error($"Failed to delete friend request from {fromProfileId} to {toProfileId}");
        }

        return new(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> DeclineFriendRequest(string fromProfileId, MongoId toProfileId)
    {
        playerRelationsHelper.RemoveFriendRequest(fromProfileId, toProfileId, ERemoveFriendReason.Decline);

        return new(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> DeleteFriend(MongoId fromProfileId, MongoId toProfileId)
    {
        playerRelationsHelper.RemoveFriend(fromProfileId, toProfileId);

        return new(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> IgnoreFriend(MongoId fromProfileId, string toProfileId)
    {
        playerRelationsHelper.AddToIgnoreList(fromProfileId, toProfileId);

        return new(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> UnIgnoreFriend(MongoId fromProfileId, string toProfileId)
    {
        playerRelationsHelper.RemoveFromIgnoreList(fromProfileId, toProfileId);

        return new(httpResponseUtil.NullResponse());
    }
}
