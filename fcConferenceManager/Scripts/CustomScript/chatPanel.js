var chatVE = $.connection.chatHub;
var pauseChat = false;

$(document).ready(function ()
{
    $.connection.hub.transportConnectTimeout = 30000;
    $.connection.hub.logging = true;
    $.connection.hub.disconnected(function ()
    {
        if ($.connection.hub.lastError)
        {
            console.log("You has been disconnected. please refresh the page again");
        }
        setTimeout(function ()
        {
            $.connection.hub.start({ transport: ['serverSentEvents', 'foreverFrame', 'longPolling', 'webSockets'] }).done(function ()
            {
                console.log("Connected by ChatPanel, transport = " + $.connection.hub.transport.name);
                var userId = getSelfConnectionId();
                var name = getMyName();
                var uri = window.location.pathname;
                chatVE.server.onconnected(userId, getMySession(), name, uri, false);

            }).fail(function (e) { console.log("Not Connected by ChatPanel, transport error occurred."); });

        }, 5000);// Restart connection after 5 seconds.
    });
    //$.connection.hub.start().done(function ()
    $.connection.hub.start({ transport: ['serverSentEvents', 'foreverFrame', 'longPolling', 'webSockets'] }).done(function ()
    {
        console.log("Connected by ChatPanel, transport = " + $.connection.hub.transport.name);
        var userId = getSelfConnectionId();
        var name = getMyName();
        var uri = window.location.pathname;
        chatVE.server.onconnected(userId, getMySession(), name, uri, true);

        pauseChat = ((getCookie("isChatPaused")).toString() == "1" ? true : false);
        if (typeof setOnLoad === 'function')
            setOnLoad();

    }).fail(function (e) { console.log("Not Connected by ChatPanel, transport error occurred."); });
    /////////////The Section ends here.............

    $("form input").on('keypress', function (e)
    {
        if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13))
        {
            return false;
        }
        else 
        {
            return true;
        }
    });

    chatVE.client.someOneIsTypingForVE = function (senderName, senderId, groupID, sndrChatTp)
    {
        var currentRV = getTarget();
        var chatType = getThisChatType();
        var show = false;

        if (chatType == '1' && sndrChatTp == '1' && currentRV == senderId)
        {
            show = true;
        }
        else if (chatType == '2' && sndrChatTp == '2' && currentRV == groupID)
        {
            show = true;
        }

        if (show)
        {
            $('#dvShowTypingHere p').html(senderName + ' is typing');
            $('#dvShowTypingHere').css('display', 'flex');
        }
        else
            $('#dvShowTypingHere').css('display', 'none');
    }

    chatVE.client.someOneIsTypedForVE = function (senderId)
    {
        var currentRV = getTarget();
        if (currentRV == senderId)
        {
            $('#dvShowTypingHere').css('display', 'none');
        }
    }

    chatVE.client.sendAsyncForVE = function (receiverId, name, ImagRv, message, ChatType, UniqueID, grpName, nick, storedDate = '')
    {
        var myself = getSelfConnectionId();
        var tempVar = getSelectedSponsor();
        var date = formatAMPM(new Date());
        if (pauseChat || (tempVar != '' && tempVar != myself))
        {
            //var tempMsg = { receiverId: receiverId, name: name, ImagRv: ImagRv, message: message, ChatType: ChatType, UniqueID: UniqueID, grpName: grpName, nick: nick, date: date };
            //msgStack.push(tempMsg);
            return;
        }
        var htmlTextMsg =
            '<li>' +
            '<div class="magiVirtualChatMsgWapper">' +
            '<span class="magiVirtualCatListIcon">' +
            '<img alt="userIcon" src="' + ImagRv + '">' +
            '</span>' +
            '<div class="magiVirtualChatMsg magiChatAvatarMsgWrap">' +
            '<span class="magiVirtualChatUser magiChatAvatarName">' + name + '</span>' +
            '<span class="magiVirtualChatUserMsg magiChatAvararMsg">' + message + '</span>' +
            '<span class="magiVirtualChatUserTime magiChatAvatarMsgTime">' +
            '<small>' + (storedDate == '' ? date : storedDate) + '</small>' +
            '<small id=' + UniqueID + ' class="msgStatus"></small>' +
            '</span>' +
            '</div>' +
            '</div>' +
            '</li>';

        var currentReciever = getTarget();
        if ((currentReciever == '') || (currentReciever == receiverId))
        {
            setTarget(receiverId);
            $("#msgsList").append(htmlTextMsg).scrollTop($("#msgsList").prop('scrollHeight'));
        }

        if (ChatType == 1)
        {
            appendToListIfNotThere(receiverId, name, ImagRv, message, ChatType, UniqueID, grpName, nick);
            ///////Send Acknowledgement
            //chatVE.server.sendAcknowledgement(myself, receiverId, UniqueID);
            //chatVE.server.updateMessageStatus(receiverId, UniqueID, 1, getSelfConnectionId());
        }
        document.title = 'MAGI - New message received';

        ////Move it on the top
        MoveToTop(receiverId);
    };

    chatVE.client.showError = function (ErroMsg)
    {
        console.log(ErroMsg);
    };

    chatVE.client.loadHistoryForThisPerson = function (chatHistory)
    {
        try
        {
            clearMsgs();
            if (IsNotUndefinedAndAlsoNotNull(chatHistory))
            {
                var jsn = chatHistory;
                if (jsn.chats.length > 0)
                {
                    $(jsn.chats).each(function (indx, obj)
                    {
                        appendMsgToChat(obj.talks);
                    });
                    $("#msgsList").scrollTop($("#msgsList").prop('scrollHeight'));
                }
            }
            coverChat(false);

            var selectedSponsor = $('#hSelectedSponor.ClientID').val();
            if (IsNotUndefinedAndAlsoNotNull(selectedSponsor) && getSelfConnectionId() != selectedSponsor)
            {
                removeMsgingTools();
            }
            else
            {
                var showControls = { 'display': 'inline-block' };
                $('#belowTypingSection').css(showControls);
                $('#typeAndSend').css(showControls);
                $('ul#msgList').css('height', $('#dvChatBox').height() - 161);
            }
        }
        catch (ex)
        {
            console.log(ex);
        }
    };
});