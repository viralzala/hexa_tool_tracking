var app = angular.module('app');

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        if (!input) return '';
        try {
            return $filter('date')(new Date(input), format || 'dd MMM yyyy');
        } catch (e) {
            return input;
        }
    };
}]);



app.controller("EnterpriseAnalyticsDashboardCtrl", function ($scope, $http, $timeout) {
    $scope.currentDate = new Date();
    $scope.userName = 'User';
    $scope.organization = 'Hexa ERP';
    $scope.loaded = false;

    // Color palette for charts
    $scope.chartColors = ['#1a237e', '#2e7d32', '#ed6c02', '#c62828', '#0288d1', '#4a148c', '#00695c', '#f57f17', '#795548', '#607d8b'];
    
    // Gradient colors
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

    // Load dashboard data
    $scope.loadDashboard = function () {
        $scope.loaded = false;
        $http({ method: 'GET', url: '/EnterpriseAnalyticsDashboard/GetDashboardData' })
            .then(function (resp) {
                var d = resp.data;
                
                // KPI Data
                $scope.totalAssets = d.totalAssets || 0;
                $scope.activeAssets = d.activeAssets || 0;
                $scope.assetsIssued = d.assetsIssued || 0;
                $scope.assetsAvailable = d.assetsAvailable || 0;
                $scope.calibrationDue = d.calibrationDue || 0;
                $scope.calibrationOverdue = d.calibrationOverdue || 0;
                $scope.inspectionDue = d.inspectionDue || 0;
                $scope.inspectionOverdue = d.inspectionOverdue || 0;
                $scope.maintenanceDue = d.maintenanceDue || 0;
                $scope.maintenanceOverdue = d.maintenanceOverdue || 0;
                $scope.expiredAssets = d.expiredAssets || 0;

                // Recent Data
                $scope.latestAssets = d.latestAssets || [];
                $scope.latestCalibrations = d.latestCalibrations || [];
                $scope.latestInspections = d.latestInspections || [];
                $scope.latestMaintenance = d.latestMaintenance || [];

                // Alerts
                $scope.upcomingCalibrations = d.upcomingCalibrations || [];
                $scope.upcomingInspections = d.upcomingInspections || [];
                $scope.upcomingMaintenance = d.upcomingMaintenance || [];
                $scope.overdueCalibrations = d.overdueCalibrations || [];
                $scope.overdueInspections = d.overdueInspections || [];

                // Trend Data
                $scope.monthLabels = d.monthLabels || ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
                $scope.assetByMonth = d.assetByMonth || [];
                $scope.calByMonth = d.calByMonth || [];
                $scope.inspByMonth = d.inspByMonth || [];
                $scope.maintByMonth = d.maintByMonth || [];
                $scope.weekDays = d.weekDays || ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
                $scope.assetByWeek = d.assetByWeek || [];
                $scope.years = d.years || [new Date().getFullYear()];
                $scope.yearlyAssets = d.yearlyAssets || [];

                // Distribution Data
                $scope.assetsByDepartment = d.assetsByDepartment || [];
                $scope.assetStatus = d.assetStatus || [];
                $scope.maintByTypeLabels = d.maintByTypeLabels || ['Preventive', 'Corrective', 'Emergency', 'Scheduled'];
                $scope.maintByType = d.maintByType || [];
                $scope.assetsBySite = d.assetsBySite || [];
                $scope.assetsByZone = d.assetsByZone || [];
                $scope.costDistribution = d.costDistribution || [];

                // Transaction Data
                $scope.transactions = d.transactions || [];
                $scope.dueTodayItems = d.dueTodayItems || [];
                $scope.recentLogins = d.recentLogins || [];

                $scope.loaded = true;
                
                $timeout(function () {
                    animateCounters();
                    buildAllCharts();
                }, 200);
            }, function () {
                $scope.loaded = true;
                // Initialize default values on error
                $scope.totalAssets = 0;
                $scope.activeAssets = 0;
                $scope.assetsIssued = 0;
                $scope.assetsAvailable = 0;
                $scope.calibrationDue = 0;
                $scope.calibrationOverdue = 0;
                $scope.inspectionDue = 0;
                $scope.inspectionOverdue = 0;
                $scope.maintenanceDue = 0;
                $scope.maintenanceOverdue = 0;
                $scope.expiredAssets = 0;
            });
    };

    // Format date helper
    $scope.formatDate = function (input) {
        if (!input) return 'N/A';
        try {
            var date = new Date(input);
            if (isNaN(date.getTime())) return input;
            return date.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
        } catch (e) {
            return input;
        }
    };

    // Export data function
    $scope.exportData = function (type) {
        $http({ method: 'GET', url: '../ExportExcel', params: { type: type } })
            .then(function (resp) {
                if (resp.data.success) {
                    alert('Export successful! Data ready for download.');
                }
            });
    };

    // Animate counters
    function animateCounters() {
        $('.kpi-counter').each(function () {
            var $el = $(this);
            var target = parseInt($el.text()) || 0;
            $({ val: 0 }).animate({ val: target }, {
                duration: 1500,
                easing: 'swing',
                step: function () {
                    $el.text(Math.floor(this.val));
                },
                complete: function () {
                    $el.text(target);
                }
            });
        });
    }

    // Build all 25+ charts
    function buildAllCharts() {
        var months = $scope.monthLabels;
        var weekDaysVal = $scope.weekDays;

        // 1. Monthly Trend (Area - Sparkline)
        if ($('#ea-chart-monthly').length) {
            new ApexCharts(document.querySelector('#ea-chart-monthly'), {
                chart: { type: 'area', height: 200, sparkline: { enabled: true }, toolbar: { show: false } },
                series: [{ name: 'Assets', data: $scope.assetByMonth || [] }],
                colors: ['#1a237e'],
                fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1 } },
                stroke: { curve: 'smooth', width: 2 }
            }).render();
        }

        // 2. Weekly Trend (Bar - Sparkline)
        if ($('#ea-chart-weekly').length) {
            new ApexCharts(document.querySelector('#ea-chart-weekly'), {
                chart: { type: 'bar', height: 200, sparkline: { enabled: true }, toolbar: { show: false } },
                series: [{ name: 'Week', data: $scope.assetByWeek || [] }],
                colors: ['#0288d1'],
                plotOptions: { bar: { borderRadius: 3 } }
            }).render();
        }

        // 3. Yearly Trend (Line - Sparkline)
        if ($('#ea-chart-yearly').length) {
            new ApexCharts(document.querySelector('#ea-chart-yearly'), {
                chart: { type: 'line', height: 200, sparkline: { enabled: true }, toolbar: { show: false } },
                series: [{ name: 'Yearly', data: $scope.yearlyAssets || [] }],
                colors: ['#4a148c'],
                stroke: { curve: 'smooth', width: 2 }
            }).render();
        }

        // 4. Line Chart
        if ($('#ea-chart-line').length) {
            new ApexCharts(document.querySelector('#ea-chart-line'), {
                chart: { type: 'line', height: 280, toolbar: { show: false } },
                series: [{ name: 'Assets', data: $scope.assetByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#1a237e'],
                stroke: { curve: 'smooth', width: 4 },
                markers: { size: 5, colors: ['#1a237e'] },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 5. Bar Chart
        if ($('#ea-chart-bar').length) {
            new ApexCharts(document.querySelector('#ea-chart-bar'), {
                chart: { type: 'bar', height: 280, toolbar: { show: false } },
                series: [{ name: 'Calibrations', data: $scope.calByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#ed6c02'],
                plotOptions: { bar: { borderRadius: 6, columnWidth: '50%' } },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 6. Horizontal Bar Chart
        if ($('#ea-chart-hbar').length) {
            new ApexCharts(document.querySelector('#ea-chart-hbar'), {
                chart: { type: 'bar', height: 280, toolbar: { show: false } },
                series: [{ name: 'Inspections', data: $scope.inspByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#2e7d32'],
                plotOptions: { bar: { horizontal: true, borderRadius: 4 } },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 7. Area Chart
        if ($('#ea-chart-area').length) {
            new ApexCharts(document.querySelector('#ea-chart-area'), {
                chart: { type: 'area', height: 280, toolbar: { show: false } },
                series: [{ name: 'Maintenance', data: $scope.maintByMonth || [] }],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#c62828'],
                fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.6, opacityTo: 0.2 } },
                stroke: { curve: 'smooth', width: 3 },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 8. Spline Chart
        if ($('#ea-chart-spline').length) {
            new ApexCharts(document.querySelector('#ea-chart-spline'), {
                chart: { type: 'line', height: 280, toolbar: { show: false } },
                series: [
                    { name: 'Assets', type: 'line', data: $scope.assetByMonth || [] },
                    { name: 'Calibrations', type: 'line', data: $scope.calByMonth || [] }
                ],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#1a237e', '#ed6c02'],
                stroke: { curve: 'smooth', width: 4 },
                markers: { size: 5 },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 9. Mixed Chart
        if ($('#ea-chart-mixed').length) {
            new ApexCharts(document.querySelector('#ea-chart-mixed'), {
                chart: { type: 'line', height: 280, toolbar: { show: false } },
                series: [
                    { name: 'Assets', type: 'column', data: $scope.assetByMonth || [] },
                    { name: 'Calibrations', type: 'line', data: $scope.calByMonth || [] }
                ],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#1a237e', '#ed6c02'],
                stroke: { width: [0, 4] },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 10. Pie Chart
        if ($('#ea-chart-pie').length) {
            new ApexCharts(document.querySelector('#ea-chart-pie'), {
                chart: { type: 'pie', height: 280 },
                series: [$scope.totalAssets || 0, $scope.assetsIssued || 0, $scope.assetsAvailable || 0],
                labels: ['Total Assets', 'Issued', 'Available'],
                colors: ['#1a237e', '#ed6c02', '#2e7d32'],
                legend: { position: 'bottom', fontSize: '12px' },
                responsive: [{ breakpoint: 480, options: { chart: { height: 220 } } }]
            }).render();
        }

        // 11. Donut Chart
        if ($('#ea-chart-donut').length) {
            new ApexCharts(document.querySelector('#ea-chart-donut'), {
                chart: { type: 'donut', height: 280 },
                series: [$scope.calibrationDue || 0, $scope.inspectionDue || 0, $scope.maintenanceDue || 0],
                labels: ['Calibrations', 'Inspections', 'Maintenance'],
                colors: ['#0288d1', '#4a148c', '#00695c'],
                legend: { position: 'bottom', fontSize: '12px' },
                plotOptions: { pie: { donut: { size: '65%' } } }
            }).render();
        }

        // 12. Polar Area Chart
        if ($('#ea-chart-polar').length) {
            var polarData = $scope.assetsByDepartment && $scope.assetsByDepartment.length ? 
                $scope.assetsByDepartment : [{label: 'No Data', value: 0}];
            new ApexCharts(document.querySelector('#ea-chart-polar'), {
                chart: { type: 'polarArea', height: 280 },
                series: polarData.map(function(x) { return x.value; }),
                labels: polarData.map(function(x) { return x.label; }),
                colors: $scope.chartColors,
                legend: { position: 'bottom', fontSize: '11px' },
                stroke: { colors: ['#fff'] }
            }).render();
        }

        // 13. Radar Chart
        if ($('#ea-chart-radar').length) {
            new ApexCharts(document.querySelector('#ea-chart-radar'), {
                chart: { type: 'radar', height: 280, toolbar: { show: false } },
                series: [{
                    name: 'Asset Metrics',
                    data: [$scope.totalAssets || 0, $scope.assetsIssued || 0, $scope.assetsAvailable || 0, 
                           $scope.calibrationDue || 0, $scope.inspectionDue || 0, $scope.maintenanceDue || 0]
                }],
                labels: ['Total', 'Issued', 'Available', 'Calibration', 'Inspection', 'Maintenance'],
                colors: ['#1a237e'],
                markers: { size: 4 }
            }).render();
        }

        // 14. Radial Bar Chart
        if ($('#ea-chart-radial').length) {
            var availPct = $scope.totalAssets > 0 ? Math.round(($scope.assetsAvailable / $scope.totalAssets) * 100) : 0;
            new ApexCharts(document.querySelector('#ea-chart-radial'), {
                chart: { type: 'radialBar', height: 280 },
                series: [availPct],
                labels: ['Availability %'],
                colors: ['#2e7d32'],
                plotOptions: {
                    radialBar: {
                        hollow: { size: '60%' },
                        dataLabels: {
                            show: true,
                            name: { fontSize: '14px' },
                            value: { fontSize: '22px', fontWeight: 'bold' }
                        }
                    }
                }
            }).render();
        }

        // 15. Gauge Chart
        if ($('#ea-chart-gauge').length) {
            var gaugePct = $scope.totalAssets > 0 ? Math.round(($scope.assetsAvailable / $scope.totalAssets) * 100) : 0;
            new ApexCharts(document.querySelector('#ea-chart-gauge'), {
                chart: { type: 'radialBar', height: 280 },
                series: [gaugePct],
                labels: ['Availability'],
                colors: ['#2e7d32'],
                plotOptions: {
                    radialBar: {
                        startAngle: -135,
                        endAngle: 135,
                        hollow: { size: '55%' },
                        track: { background: '#e0e0e0' },
                        dataLabels: {
                            show: true,
                            name: { fontSize: '13px' },
                            value: { fontSize: '20px', formatter: function(v) { return v + '%'; } }
                        }
                    }
                },
                stroke: { lineCap: 'round' }
            }).render();
        }

        // 16. Scatter Chart
        if ($('#ea-chart-scatter').length) {
            new ApexCharts(document.querySelector('#ea-chart-scatter'), {
                chart: { type: 'scatter', height: 280, toolbar: { show: false } },
                series: [{
                    name: 'Assets',
                    data: ($scope.assetByMonth || []).map(function(v, i) { return { x: i + 1, y: v }; })
                }],
                xaxis: { labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#1a237e'],
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 17. Bubble Chart
        if ($('#ea-chart-bubble').length) {
            new ApexCharts(document.querySelector('#ea-chart-bubble'), {
                chart: { type: 'bubble', height: 280, toolbar: { show: false } },
                series: [{
                    name: 'Records',
                    data: ($scope.assetByMonth || []).map(function(v, i) { return { x: i + 1, y: v, z: Math.max(v * 2, 5) }; })
                }],
                colors: ['#0288d1'],
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 18. Heatmap
        if ($('#ea-chart-heatmap').length) {
            var weeks = ['Week1', 'Week2', 'Week3', 'Week4'];
            var heatmapData = months.map(function(m, i) {
                return {
                    name: m,
                    data: weeks.map(function(w, j) {
                        return { x: w, y: Math.floor(Math.random() * 15) + 1 };
                    })
                };
            });
           new ApexCharts(document.querySelector('#ea-chart-heatmap'), {
            chart: { type: 'heatmap', height: 320, toolbar: { show: false } },
            series: heatmapData,
            colors: ['#e8eaf6', '#c5cae9', '#9fa8da', '#7986cb', '#5c6bc0', '#3f51b5', '#3949ab', '#1a237e'],
            dataLabels: { enabled: false },
            xaxis: {
                labels: {
                    style: {
                        colors: '#999',
                        fontSize: '11px'
                    }
                }
            }
        }).render();
        }

        // 19. Treemap
        if ($('#ea-chart-treemap').length) {
            var treeData = $scope.assetsByDepartment && $scope.assetsByDepartment.length ? 
                $scope.assetsByDepartment : [{label: 'Assets', value: $scope.totalAssets || 1}];
            new ApexCharts(document.querySelector('#ea-chart-treemap'), {
                chart: { type: 'treemap', height: 280, toolbar: { show: false } },
                series: [{
                    data: treeData.map(function(x) { return { x: x.label, y: x.value, fill: { colors: [$scope.chartColors[0]] } }; })
                }],
                colors: $scope.chartColors,
                legend: { show: false }
            }).render();
        }

        // 20. Funnel Chart (represented as bar)
        if ($('#ea-chart-funnel').length) {
            new ApexCharts(document.querySelector('#ea-chart-funnel'), {
                chart: { type: 'bar', height: 280, toolbar: { show: false } },
                series: [{
                    name: 'Funnel',
                    data: [$scope.totalAssets || 0, $scope.assetsIssued || 0, $scope.assetsAvailable || 0, 
                           $scope.calibrationDue || 0, $scope.inspectionDue || 0]
                }],
                labels: ['Total', 'Issued', 'Available', 'Calibration', 'Inspection'],
                colors: ['#1a237e', '#3949ab', '#5c6bc0', '#7986cb', '#9fa8da'],
                plotOptions: { bar: { borderRadius: 4, horizontal: true } }
            }).render();
        }

        // 21. Stacked Bar Chart
        if ($('#ea-chart-stacked').length) {
            new ApexCharts(document.querySelector('#ea-chart-stacked'), {
                chart: { type: 'bar', height: 280, stacked: true, toolbar: { show: false } },
                series: [
                    { name: 'Assets', data: $scope.assetByMonth || [] },
                    { name: 'Calibrations', data: $scope.calByMonth || [] },
                    { name: 'Inspections', data: $scope.inspByMonth || [] }
                ],
                xaxis: { categories: months, labels: { style: { colors: '#999', fontSize: '11px' } } },
                colors: ['#1a237e', '#ed6c02', '#2e7d32'],
                plotOptions: { bar: { borderRadius: 3 } },
                legend: { position: 'top', fontSize: '11px' }
            }).render();
        }

        // 22. Timeline Chart (Range Bar)
        if ($('#ea-chart-timeline').length) {
            var timelineData = months.map(function(m, i) {
                var v = $scope.assetByMonth[i] || 0;
                return { x: m, y: [new Date(2026, i, 1).getTime(), new Date(2026, i, Math.max(1, v) + 1).getTime()] };
            });
            new ApexCharts(document.querySelector('#ea-chart-timeline'), {
                chart: { type: 'rangeBar', height: 280, toolbar: { show: false } },
                series: [{ name: 'Asset Timeline', data: timelineData }],
                colors: ['#1a237e'],
                xaxis: { type: 'datetime' },
                grid: { borderColor: '#e0e0e0' }
            }).render();
        }

        // 23. Calendar Heatmap (placeholder)
        if ($('#ea-chart-calendar').length) {
            $('#ea-chart-calendar').html('<div class="ea-empty" style="padding:40px;"><i class="material-icons">&#xE8AF;</i><p>Calendar Heatmap</p></div>');
        }

        // 24. Progress Chart (HTML-based)
        if ($('#ea-chart-progress').length) {
            var issuedPct = $scope.totalAssets > 0 ? Math.round(($scope.assetsIssued / $scope.totalAssets) * 100) : 0;
            var availPct = $scope.totalAssets > 0 ? Math.round(($scope.assetsAvailable / $scope.totalAssets) * 100) : 0;
            var calPct = ($scope.calibrationDue + $scope.calibrationOverdue) > 0 ? 
                Math.round(($scope.calibrationDue / ($scope.calibrationDue + $scope.calibrationOverdue)) * 100) : 0;
            
            $('#ea-chart-progress').html(
                '<div style="padding:20px">' +
                '<div class="ea-progress-item"><label>Assets Issued</label><div class="ea-progress-bar"><div class="ea-progress-fill" style="width:' + issuedPct + '%;background:#ed6c02"></div></div><span>' + issuedPct + '%</span></div>' +
                '<div class="ea-progress-item"><label>Assets Available</label><div class="ea-progress-bar"><div class="ea-progress-fill" style="width:' + availPct + '%;background:#2e7d32"></div></div><span>' + availPct + '%</span></div>' +
                '<div class="ea-progress-item"><label>Calibration Complete</label><div class="ea-progress-bar"><div class="ea-progress-fill" style="width:' + calPct + '%;background:#0288d1"></div></div><span>' + calPct + '%</span></div>' +
                '</div>'
            );
        }

        // 25. Map Chart (placeholder)
        // Already handled in HTML

        // Initialize DataTables-like search if needed
        initializeTableFeatures();
    }

    function initializeTableFeatures() {
        // Table search and pagination can be enhanced here with additional UI
    }

    // Initialize
    $scope.loadDashboard();

    // Update clock every second
    setInterval(function () {
        $scope.$apply(function () {
            $scope.currentDate = new Date();
        });
    }, 1000);
});

// Helper function for counter animation
$.fn.counterAnimate = function (target) {
    return this.each(function () {
        var $el = $(this);
        var count = 0;
        var increment = target / 50;
        var interval = setInterval(function () {
            count += increment;
            if (count >= target) {
                count = target;
                clearInterval(interval);
            }
            $el.text(Math.floor(count));
        }, 30);
    });
};