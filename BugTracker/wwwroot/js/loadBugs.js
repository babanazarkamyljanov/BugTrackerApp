"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/loadBugsHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

start();

async function start() {
    try {
        await connection.start();
    } catch (err) {
        console.log(err);
    }
};

connection.onclose(async () => {
    await start();
});

connection.on("refreshBugs", function (bugs) {
    loadBugs(bugs);
});

function loadBugs(bugs) {
    var tbody = '';
    var src = '';
    
    bugs.map(item => {
        var detailsSrc = '/Bugs/Details/' + item.id;
        var editSrc = '/Bugs/Edit/' + item.id;
        var projectSrc = 'Projects/Details/' + item.project.id;
        if (item.assignee.avatarPhoto === null) {
            src = '/img/default-avatar.png';
        } else {
            const byteArray = item.assignee.avatarPhoto;
            const binaryString = byteArray.reduce((data, byte) => data + String.fromCharCode(byte), '');
            const base64String = btoa(binaryString);
            src = 'data:image/png;base64,' + base64String;
        }

        tbody += `
        <tr>
            <td>
                ${item.title}
                <br />
                <small>Created ${item.createdDate}</small>
            </td>
            <td>
                <a href="${projectSrc}">${item.project.title}</a>
            </td>
            <td>
                <ul class="list-inline">
                    <li class="list-inline-item">
                        <img src="${src}" class="table-avatar" alt="User Image">
                    </li>
                    <li class="list-inline-item">
                        <h6>${item.assignee.userName}</h6>
                    </li>
                </ul>
            </td>
            <td>
                <h6>${item.priority}</h6>
            </td>
            <td class="project-state">
                <span class="badge badge-success">${item.status}</span>
            </td>
            <td class="project-actions text-right">
                <a title="View" class="btn btn-info btn-sm" href="${detailsSrc}">
                    <i class="fa-solid fa-eye">
                    </i>
                                        
                </a>
                <a title="edit" class="btn btn-warning btn-sm" href="${editSrc}">
                    <i class="fas fa-pencil-alt">
                    </i> 
                </a>
            </td>
        </tr>
        `
    })
    $("#tbody").html(tbody);
}