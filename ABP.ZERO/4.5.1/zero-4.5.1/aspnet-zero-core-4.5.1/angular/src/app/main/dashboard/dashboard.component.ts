import { Component, AfterViewInit, Injector, ViewEncapsulation, OnDestroy } from '@angular/core';
import { TenantDashboardServiceProxy } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppSalesSummaryDatePeriod } from '@shared/AppEnums';
declare let d3, Datamap: any;
import * as _ from "lodash";

@Component({
    templateUrl: './dashboard.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class DashboardComponent extends AppComponentBase implements AfterViewInit, OnDestroy {

    appSalesSummaryDateInterval = AppSalesSummaryDatePeriod;
    selectedSalesSummaryDatePeriod: any = AppSalesSummaryDatePeriod.Daily;
    dashboardHeaderStats: DashboardHeaderStats;
    salesSummaryChart: SalesSummaryChart;
    regionalStatsWorldMap: RegionalStatsWorldMap;
    generalStatsPieChart: GeneralStatsPieChart;
    serverStatsLineChart: ServerStatsLineChart;
    activitiesTimeline: ActivitiesTimeline;
    memberActivityTable: MemberActivityTable;

    constructor(
        injector: Injector,
        private _dashboardService: TenantDashboardServiceProxy
    ) {
        super(injector);
        this.dashboardHeaderStats = new DashboardHeaderStats();
        this.salesSummaryChart = new SalesSummaryChart(this._dashboardService, "salesStatistics");
        this.regionalStatsWorldMap = new RegionalStatsWorldMap(this._dashboardService, "worldmap");
        this.generalStatsPieChart = new GeneralStatsPieChart(this._dashboardService);
        this.serverStatsLineChart = new ServerStatsLineChart(this._dashboardService, "#network", "#cpu-load", "#load-rate");
        this.activitiesTimeline = new ActivitiesTimeline("#timelineDetails", "#timelineDateList", "#horizontalTimelineContainer");
        this.memberActivityTable = new MemberActivityTable(this._dashboardService);
    }

    getDashboardStatisticsData(datePeriod): void {
        this.salesSummaryChart.showLoading();
        this.generalStatsPieChart.showLoading();
        this.activitiesTimeline.showLoading();

        this._dashboardService
            .getDashboardData(datePeriod)
            .subscribe(result => {
                this.dashboardHeaderStats.init(result.totalProfit, result.newFeedbacks, result.newOrders, result.newUsers);
                this.generalStatsPieChart.init(result.transactionPercent, result.newVisitPercent, result.bouncePercent);
                this.serverStatsLineChart.init(result.networkLoad, result.cpuLoad, result.loadRate);
                this.activitiesTimeline.init(result.timeLineItems);
                this.salesSummaryChart.init(result.salesSummary, result.totalSales, result.revenue, result.expenses, result.growth);
            });
    };

    ngAfterViewInit(): void {
        this.getDashboardStatisticsData(AppSalesSummaryDatePeriod.Daily);
        this.regionalStatsWorldMap.draw(true);
        this.memberActivityTable.init();
    }

    ngOnDestroy() {
        this.regionalStatsWorldMap.dispose();
    }
};


abstract class DashboardChartBase {
    loading: boolean = true;

    showLoading() {
        setTimeout(() => { this.loading = true; });
    }

    hideLoading() {
        setTimeout(() => { this.loading = false; });
    }
};

class SalesSummaryChart extends DashboardChartBase {
    //Sales summary => MorrisJs: https://github.com/morrisjs/morris.js/

    instance: morris.GridChart;
    totalSales: number = 0; totalSalesCounter: number = 0;
    revenue: number = 0; revenuesCounter: number = 0;
    expenses: number = 0; expensesCounter: number = 0;
    growth: number = 0; growthCounter: number = 0;

    constructor(private _dashboardService: TenantDashboardServiceProxy, private _containerElement: any) {
        super();
    }

    init(salesSummaryData, totalSales, revenue, expenses, growth) {
        this.instance = Morris.Area({
            element: this._containerElement,
            padding: 0,
            behaveLikeLine: false,
            gridEnabled: false,
            gridLineColor: "transparent",
            axes: false,
            fillOpacity: 1,
            data: salesSummaryData,
            lineColors: ['#399a8c', '#92e9dc'],
            xkey: 'period',
            ykeys: ['sales', 'profit'],
            labels: ['Sales', 'Profit'],
            pointSize: 0,
            lineWidth: 0,
            hideHover: 'auto',
            resize: true
        });

        this.totalSales = totalSales;
        this.totalSalesCounter = totalSales;

        this.revenue = revenue;
        this.expenses = expenses;
        this.growth = growth;

        this.hideLoading();
    };

    reload(datePeriod) {
        this.showLoading();
        this._dashboardService
            .getSalesSummary(datePeriod)
            .subscribe(result => {
                this.instance.setData(result.salesSummary, true);
                this.hideLoading();
            });
    }
}

class RegionalStatsWorldMap extends DashboardChartBase {
    //World map => DataMaps: https://github.com/markmarkoh/datamaps/

    private _worldMap;
    private colors: any = d3.scale.category10();
    private refreshIntervalId: number;

    worldMap = element => {
        let instance: any;
        var init = data => new Datamap({
            element: document.getElementById(element),
            projection: 'mercator',
            fills: {
                defaultFill: "#ABDDA4",
                key: "#fa0fa0"
            },
            data: data,
            done(datamap) {
                const redraw = () => {
                    datamap.svg.selectAll("g").attr("transform", "translate(" + d3.event.translate + ")scale(" + d3.event.scale + ")");
                };

                datamap.svg.call(d3.behavior.zoom().on("zoom", redraw));
            }
        });

        var redraw = () => {
            this._dashboardService
                .getWorldMap({})
                .subscribe(result => {
                    var mapData = {};
                    for (var i = 0; i < result.countries.length; i++) {
                        var country = result.countries[i];
                        mapData[country.countryName] = this.colors(Math.random() * country.color);
                    }

                    instance.updateChoropleth(mapData);
                });

        };

        var draw = data => {
            if (!instance) {
                instance = init(data);
            } else {
                instance.redraw();
            }
        };

        return {
            draw: draw,
            redraw: redraw
        }
    };

    constructor(private _dashboardService: TenantDashboardServiceProxy, private _containerElement) {
        super();
        this._worldMap = this.worldMap(this._containerElement);
    }

    draw(isAutoReload = false, reloadInterval = 3000) {
        this._worldMap.draw({
            USA: { fillKey: "key" },
            JPN: { fillKey: "key" },
            ITA: { fillKey: "key" },
            CRI: { fillKey: "key" },
            KOR: { fillKey: "key" },
            DEU: { fillKey: "key" },
            TUR: { fillKey: "key" },
            RUS: { fillKey: "key" }
        });

        if (isAutoReload) {
            this.reloadEvery(reloadInterval);
        }

        this.hideLoading();
    };

    dispose() {
        if (!this.refreshIntervalId) {
            return;
        }

        clearInterval(this.refreshIntervalId);
    };

    reloadEvery(milliseconds) {
        this.refreshIntervalId = setInterval(() => {
            this._worldMap.redraw();
        }, milliseconds);
    }
}

class GeneralStatsPieChart extends DashboardChartBase {
    //General stats =>  EasyPieChart: https://rendro.github.io/easy-pie-chart/

    transactionPercent = {
        value: 0,
        options: {
            barColor: '#F8CB00',
            trackColor: '#f9f9f9',
            scaleColor: '#dfe0e0',
            scaleLength: 5,
            lineCap: 'round',
            lineWidth: 3,
            size: 75,
            rotate: 0,
            animate: {
                duration: 1000,
                enabled: true
            }
        }
    };
    newVisitPercent = {
        value: 0,
        options: {
            barColor: '#1bbc9b',
            trackColor: '#f9f9f9',
            scaleColor: '#dfe0e0',
            scaleLength: 5,
            lineCap: 'round',
            lineWidth: 3,
            size: 75,
            rotate: 0,
            animate: {
                duration: 1000,
                enabled: true
            }
        }
    };
    bouncePercent = {
        value: 0,
        options: {
            barColor: '#F3565D',
            trackColor: '#f9f9f9',
            scaleColor: '#dfe0e0',
            scaleLength: 5,
            lineCap: 'round',
            lineWidth: 3,
            size: 75,
            rotate: 0,
            animate: {
                duration: 1000,
                enabled: true
            }
        }
    };

    constructor(private _dashboardService: TenantDashboardServiceProxy) {
        super();
    }

    init(transactionPercent, newVisitPercent, bouncePercent) {
        this.transactionPercent.value = transactionPercent;
        this.newVisitPercent.value = newVisitPercent;
        this.bouncePercent.value = bouncePercent;
        this.hideLoading();
    }

    reload() {
        this.showLoading();
        this._dashboardService
            .getGeneralStats({})
            .subscribe(result => {
                this.init(result.transactionPercent, result.newVisitPercent, result.bouncePercent);
            });
    }
}

class ServerStatsLineChart extends DashboardChartBase {
    //Server stats => Sparklines: https://github.com/imsky/jquery.sparkline

    constructor(private _dashboardService: TenantDashboardServiceProxy
        , private _networkContainerElement: any
        , private _cpuLoadContainerElement: any
        , private _loadRateContainerElement: any) {
        super();
    }

    init(networkLoad, cpuLoad, loadRate) {
        $(this._networkContainerElement).sparkline(loadRate,
            {
                type: 'bar',
                width: '100',
                barWidth: 5,
                height: '55',
                barColor: '#35aa47',
                negBarColor: '#e02222'
            });

        $(this._cpuLoadContainerElement).sparkline(cpuLoad,
            {
                type: 'bar',
                width: '100',
                barWidth: 5,
                height: '55',
                barColor: '#ffb848',
                negBarColor: '#e02222'
            });

        $(this._loadRateContainerElement).sparkline(loadRate,
            {
                type: 'line',
                width: '100',
                height: '55',
                lineColor: '#ffb848'
            });

        this.hideLoading();
    }

    reload() {
        this.showLoading();
        this._dashboardService
            .getServerStats({})
            .subscribe(result => {
                this.init(result.networkLoad, result.cpuLoad, result.loadRate);
                this.hideLoading();
            });
    }
}

class ActivitiesTimeline extends DashboardChartBase {
    //Activities => Horizontal timeline: https://github.com/CodyHouse/horizontal-timeline

    constructor(private _detailsContainer: any,
        private _dateListContainer: any,
        private _mainContainer: any) {
        super();
    }

    init(timelineItems) {
        var $detailsContainer = $(this._detailsContainer);
        var $dateListContainer = $(this._dateListContainer);
        var $mainContainer = $(this._mainContainer);

        $detailsContainer.empty();
        $dateListContainer.empty();

        for (var i = 0; i < timelineItems.length; i++) {
            var timeline = timelineItems[i];

            var timeLineHeader = abp.utils.formatString(
                "<li><a href='#0' data-date='{0}' class='border-after-red bg-after-red {2}'>{1}</a></li>",
                timeline.shortDate,
                timeline.titleDate,
                i === 0 ? "selected" : "");

            $dateListContainer.append(timeLineHeader);

            const templateHtml = `
<li class='{0}' data-date='{1}'>
   <div class='mt-title'>
      <h2 class='mt-content-title'>{2}</h2>
   </div>
   <div class='mt-author' >
      <div class='mt-avatar' > 
         <img src='{3}'/>
      </div>
      <div class='mt-author-name' > 
         <a href= 'javascript:;' class='font-blue-madison'>{4}</a > 
      </div>
      <div class='mt-author-datetime font-grey-mint'>{5}</div>
   </div>
   <div class='clearfix'></div >
   <div class='mt-content border-grey-steel' style='max-height: 70px;overflow: auto;'>
      <p>{6}</p>
   </div >
</li>`;

            var timeLineDetail = abp.utils.formatString(templateHtml,
                (i === 0 ? "selected" : ""),
                timeline.shortDate,
                timeline.title,
                abp.appPath + 'assets/metronic/admin/layout4/media/users/' + timeline.image,
                timeline.autherName,
                timeline.longDate,
                timeline.text);

            $detailsContainer.append(timeLineDetail);
        }

        $mainContainer.addClass("cd-horizontal-timeline mt-timeline-horizontal");

        if (timelineItems) {
            $('.cd-horizontal-timeline').horizontalTimeline();
        }

        this.hideLoading();
    }
}

class DashboardHeaderStats extends DashboardChartBase {

    totalProfit: number = 0; totalProfitCounter: number = 0;
    newFeedbacks: number = 0; newFeedbacksCounter: number = 0;
    newOrders: number = 0; newOrdersCounter: number = 0;
    newUsers: number = 0; newUsersCounter: number = 0;

    totalProfitChange: number = 76; totalProfitChangeCounter: number = 0;
    newFeedbacksChange: number = 85; newFeedbacksChangeCounter: number = 0;
    newOrdersChange: number = 45; newOrdersChangeCounter: number = 0;
    newUsersChange: number = 57; newUsersChangeCounter: number = 0;

    init(totalProfit, newFeedbacks, newOrders, newUsers) {
        this.totalProfit = totalProfit;
        this.newFeedbacks = newFeedbacks;
        this.newOrders = newOrders;
        this.newUsers = newUsers;
        this.hideLoading();
    };
};

class MemberActivityTable extends DashboardChartBase {

    memberActivities: Array<any>;

    constructor(private _dashboardService: TenantDashboardServiceProxy) {
        super();
    }

    init() {
        this.reload();
    }

    reload() {
        this.showLoading();
        this._dashboardService
            .getMemberActivity()
            .subscribe(result => {
                this.memberActivities = result.memberActivities;
                this.hideLoading();
            });
    }
}
