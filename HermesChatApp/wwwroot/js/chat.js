"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

//filled when button with online user was clicked
var toUser = null;
//filled when onconnected 
var currentUser = null;
//filled on join
var toGroup = null;

connection.on("IdentifyUser", function (user) {
    currentUser = user;
});

connection.on("RecieveOnlineUsers", function (userNames) {
    Array.from(userNames);
    var userNameWindow = document.getElementById("userOnlineList");
    userNameWindow.innerHTML = "";
    userNames.forEach(addOnlineUserNames);
    const scrollingElement = document.getElementsByClassName("sidenavRight")[0];
    scrollingElement.scrollTop = scrollingElement.scrollHeight;
});

function addOnlineUserNames(user) {
    if (user.userIdentifier === currentUser.userIdentifier) {
        return;
    }
    var li = document.createElement("li");
    var button = document.createElement("button");
    // add class to style
    button.className = "user-connected";
    button.textContent = user.name;
    button.onclick = function (event) {
        //dont join if you are already in a chat
        if (toUser === user) {
            return;
        }
        document.getElementById("groupError").innerHTML = "";
        // change header to see with whom you chat
        document.getElementById("chat-intro").innerHTML = `Private Chat with ${user.name}`;
        // allow to send messages and chat
        document.getElementById("sendButton").disabled = false;
        // reset button color
        button.style.backgroundColor = null;
        // user leave group chat
        if (toGroup !== null) {
            connection.invoke("LeaveRoom", toGroup);
            // clean to group
            toGroup = null;
        }
        // user leave private chat
        if (toUser !== null && toUser !== user) {
            connection.invoke("LeavePrivateChat", toUser.name);
        }        
        // set user for this button
        toUser = user;        
        //clean message list
        document.getElementById("messagesList").innerHTML = "";
        // add user to private chat
        connection.invoke("JoinPrivateChat", user.name);
    };
    // append items
    li.appendChild(button);
    document.getElementById("userOnlineList").appendChild(li);
}

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    //send private message
    if (message !== "" && toUser !== null && toGroup === null) {
        connection.invoke("SendPrivateMessage", toUser.name, message).catch(function (err) {
            return console.error(err.toString());
        });
        $("textarea").val("");
    }
    //send message to group
    if (message !== "" && toGroup !== null && toUser === null) {
        connection.invoke("SendMessageGroup", toGroup, message).catch(function (err) {
            return console.error(err.toString());
        });
        $("textarea").val("");
    }
    event.preventDefault();
});

connection.on("MessageNotification", function (user) {
    Array.from(document.getElementsByClassName("user-connected")).forEach(function (button) {
        if (button.textContent === user.name && (toUser === null || user.name !== toUser.name)) {
            button.style.backgroundColor = "#FFA500";
        }
    });
});

connection.on("GetOldMessagesOnJoin", function (messageList) {
    Array.from(messageList);
    var messageWindow = document.getElementById("messagesList");
    messageWindow.innerHTML = "";
    messageList.forEach(addLastTenMessages);
});

function addLastTenMessages(message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.innerHTML = `<div class="one-message ${currentUser.name === message.fromUserName ? "my-message" : ""}">
<img src="/images/user.png"/>
<p class="username"><b>${message.fromUserName}</b></p>
<div class="message">
<p class="text-content"></p>
</div>
<span class="time-right">${message.time}</span>
</div>`;
    li.getElementsByClassName("text-content")[0].textContent = message.message;
    const scrollingElement = document.getElementsByClassName("message-box")[0];
    scrollingElement.scrollTop = scrollingElement.scrollHeight;
}

connection.on("ReceiveMessage", function (user, message, timenow) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.innerHTML = `<div class="one-message ${currentUser.userIdentifier === user.userIdentifier ? "my-message" : ""}">
<img src="/images/user.png"/>
<p class="username"><b>${user.name}</b></p>
<div class="message">
<p class="text-content"></p>
</div>
<span class="time-right">${timenow}</span>
</div>`;
    li.getElementsByClassName("text-content")[0].textContent = message;
    const scrollingElement = document.getElementsByClassName("message-box")[0];
    scrollingElement.scrollTop = scrollingElement.scrollHeight;
});

connection.on("NotifyGroup", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.innerHTML = `<p><center><b> ${user.name} ${message}</b></center></p>`;
    const scrollingElement = document.getElementsByClassName("message-box")[0];
    scrollingElement.scrollTop = scrollingElement.scrollHeight;
});
//validation
document.getElementById("joinGroupButton").addEventListener("click", function (event) {
    var group = document.getElementById("GroupChatName").value;    
    var groups = Array.from(document.getElementsByClassName("group-connected"));

    var listForGroupnames = [];
    groups.forEach(function (groupButton) {
        listForGroupnames.push(groupButton.textContent)
    });
    if (listForGroupnames.includes(group)) {
        var error = document.getElementById("groupError");
        error.textContent = 'Group name already exists';
        error.style = "color: red";
        $("#GroupChatName").val("");
        return;
    }
    else if (group.length > 50) {
        var error = document.getElementById("groupError");
        error.textContent = 'Group name is too long';
        error.style = "color: red";
        $("#GroupChatName").val("");
        return;
    }
    else if (!group || group === '') {
        var error = document.getElementById("groupError");
        error.textContent = 'Please enter group name';
        error.style = "color: red";
        $("#GroupChatName").val("");
        return;
    }
    else {
        var error = document.getElementById("groupError");
        error.textContent = 'Group successfully created!';
        error.style = "color: green";
        connection.invoke("CreateGroup", group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    $("#GroupChatName").val("");
    event.preventDefault();
});

connection.on("AddCreatorToGroup", function (user, groupName) {    
    // change header to see with whom you chat
    document.getElementById("chat-intro").innerHTML = `You are now chatting in ${groupName} room`;
    // allow to send messages and chat
    document.getElementById("sendButton").disabled = false;
    // user leave group chat
    if (toGroup !== null && toGroup !== groupName) {
        connection.invoke("LeaveRoom", toGroup);
    }
    // user leave private chat
    if (toUser !== null) {
        connection.invoke("LeavePrivateChat", toUser.name);
        // clean toUser
        toUser = null;
    }
    // set group for this button
    toGroup = groupName;    
    //clean message list
    document.getElementById("messagesList").innerHTML = "";
});

connection.on("AddToMainChat", function (user, groupName) {
    // change header to see with whom you chat
    document.getElementById("chat-intro").innerHTML = `You are now chatting in General Chat`;
    // allow to send messages and chat
    document.getElementById("sendButton").disabled = false;
    // user leave group chat
    if (toGroup !== null && toGroup !== groupName) {
        connection.invoke("LeaveRoom", toGroup);
    }
    // user leave private chat
    if (toUser !== null) {
        connection.invoke("LeavePrivateChat", toUser.name);
        // clean toUser
        toUser = null;
    }
    // set group for this button
    toGroup = groupName;
    //clean message list
    document.getElementById("messagesList").innerHTML = "";
});

// receiveing groups online
connection.on("RecieveOnlineGroups", function (groupNames) {
    Array.from(groupNames);
    var groupNameWindow = document.getElementById("groupList");
    groupNameWindow.innerHTML = "";
    groupNames.forEach(addOnlineGroupNames);
    const scrollingElement = document.getElementsByClassName("groups-main")[0];
    scrollingElement.scrollTop = scrollingElement.scrollHeight;
});

function addOnlineGroupNames(group) {
    var li = document.createElement("li");
    var button = document.createElement("button");

    // add class to style
    button.className = "group-connected";
    button.textContent = group;
    button.onclick = function (event) {
        //dont join if you are already in a chat
        if (toGroup === group) {
            return;
        }
        document.getElementById("groupError").innerHTML = "";
        // change header to see with whom you chat
        document.getElementById("chat-intro").innerHTML = `You are now chatting in ${group} room`;
        // allow to send messages and chat
        document.getElementById("sendButton").disabled = false;
        // user leave group chat
        if (toGroup !== null && toGroup !== group) {
            connection.invoke("LeaveRoom", toGroup);
        }
        // user leave private chat
        if (toUser !== null) {
            connection.invoke("LeavePrivateChat", toUser.name);
        }
        // set group for this button
        toGroup = group;
        // clean toUser
        toUser = null;
        //clean message list
        document.getElementById("messagesList").innerHTML = "";
        // add user to group chat
        connection.invoke("JoinRoom", group);
    };
    // append items
    li.appendChild(button);
    document.getElementById("groupList").appendChild(li);
}
//Group rooms end

connection.start().then(function () {
    document.getElementById("sendButton").disabled;
}).catch(function (err) {
    return console.error(err.toString());
});