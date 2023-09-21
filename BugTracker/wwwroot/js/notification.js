export function LoadNotifications() {
    var a = ''
    $.ajax({
        url: '/Home/GetNotifications/',
        method: 'GET',
        success: (result) => {
            console.log('notif.')
            console.log(result)
            $.each(result, (k, v) => {
                var str = new String(v.Controller)
                str = str.substring(0, str.length - 1).toLowerCase();
                a += `<p>new ${str} assigned to you. <a href='/${v.Controller}/Details?id=${v.DetailsID}&user=${v.AssignedUserID}&isRead=${true}'>Read More</a></p>`;
            })
            $("#notifications").html(a);
        },
        error: (error) => {
            console.log(error)
        }
    })
}


//LoadNotifications();


//var connection = new signalR.HubConnectionBuilder()
//    .withUrl("/bugDetailsHub")
//    .withAutomaticReconnect()
//    .configureLogging(signalR.LogLevel.Information)
//    .build();
//async function start() {
//    try {
//        await connection.start();
//        console.log("SignalR connected.");
//    } catch (err) {
//        console.log(err);
//        setTimeout(start, 5000);
//    }
//};
//connection.onclose(async () => {
//    await start();
//});
//start();

//connection.on("GetNotifications", function (notificationCount) {
//    console.log('message:', notificationCount)
//    //var span = document.getElementById("notifCount");
//    //span.textContent = notificationCount;

//    //LoadNotifications();
//})


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