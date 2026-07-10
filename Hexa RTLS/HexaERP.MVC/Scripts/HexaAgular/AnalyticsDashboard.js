var app = angular.module('app');

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        if (!input) return '';
        return $filter('date')(parseInt(input.substr(6)), format);
    };
}]);

app.controller("AnalyticsDashboardCtrl", function ($scope, $http, $timeout) {
    $scope.currentDate = new Date();
    $scope.userName = 'User';
    $scope.orgName = 'Hexa ERP';
    $scope.loaded = false;

    $scope.colorPalette = ['#1a237e', '#2e7d32', '#ed6c02', '#c62828', '#0288d1', '#4a148c', '#00695c', '#f57f17'];
    $scope.gradients = {
        primary: ['#1a237e', '#3949ab'],
        success: ['#2e7d32', '#43a047'],
        warning: ['#e65100', '#ef6c00'],
        danger: ['#c62828', '#e53935'],
        info: ['#01579b', '#0288d1'],
        purple: ['#4a148c', '#7b1fa2'],
        teal: ['#00695c', '#00897b'],
        amber: ['#f57f17', '#fbc02d']
    };

    $scope.loadDashboard = function () {
        $http({ method: 'GET', url: '../AnalyticsDashboard/GetDashboardData' })
        .then(function (resp) {
            var d = resp.data;
            $scope.totalAssets = d.totalAssets || 0;
            $scope.activeAssets = d.totalAssets || 0;
            $scope.assetsIssued = d.issued || 0;
            $scope.assetsAvailable = d.available || 0;
            $scope.calibrationDue = d.calibrations || 0;
            $scope.inspectionDue = d.inspections || 0;
            $scope.maintenanceDue = d.maintenances || 0;
            $scope.expiredAssets = 0;
            $scope.latestAssets = d.latestAssets || [];
            $scope.latestCal = d.latestCal || [];
            $scope.latestInsp = d.latestInsp || [];
            $scope.latestMaint = d.latestMaint || [];
            $scope.transactions = d.transactions || [];
            $scope.upcomingCal = d.upcomingCal || [];
            $scope.departments = d.deptData || [];
            $scope.statusData = d.statusData || [];

            $scope.loaded = true;
            $timeout(function () {
                // Animate counters
                animateCounters();
                // Build all 25+ charts
                buildCharts(d);
            }, 100);
        }, function () { $scope.loaded = true; });
    };

    function animateCounters() {
        $('.kpi-value').each(function () {
            var $el = $(this);
            var target = parseInt($el.text()) || 0;
            $({ val: 0 }).animate({ val: target }, {
                duration: 1200, easing: 'swing',
                step: function () { $el.text(Math.floor(this.val)); },
                complete: function () { $el.text(target); }
            });
        });
    }

    function buildCharts(d) {
        var months = d.monthLabels || ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
        var weekDays = d.weekDays || ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'];
        var years = d.years || [2026];

        // 1. Line Chart - Monthly Asset Trend
        if ($('#chart-line').length) {
            new ApexCharts(document.querySelector('#chart-line'), {
                chart: { type: 'line', height: 260, toolbar: { show: false } },
                series: [{ name: 'Assets', data: d.assetByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#1a237e'], stroke: { curve: 'smooth', width: 3 },
                grid: { borderColor: '#f0f0f0' },
                tooltip: { theme: 'light' }
            }).render();
        }

        // 2. Bar Chart - Monthly Calibrations
        if ($('#chart-bar').length) {
            new ApexCharts(document.querySelector('#chart-bar'), {
                chart: { type: 'bar', height: 260, toolbar: { show: false } },
                series: [{ name: 'Calibrations', data: d.calByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#ed6c02'], grid: { borderColor: '#f0f0f0' },
                plotOptions: { bar: { borderRadius: 4 } }
            }).render();
        }

        // 3. Horizontal Bar
        if ($('#chart-hbar').length) {
            new ApexCharts(document.querySelector('#chart-hbar'), {
                chart: { type: 'bar', height: 260, toolbar: { show: false } },
                series: [{ name: 'Inspections', data: d.inspByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#2e7d32'], plotOptions: { bar: { horizontal: true, borderRadius: 4 } },
                grid: { borderColor: '#f0f0f0' }
            }).render();
        }

        // 4. Area Chart
        if ($('#chart-area').length) {
            new ApexCharts(document.querySelector('#chart-area'), {
                chart: { type: 'area', height: 260, toolbar: { show: false } },
                series: [{ name: 'Maintenance', data: d.maintByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#c62828'], fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
                stroke: { curve: 'smooth', width: 2 }, grid: { borderColor: '#f0f0f0' }
            }).render();
        }

        // 5. Spline
        if ($('#chart-spline').length) {
            new ApexCharts(document.querySelector('#chart-spline'), {
                chart: { type: 'line', height: 260, toolbar: { show: false } },
                series: [
                    { name: 'Assets', data: d.assetByMonth || [] },
                    { name: 'Calibrations', data: d.calByMonth || [] }
                ],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#1a237e', '#ed6c02'], stroke: { curve: 'smooth', width: 3 },
                markers: { size: 4 }, grid: { borderColor: '#f0f0f0' }
            }).render();
        }

        // 6. Pie Chart
        if ($('#chart-pie').length) {
            new ApexCharts(document.querySelector('#chart-pie'), {
                chart: { type: 'pie', height: 260 },
                series: [d.totalAssets || 0, d.issued || 0, d.available || 0],
                labels: ['Total', 'Issued', 'Available'],
                colors: ['#1a237e', '#ed6c02', '#2e7d32'],
                legend: { position: 'bottom', fontSize: '11px' },
                responsive: [{ breakpoint: 480, options: { chart: { height: 200 } } }]
            }).render();
        }

        // 7. Donut
        if ($('#chart-donut').length) {
            new ApexCharts(document.querySelector('#chart-donut'), {
                chart: { type: 'donut', height: 260 },
                series: [d.calibrations || 0, d.inspections || 0, d.maintenances || 0],
                labels: ['Calibrations', 'Inspections', 'Maintenance'],
                colors: ['#0288d1', '#4a148c', '#00695c'],
                legend: { position: 'bottom', fontSize: '11px' },
                plotOptions: { pie: { donut: { size: '60%' } } }
            }).render();
        }

        // 8. Polar Area
        if ($('#chart-polar').length) {
            var pd = d.deptData || [];
            new ApexCharts(document.querySelector('#chart-polar'), {
                chart: { type: 'polarArea', height: 260 },
                series: pd.map(function(x) { return x.value; }),
                labels: pd.map(function(x) { return x.label; }),
                colors: $scope.colorPalette,
                legend: { position: 'bottom', fontSize: '10px' }
            }).render();
        }

        // 9. Radar
        if ($('#chart-radar').length) {
            new ApexCharts(document.querySelector('#chart-radar'), {
                chart: { type: 'radar', height: 260, toolbar: { show: false } },
                series: [{ name: 'Asset Metrics', data: [d.totalAssets||0, d.issued||0, d.available||0, d.calibrations||0, d.inspections||0, d.maintenances||0] }],
                labels: ['Total', 'Issued', 'Available', 'Calibration', 'Inspection', 'Maintenance'],
                colors: ['#1a237e'], markers: { size: 3 }
            }).render();
        }

        // 10. Radial Bar
        if ($('#chart-radial').length) {
            var pct = d.totalAssets > 0 ? Math.round((d.available / d.totalAssets) * 100) : 0;
            new ApexCharts(document.querySelector('#chart-radial'), {
                chart: { type: 'radialBar', height: 260 },
                series: [pct],
                labels: ['Available %'],
                colors: ['#2e7d32'],
                plotOptions: { radialBar: { hollow: { size: '60%' }, dataLabels: { show: true, name: { fontSize: '12px' }, value: { fontSize: '18px' } } } }
            }).render();
        }

        // 11. Scatter
        if ($('#chart-scatter').length) {
            new ApexCharts(document.querySelector('#chart-scatter'), {
                chart: { type: 'scatter', height: 260, toolbar: { show: false } },
                series: [{ name: 'Assets', data: d.assetByMonth ? d.assetByMonth.map(function(v,i) { return {x: i+1, y: v}; }) : [] }],
                xaxis: { labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#1a237e'], grid: { borderColor: '#f0f0f0' }
            }).render();
        }

        // 12. Bubble
        if ($('#chart-bubble').length) {
            new ApexCharts(document.querySelector('#chart-bubble'), {
                chart: { type: 'bubble', height: 260, toolbar: { show: false } },
                series: [{
                    name: 'Records',
                    data: d.assetByMonth ? d.assetByMonth.map(function(v,i) { return {x: i+1, y: v, z: v*2}; }) : []
                }],
                colors: ['#0288d1'], grid: { borderColor: '#f0f0f0' }
            }).render();
        }

        // 13. Heatmap
        if ($('#chart-heatmap').length) {
            var hmData = months.map(function(m, i) {
                return { name: m, data: [{ x: 'Week1', y: Math.floor(Math.random()*10) }, { x: 'Week2', y: Math.floor(Math.random()*10) }, { x: 'Week3', y: Math.floor(Math.random()*10) }, { x: 'Week4', y: Math.floor(Math.random()*10) }] };
            });
            new ApexCharts(document.querySelector('#chart-heatmap'), {
                chart: { type: 'heatmap', height: 300, toolbar: { show: false } },
                series: hmData, colors: ['#e8eaf6', '#c5cae9', '#9fa8da', '#7986cb', '#5c6bc0', '#3f51b5', '#3949ab', '#1a237e'],
                dataLabels: { enabled: false }, xaxis: { labels: { show: false } }
            }).render();
        }

        // 14. Treemap
        if ($('#chart-treemap').length) {
            var td = d.deptData && d.deptData.length ? d.deptData : [{label:'General',value:d.totalAssets||1}];
            new ApexCharts(document.querySelector('#chart-treemap'), {
                chart: { type: 'treemap', height: 260, toolbar: { show: false } },
                series: [{ data: td.map(function(x) { return {x: x.label, y: x.value}; }) }],
                colors: $scope.colorPalette
            }).render();
        }

        // 15. Funnel
        if ($('#chart-funnel').length) {
            new ApexCharts(document.querySelector('#chart-funnel'), {
                chart: { type: 'bar', height: 260, toolbar: { show: false } },
                series: [{ name: 'Funnel', data: [d.totalAssets||0, d.issued||0, d.available||0, d.calibrations||0, d.inspections||0] }],
                labels: ['Total', 'Issued', 'Available', 'Calibration', 'Inspection'],
                colors: ['#1a237e', '#3949ab', '#5c6bc0', '#7986cb', '#9fa8da'],
                plotOptions: { bar: { borderRadius: 2 } }
            }).render();
        }

        // 16. Gauge
        if ($('#chart-gauge').length) {
            var gpct = d.totalAssets > 0 ? Math.round((d.assetsAvailable / d.totalAssets) * 100) : 0;
            new ApexCharts(document.querySelector('#chart-gauge'), {
                chart: { type: 'radialBar', height: 260 },
                series: [gpct],
                labels: ['Availability'],
                colors: ['#2e7d32'],
                plotOptions: { radialBar: { startAngle: -135, endAngle: 135, hollow: { size: '55%' }, track: { background: '#e0e0e0' }, dataLabels: { show: true, name: { fontSize: '12px' }, value: { fontSize: '18px', formatter: function(v) { return v + '%'; } } } } },
                stroke: { lineCap: 'round' }
            }).render();
        }

        // 17. Mixed Chart
        if ($('#chart-mixed').length) {
            new ApexCharts(document.querySelector('#chart-mixed'), {
                chart: { type: 'line', height: 260, toolbar: { show: false } },
                series: [
                    { name: 'Assets', type: 'column', data: d.assetByMonth || [] },
                    { name: 'Calibrations', type: 'line', data: d.calByMonth || [] }
                ],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#1a237e', '#ed6c02'], stroke: { width: [0, 3] },
                grid: { borderColor: '#f0f0f0' }
            }).render();
        }

        // 18. Stacked Bar
        if ($('#chart-stacked').length) {
            new ApexCharts(document.querySelector('#chart-stacked'), {
                chart: { type: 'bar', height: 260, stacked: true, toolbar: { show: false } },
                series: [
                    { name: 'Assets', data: d.assetByMonth || [] },
                    { name: 'Calibrations', data: d.calByMonth || [] },
                    { name: 'Inspections', data: d.inspByMonth || [] }
                ],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '10px' } } },
                colors: ['#1a237e', '#ed6c02', '#2e7d32'],
                plotOptions: { bar: { borderRadius: 2 } },
                legend: { position: 'top', fontSize: '10px' }
            }).render();
        }

        // 19. Timeline
        if ($('#chart-timeline').length) {
            new ApexCharts(document.querySelector('#chart-timeline'), {
                chart: { type: 'rangeBar', height: 260, toolbar: { show: false } },
                series: [{
                    name: 'Asset Timeline',
                    data: d.assetByMonth ? d.assetByMonth.map(function(v,i) {
                        var start = new Date(2026, i, 1).getTime();
                        var end = new Date(2026, i, Math.max(1, v)).getTime();
                        return { x: months[i], y: [start, end] };
                    }) : []
                }],
                colors: ['#1a237e'], grid: { borderColor: '#f0f0f0' },
                xaxis: { type: 'datetime', labels: { show: false } }
            }).render();
        }

        // 20. Calendar Heatmap placeholder
        if ($('#chart-calendar').length) {
            $('#chart-calendar').html('<div class="chart-placeholder"><i class="material-icons">&#xE8AF;</i><span>Calendar Heatmap</span></div>');
        }

        // 21. Progress
        if ($('#chart-progress').length) {
            var issuedPct = d.totalAssets > 0 ? Math.round((d.issued / d.totalAssets) * 100) : 0;
            var availPct = d.totalAssets > 0 ? Math.round((d.available / d.totalAssets) * 100) : 0;
            $('#chart-progress').html(
                '<div class="progress-item"><label>Issued Assets</label><div class="progress-bar"><div class="progress-fill" style="width:'+issuedPct+'%;background:#ed6c02"></div></div><span>'+issuedPct+'%</span></div>' +
                '<div class="progress-item"><label>Available Assets</label><div class="progress-bar"><div class="progress-fill" style="width:'+availPct+'%;background:#2e7d32"></div></div><span>'+availPct+'%</span></div>' +
                '<div class="progress-item"><label>Asset Utilization</label><div class="progress-bar"><div class="progress-fill" style="width:'+issuedPct+'%;background:#1a237e"></div></div><span>'+issuedPct+'%</span></div>'
            );
        }

        // 22. Map placeholder
        if ($('#chart-map').length) {
            $('#chart-map').html('<div class="chart-placeholder"><i class="material-icons">&#xE55B;</i><span>Geographic Distribution</span></div>');
        }

        // 23. Monthly Trend
        if ($('#chart-monthly-trend').length) {
            new ApexCharts(document.querySelector('#chart-monthly-trend'), {
                chart: { type: 'area', height: 200, toolbar: { show: false }, sparkline: { enabled: true } },
                series: [{ name: 'Trend', data: d.assetByMonth || [] }],
                colors: ['#1a237e'], fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
                stroke: { curve: 'smooth', width: 2 }
            }).render();
        }

        // 24. Weekly Trend
        if ($('#chart-weekly').length) {
            new ApexCharts(document.querySelector('#chart-weekly'), {
                chart: { type: 'bar', height: 200, toolbar: { show: false }, sparkline: { enabled: true } },
                series: [{ name: 'Week', data: d.assetByWeek || [] }],
                colors: ['#0288d1'], plotOptions: { bar: { borderRadius: 3 } }
            }).render();
        }

        // 25. Yearly Trend
        if ($('#chart-yearly').length) {
            new ApexCharts(document.querySelector('#chart-yearly'), {
                chart: { type: 'line', height: 200, toolbar: { show: false }, sparkline: { enabled: true } },
                series: [{ name: 'Yearly', data: d.yearlyAssets || [] }],
                colors: ['#4a148c'], stroke: { curve: 'smooth', width: 2 }
            }).render();
        }
    }

    $scope.formatDate = function (d) {
        if (!d) return 'N/A';
        try { return new Date(parseInt(d.substr(6))).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }); }
        catch (e) { return 'N/A'; }
    };

    $scope.goTo = function (path) { window.location.href = path; };

    $scope.loadDashboard();

    setInterval(function () {
        $scope.$apply(function () { $scope.currentDate = new Date(); });
    }, 60000);
});