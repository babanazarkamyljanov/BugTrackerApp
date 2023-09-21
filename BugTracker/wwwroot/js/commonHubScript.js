"use strict";

LoadBugComments();
LoadBugFiles();

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/bugDetailsHub")
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

connection.on("GetBugFiles", function () {
    LoadBugFiles();
});

connection.on("GetBugComments", function () {
    LoadBugComments();
});

function LoadBugFiles() {
    var div = '';
    var bugId = 0;
    if (document.getElementById("bugId") !== null) {
        bugId = document.getElementById("bugId").value;
    }

    $.ajax({
        url: '/Bugs/GetBugFiles/',
        method: 'GET',
        data: 'id=' + bugId,
        success: (files) => {
            files.map(f => {
                div += `
                    <span style="display: flex; flex-direction: column; align-items:center; max-width: 100px;">
                        <a href ="/bugfiles/${f.FileName}">
                            <i class="fa-regular fa-file" style="font-size: 60px;"></i>  
                        </a>
                        <p>
                            ...${f.FileName.substring(f.FileName.length - 8, f.FileName.length + 1)}
                        </p>
                    </span>`
            })
            $("#filesDiv").html(div);

        },
        error: (error) => {
            console.log(JSON.stringify(error))
        }
    });
}

function LoadBugComments() {
    var h2 = '';
    var div = '';
    var bugId = 0;
    if (document.getElementById("bugId") !== null) {
        bugId = document.getElementById("bugId").value;
    }

    $.ajax({
        url: '/Bugs/GetBugComments/',
        method: 'GET',
        data: 'id=' + bugId,
        success: (comments) => {
            h2 += `<h2 class="text-success">Comments (${comments.length})</h2>`;
            $("#commentsCount").html(h2);

            comments.map(c => {
                div += `
                <div class="user-block">
                    <img class="img-circle img-bordered-sm" src="/img/default-avatar.png" alt="user image">
                    <span class="username">
                        <a href="#">${c.Author.UserName}</a>
                    </span>
                    <span class="description">${c.CreatedDate}</span>
                    <p style="margin-top: 1rem;">
                        ${c.Message}
                    </p>
                </div>`
            });
            $("#commentPost").html(div);
        },
        error: (error) => {
            console.log(JSON.stringify(error))
        }
    });
}
