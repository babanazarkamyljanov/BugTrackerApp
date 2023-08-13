"use strict";

LoadBugDetails();
//LoadNotifications();

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/commonHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
async function start() {
    try {
        await connection.start();
        console.log("SignalR connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};
connection.onclose(async () => {
    await start();
});
// start the connection
start();
connection.on("GetBugDetails", function () {
    console.log("comment added, signalr called");
    LoadBugDetails();
})
//connection.on("GetNotifications", function (notificationCount) {
//    console.log('message:', notificationCount)
//    //var span = document.getElementById("notifCount");
//    //span.textContent = notificationCount;

//    //LoadNotifications();
//})
LoadBugDetails();
//LoadNotifications();


// Notifications
//function LoadNotifications() {
//    console.log('loadnotif called')
//    var a = ''
//    var arr = []
//    $.ajax({
//        url: '/Home/GetNotifications/',
//        method: 'GET',
//        success: (result) => {
//            arr = result
//            console.log(result)
//            var div = document.getElementById("notifications");
//            var body = document.getElementById("notifBody");
//            body.innerText=""
//            var span = document.getElementById("notifCount");
//            //console.log("notif count:", result[0].AssignedUser.NotificationCount)
//            if (result.length > 0) {
//                if (result[0].AssignedUser.NotificationCount > 0)
//                    span.textContent = result[0].AssignedUser.NotificationCount;
//                else
//                    span.textContent = ""
//            } else {
//                span.textContent = "";
//            }

//            var h = document.getElementById("notifHeader");
//            h.textContent = "All Notifications";

//            $.each(result, (k, v) => {
//                //var p = document.createElement("p");
//                //p.textContent = v.Controller + v.DetailsID + v.AssignedUserID
//                //body.appendChild(p);

//                var str = new String(v.Controller)
//                str = str.substring(0, str.length - 1).toLowerCase();
//                a += `<p>new ${str} assigned to you. <a href='/${v.Controller}/Details?id=${v.DetailsID}&user=${v.AssignedUserID}&isRead=${true}'>Read More</a></p>`;        

//            })
//            //div.appendChild(body)
//            $("#notifBody").html(a);
//        },
//        error: (error) => {
//            console.log(error)
//        }
//    })
//    //var div = document.getElementById("notifications");
//    //arr.forEach((element, index, array) => {
//    //    var p = document.createElement("p");
//    //    console.log(element.Controller, element.DetailsID, element.AssignedUserID)
//    //    p.textContent = element.Controller + element.DetailsID + element.AssignedUserID
//    //    div.appendChild(p);
//    //})
//}

// Bugs/Details
function LoadBugDetails() {
    var h2 = '';
    var commentsDiv = '';
    var filesDivBody = '';
    var date = ''
    var id = 1;
    document.getElementById("bugId") !== null ? (id = document.getElementById("bugId").value) : (id = 1);
    $.ajax({
        url: '/Bugs/GetBugDetails/',
        method: 'GET',
        data: 'id=' + id,
        success: (result) => {
            h2 += `<h2 class="text-success">Comments (${result.Comments.length})</h2>`
            $("#commentsCount").html(h2)

            $.each(result.Comments, (k, v) => {
                date = new Date(v.CreatedDate).toLocaleDateString();

                commentsDiv += `
                <div class="user-block">
                    <img class="img-circle img-bordered-sm" src="/img/default-avatar.png" alt="user image">
                    <span class="username">
                        <a href="#">${v.Author.UserName}</a>
                    </span>
                    <span class="description">${date}</span>
                    <p>
                        ${v.Message}
                    </p>
                </div>`
             })
            $("#commentPost").html(commentsDiv);

            $.each(result.Files, (k, v) => {
                filesDivBody += `
                    <span style="display: flex; flex-direction: column; max-width: 65px;">
                        <a href ="/files/${v.FileName}">
                            <i class="fa-regular fa-file" style="font-size: 60px;"></i>  
                        </a>
                        ${v.FileName}
                    </span>`
            })
            $("#filesDiv").html(filesDivBody);

        },
        error: (error) => {
            console.log(JSON.stringify(error))
        }
    })
}
