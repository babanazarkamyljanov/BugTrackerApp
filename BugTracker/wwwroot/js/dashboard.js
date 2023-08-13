$(function () {
    'use strict'

    // Make the dashboard widgets sortable Using jquery UI
    $('.connectedSortable').sortable({
        placeholder: 'sort-highlight',
        connectWith: '.connectedSortable',
        handle: '.card-header, .nav-tabs',
        forcePlaceholderSize: true,
        zIndex: 999999
    })
    $('.connectedSortable .card-header').css('cursor', 'move')

    //-------------
    //- PROJECTS STATUS CHART -
    //-------------
    // Get context with jQuery - using jQuery's .get() method.
    var projectsStatusChartCanvas = $('#projectsStatusChart').get(0).getContext('2d')
    var projectsStatusData = {
        labels: [
            'Active',
            'In Progress',
            'Completed',
            'Not Active',
            'Closed'
        ],
        datasets: [
            {
                data: [700, 500, 400, 600, 300, 100],
                backgroundColor: ['#f56954', '#00a65a', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de'],
            }
        ]
    }
    var chartOptions = {
        maintainAspectRatio: false,
        responsive: true,
    }
    new Chart(projectsStatusChartCanvas, {
        type: 'doughnut',
        data: projectsStatusData,
        options: chartOptions
    })

    //-------------
    //- BUGS STATUS CHART -
    //-------------
    // Get context with jQuery - using jQuery's .get() method.
    var bugsStatusChartCanvas = $('#bugsStatusChart').get(0).getContext('2d')
    var bugsStatusData = {
        labels: [
            'Open',
            'Build In Progress',
            'Code Review',
            'Functional Testing',
            'Fixed',
            'Closed'
        ],
        datasets: [
            {
                data: [700, 500, 400, 600, 300, 100, 200],
                backgroundColor: ['#f56954', '#00a65a', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de', '#111688'],
            }
        ]
    };
    new Chart(bugsStatusChartCanvas, {
        type: 'pie',
        data: bugsStatusData,
        options: chartOptions
    })

    //-------------
    //- PROJECTS PRIORITY BAR CHART -
    //-------------
    var barChartData = {
        labels: ['Low', 'Medium', 'High'],
        datasets: [
            {
                label: 'Projects',
                data: [5, 15, 25],
                backgroundColor: ['rgba(0,255,0,1)', 'rgba(0, 0, 255, 1)', 'rgba(255, 0, 0, 1)'],
                borderColor: ['rgba(0,255,0)', 'rgba(0, 0, 255)', 'rgba(255, 0, 0)']

            }
        ]
    }
    var barChartCanvas = $('#projectsPriorityChart').get(0).getContext('2d')
    var barChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            xAxes: [{
                stacked: true,
            }],
            yAxes: [{
                stacked: true
            }]
        }
    }
    new Chart(barChartCanvas, {
        type: 'bar',
        data: barChartData,
        options: barChartOptions
    })

    //-------------
    //- BUGS PRIORITY BAR CHART -
    //-------------
    var bugsBarChartCanvas = $('#bugsPriorityChart').get(0).getContext('2d')
    var bugsBarChartData = {
        labels: ['Low', 'Medium', 'High'],
        datasets: [
            {
                label: 'Bugs',
                data: [5, 15, 25],
                backgroundColor: ['rgba(0,255,0,1)', 'rgba(0, 0, 255, 1)', 'rgba(255, 0, 0, 1)'],
                borderColor: ['rgba(0,255,0)', 'rgba(0, 0, 255)', 'rgba(255, 0, 0)']

            }
        ]
    }
    new Chart(bugsBarChartCanvas, {
        type: 'bar',
        data: bugsBarChartData,
        options: barChartOptions
    })
})
