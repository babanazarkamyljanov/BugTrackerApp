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