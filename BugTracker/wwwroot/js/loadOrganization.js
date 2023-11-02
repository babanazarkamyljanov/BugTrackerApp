"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/loadOrganizationHub")
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

connection.on("refreshOrganization", function (title) {
    refreshOrganization(title);
});

function refreshOrganization(title) {
    console.log(title);
    var h = document.getElementById('organizationName');
    h.innerText = title;

    $('#editForm')[0].reset();
    $('#commentModal').modal('hide');
}