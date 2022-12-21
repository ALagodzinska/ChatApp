using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HermesChatApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public static List<HubUser> connectedUsers = new List<HubUser>();
        private static MessageDictionary _messageDictionary = new MessageDictionary();
        private static GroupDictionary _groupDictionary = new GroupDictionary();

        public override async Task OnConnectedAsync()
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            //lock this list while completing method
            lock (connectedUsers)
            {
                if (connectedUsers.Find(x => x.Name == hubUser.Name) == null)
                {
                    connectedUsers.Add(hubUser);
                }
            }

            await Clients.Caller.SendAsync("IdentifyUser", hubUser);
            await Clients.All.SendAsync("RecieveOnlineUsers", connectedUsers);
            await Clients.All.SendAsync("RecieveOnlineGroups", _groupDictionary.GetListOfGroups());
            //add user to general chat
            var generalChatName = "GeneralDefaultChat";
            await Groups.AddToGroupAsync(Context.ConnectionId, generalChatName);
            await Clients.User(hubUser.UserIdentifier).SendAsync("AddToMainChat", hubUser, generalChatName);
            await Clients.Group(generalChatName).SendAsync("NotifyGroup", hubUser, " joined General Chat").ConfigureAwait(true);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var hubUser = connectedUsers.Find(x => x.UserIdentifier == Context.UserIdentifier);
            lock (connectedUsers)
            {                
                if(hubUser != null)
                {
                    connectedUsers.Remove(hubUser);
                }                
            }
            _groupDictionary.RemoveUserFromGroup(hubUser);
            await Clients.All.SendAsync("RecieveOnlineGroups", _groupDictionary.GetListOfGroups());
            await Clients.All.SendAsync("RecieveOnlineUsers", connectedUsers);           
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinPrivateChat(string toUser)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            var groupName = CreatePrivateGroupName(hubUser.Name, toUser);
            var messageList = _messageDictionary.GetLastMessageList(groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (messageList != null)
            {
                await Clients.User(hubUser.UserIdentifier).SendAsync("GetOldMessagesOnJoin", messageList);
            }
            await Clients.Group(groupName).SendAsync("NotifyGroup", hubUser, " is here! ").ConfigureAwait(true);
        }

        public async Task SendPrivateMessage(string toUser, string message)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            //find user in a list to get user identifier 
            var foundToUser = connectedUsers.FirstOrDefault(x => x.Name == toUser);
            //time when message was sent
            var timeNow = DateTime.Now;
            var groupName = CreatePrivateGroupName(hubUser.Name, toUser);

            var saveMessage = new HubMessage()
            {
                FromUserName = hubUser.Name,
                Time = timeNow.ToString("HH:mm:ss"),
                Message = message
            };

            _messageDictionary.Add(groupName, saveMessage);

            await Clients.Group(groupName).SendAsync("ReceiveMessage", hubUser, message, timeNow.ToString("HH:mm:ss"));
            //send notification to user
            await Clients.User(foundToUser.UserIdentifier).SendAsync("MessageNotification", hubUser);
        }

        public string CreatePrivateGroupName(string userFrom, string userTo)
        {
            //compare two usernames to make same chat naming for both users
            var compareNames = string.Compare(userFrom, userTo);
            if (compareNames > 0)
            {
                var groupName = $"{userFrom}-{userTo}";
                return groupName;
            }
            else
            {
                var groupName = $"{userTo}-{userFrom}";
                return groupName;
            }
        }

        public async Task SendMessageGroup(string toGroup, string message)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            //time when message was sent
            var timeNow = DateTime.Now;

            await Clients.Group(toGroup).SendAsync("ReceiveMessage", hubUser, message, timeNow.ToString("HH:mm:ss"));
        }

        public async Task CreateGroup(string groupName)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };
            //add to dictionary
            _groupDictionary.Add(groupName, hubUser);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.All.SendAsync("RecieveOnlineGroups", _groupDictionary.GetListOfGroups());
            //add him immediatly to this chat
            await Clients.User(hubUser.UserIdentifier).SendAsync("AddCreatorToGroup", hubUser, groupName);
        }

        public async Task JoinRoom(string groupName)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _groupDictionary.Add(groupName, hubUser);
            await Clients.All.SendAsync("RecieveOnlineGroups", _groupDictionary.GetListOfGroups());
            await Clients.Group(groupName).SendAsync("NotifyGroup", hubUser, " joined " + groupName).ConfigureAwait(true);
        }

        public async Task LeaveRoom(string groupName)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            //remove from dictionary
            _groupDictionary.Remove(groupName, hubUser);
            await Clients.All.SendAsync("RecieveOnlineGroups", _groupDictionary.GetListOfGroups());
            if(groupName == "GeneralDefaultChat")
            {
                await Clients.Group(groupName).SendAsync("NotifyGroup", hubUser, " left General Chat").ConfigureAwait(true);
            }
            else
            {
                await Clients.Group(groupName).SendAsync("NotifyGroup", hubUser, " left " + groupName).ConfigureAwait(true);
            }            
        }

        public async Task LeavePrivateChat(string toUserName)
        {
            var hubUser = new HubUser()
            {
                UserIdentifier = Context.UserIdentifier,
                Name = Context.User.Identity.Name
            };

            var groupName = CreatePrivateGroupName(hubUser.Name, toUserName);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("NotifyGroup", hubUser, " left private chat! Bye, Bye! ").ConfigureAwait(true);
        }
    }
}

