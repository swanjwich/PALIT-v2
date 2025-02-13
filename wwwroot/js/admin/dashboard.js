$(document).ready(function () {
    // start: Charts
    const labels = [
        'January',
        'February',
        'March',
        'April',
        'May',
        'June',
    ];

    const salesChart = new Chart($('#sales-chart'), {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                backgroundColor: '#6610f2',
                data: [5, 10, 5, 2, 20, 30, 45],
            }]
        },
        options: {
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    })

    const visitorsChart = new Chart($('#visitors-chart'), {
        type: 'doughnut',
        data: {
            labels: ['Children', 'Teenager', 'Parent'],
            datasets: [{
                backgroundColor: ['#6610f2', '#198754', '#ffc107'],
                data: [40, 60, 80],
            }]
        }
    })
    // end: Charts
})