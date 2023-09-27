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
    var projectSrc = 'Projects/Details/';
    var detailsSrc = '/Bugs/Details/';
    var editSrc = '/Bugs/Edit/';

    bugs.map(item => {
        if (item.assignee.avatarPhoto === null) {
            src = '/img/default-avatar.png';
        } else {
            const byteArray = item.assignee.avatarPhoto;
            const binaryString = byteArray.reduce((data, byte) => data + String.fromCharCode(byte), '');
            const base64String = btoa(binaryString);
            src = 'data:image/png;base64,' + base64String;
        }
        detailsSrc += item.id;
        editSrc += item.id;
        projectSrc += item.projectId;

        tbody += `
        <tr>
            <td>
                ${item.title}
                <br />
                <small>Created ${item.createdDate}</small>
            </td>
            <td>
                <a href="${projectSrc}">${item.projectKey}</a>
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
            <td class="project_progress">
                <div class="progress progress-sm">
                    <div class="progress-bar bg-green" role="progressbar" aria-valuenow="57" aria-valuemin="0" aria-valuemax="100" style="width: 57%">
                    </div>
                </div>
                <small>
                    57% Complete
                </small>
            </td>
            <td class="project-state">
                <span class="badge badge-success">${item.status}</span>
            </td>
            <td class="project-actions text-right">
                <a title="View" class="btn btn-primary btn-sm" src="${detailsSrc}">
                    <i class="fa-solid fa-eye">
                    </i>
                                        
                </a>
                <a title="edit" class="btn btn-info btn-sm" src="${editSrc}">
                    <i class="fas fa-pencil-alt">
                    </i> 
                </a>
            </td>
        </tr>
        `
    })
    $("#tbody").html(tbody);
}