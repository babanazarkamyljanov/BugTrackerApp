//import { LoadNotifications } from "./notification"
"use strict";

//const LoadNotifications = require('./notification')
LoadProjects();
LoadBugDetails();
LoadNotifications();

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
connection.on("LoadProjectsIndex", function () {
    LoadProjects();
})
connection.on("GetBugDetails", function () {
    LoadBugDetails();
})
connection.on("GetNotifications", function () {
    LoadNotifications();
})
LoadProjects();
LoadBugDetails();
LoadNotifications();


// Notifications
function LoadNotifications() {
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

// Projects/Index
function LoadProjects() {
    var tr = '';
    $.ajax({
        url: '/Projects/GetProjectsIndex/',
        method: 'GET',
        success: (result) => {
            $.each(result, (k, v) => {
                tr += `<tr class="Search">
                    <td>${v.Title}</td>
                    <td>${v.Description}</td>
                    <td>${v.Priority}</td>
                    <td>${v.CreatedDate}</td>
                    <td>
                        <a href='../Projects/Edit?id=${v.ProjectId}'>Edit</a>
                        <a href='../Projects/Details?id=${v.ProjectId}'>Details</a>
                        <a href='../Projects/Delete?id=${v.ProjectId}'>Delete</a>
                    </td>
                    </tr>`
            })
            $("#tableBody").html(tr);
        },
        error: (error) => {
            console.log(error)
        }
    })
}

// Bugs/Details
var pageNumOfComments = 1
var pageNumOfHistories = 1
var pageNumOfFiles = 1
var commentsPageIndex, commentsTotalPages, historyPageIndex, historyTotalPages, filesPageIndex, filesTotalPages;

function LoadBugDetails() {
    var tr = '';
    var table1 = '';
    var table2 = '';
    var a = '';
    var id = 1;
    document.getElementById("bugId") !== null ? (id = document.getElementById("bugId").value) : (id = 1);
    $.ajax({
        url: '/Bugs/GetBugDetails/',
        method: 'GET',
        data: 'id=' + id + '&pageNumOfComments=' + pageNumOfComments + '&pageNumOfFiles=' + pageNumOfFiles + '&pageNumOfHistories=' + pageNumOfHistories,
        success: (result) => {
            console.log('result')
            console.log(result)  
            var date = ''

            // bug details
            table1 += `
            <table>
                <tr><th>Title</th></tr>
                <tr><td>${result.Bug.Title}</td></tr>
                <tr><th>Description</th></tr>
                <tr><td>${result.Bug.Description}</td></tr>
                <tr><th>Status</th></tr>
                <tr><td>${result.Bug.Status}</td></tr>
                <tr><th>Priority</th></tr>
                <tr><td>${result.Bug.Priority}</td></tr>
            </table>`
            $("#leftSide").html(table1);
            date = new Date(result.Bug.CreatedDate).toLocaleDateString();
            table2 += `
                <table>
                    <tr><th>Project</th></tr>
                    <tr><td>${result.Bug.Project.Title}</td></tr>
                    <tr><th>Submitted By</th></tr>
                    <tr><td>${result.Bug.Submitter.FirstName}</td></tr>
                    <tr><th>Created Date</th></tr>
                    <tr><td>${date}</td></tr>
                    <tr><th>Last Updated Date</th></tr>
                    <tr><td>${2022}</td></tr>
                </table>`
            $("#rightSide").html(table2);

            
            // comments
            commentsPageIndex = parseInt(`${result.CommentsPageIndex}`)
            commentsTotalPages = parseInt(`${result.CommentsTotalPages}`)
            $.each(result.PaginatedComments, (k, v) => {
                date = new Date(v.CreatedDate).toLocaleDateString();
                tr += `<tr>
                <td>${v.Author.FirstName} ${v.Author.LastName}</td>
                <td>${v.Message}</td>
                <td>${date}</td>
                </tr>`
            })
            $("#commentsTableBody").html(tr);

            let prevComments = ''
            let nextComments = ''
            if (commentsPageIndex > 1) {
                prevComments = 'enabled'
            } else {
                prevComments = 'disabled'
            }
            if (commentsPageIndex < commentsTotalPages) {
                nextComments = 'enabled'
            } else {
                nextComments = 'disabled'
            }
            a += `
            <a onclick="prev()" class="btn btn-default ${prevComments}">
                Previous
            </a>
            <a onclick="next()" class="btn btn-default ${nextComments}">
                Next
            </a>
            <label style="margin-left: 25px;">Page: ${commentsPageIndex} of ${commentsTotalPages}</label>`
                
            $("#prevNext").html(a);




            // bug histories
            historyPageIndex = parseInt(`${result.HistoryPageIndex}`)
            historyTotalPages = parseInt(`${result.HistoryTotalPages}`)
            tr = ''
            $.each(result.PaginatedHistory, (k, v) => {
                date = new Date(v.DateChanged).toLocaleDateString();
                tr += `<tr>
                <td>${v.Property}</td>
                <td>${v.OldValue}</td>
                <td>${v.NewValue}</td>
                <td>${date}</td>
                </tr>`
            })
            $("#historyBody").html(tr);

            let prevHistory = ''
            let nextHistory = ''

            if (historyPageIndex > 1) {
                prevHistory = 'enabled'
            } else {
                prevHistory = 'disabled'
            }
            if (historyPageIndex < historyTotalPages) {
                nextHistory = 'enabled'
            } else {
                nextHistory = 'disabled'
            }
            a = ''
            a += `
            <a onclick="prevHistory()" class="btn btn-default ${prevHistory}">
                Previous
            </a>
            <a onclick="nextHistory()" class="btn btn-default ${nextHistory}">
                Next
            </a>
            <label style="margin-left: 25px;">Page: ${historyPageIndex} of ${historyTotalPages}</label>`
            $("#prevNextHistory").html(a);



            // bug files
            tr = ''
            filesPageIndex = parseInt(`${result.FilesPageIndex}`)
            filesTotalPages = parseInt(`${result.FilesTotalPages}`)
            $.each(result.PaginatedFiles, (k, v) => {
                console.log(v.Description)
                tr += `<tr>
                <td>${v.Description}</td>
                <td><a href="~/Files/'${v.FileName}'">${v.FileName}</a></td>
                </tr>`
            })
            $("#filesBody").html(tr);

            let prevFiles = ''
            let nextFiles = ''
            if (filesPageIndex > 1) {
                prevFiles = 'enabled'
            } else {
                prevFiles = 'disabled'
            }
            if (filesPageIndex < filesTotalPages) {
                nextFiles = 'enabled'
            } else {
                nextFiles = 'disabled'

            }
            a = ''
            a += `
            <a onclick="prevFiles()" class="btn btn-default ${prevFiles}">
                Previous
            </a>
            <a onclick="nextFiles()" class="btn btn-default ${nextFiles}">
                Next
            </a>
            <label style="margin-left: 25px;">Page: ${filesPageIndex} of ${filesTotalPages}</label>`
            $("#prevNextFiles").html(a);
        },
        error: (error) => {
            console.log(JSON.stringify(error))
        }
    })
}


function prev() {
    console.log('prev clicked')
    pageNumOfComments = --commentsPageIndex
    LoadBugDetails()
}
function next() {
    console.log('next clicked')
    pageNumOfComments = ++commentsPageIndex
    console.log(pageNumOfComments)
    LoadBugDetails()
}

function prevHistory() {
    console.log('prev clicked')
    pageNumOfHistories = --historyPageIndex
    LoadBugDetails()
}
function nextHistory() {
    console.log('next clicked')
    pageNumOfHistories = ++historyPageIndex
    LoadBugDetails()
}
function prevFiles() {
    console.log('prev clicked')
    pageNumOfFiles = --filesPageIndex
    LoadBugDetails()
}
function nextFiles() {
    console.log('next clicked')
    pageNumOfFiles = ++filesPageIndex
    console.log(pageNumOfFiles)
    LoadBugDetails()
}