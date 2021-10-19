define(["require", "exports", "../modules/layout/notificator"], function (require, exports, notificator_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ReportExport = void 0;
    var ReportExport = /** @class */ (function () {
        function ReportExport() {
        }
        ReportExport._getRound = function (column) {
            if (column.round) {
                return column.round;
            }
            else if (column.format && column.format.substring(0, 4) == "{0:n") {
                return column.format.substring(4, 5);
            }
            return undefined;
        };
        ReportExport.downloadDataset = function (kendoGrid, title) {
            var that = this;
            var columns = kendoGrid.grid.columns.filter(function (c) { return (c.hidden !== true && c.field); })
                .map(function (c) { return ({ width: c.width, field: c.field, title: c.title, round: that._getRound(c) }); });
            var dataSource = kendoGrid.dataSource;
            var gridOptions = {
                take: 0,
                skip: 0,
                page: 0,
                pageSize: 0,
                sort: kendoGrid.sort,
                filter: kendoGrid.filters
            };
            gridOptions = dataSource.transport.parameterMap(gridOptions);
            var data = {
                inputRequestParameters: jQuery.param(gridOptions),
                columnsInput: jQuery.param({ columns: columns, headerDoubleRow: false }),
                url: dataSource.transport.options.read.url,
                header: title
            };
        };
        ReportExport._showError = function (message, jqXhr) {
            if (jqXhr.responseText.indexOf('OutOfMemoryException') !== -1) {
                notificator_1.globalNotifier.error(" Не удалось создать документ. Попробуйте выгрузить меньше данных");
            }
            else if (jqXhr.responseText.indexOf('WebClientException') !== -1) {
                notificator_1.globalNotifier.error(" Не удалось получить данные");
            }
            else {
                notificator_1.globalNotifier.error(" Ошибка при формировании документа");
            }
        };
        ReportExport._downloadDataset = function () {
            window.location.href = "/DownloadDataset/DownloadDataset";
        };
        return ReportExport;
    }());
    exports.ReportExport = ReportExport;
});
//# sourceMappingURL=reportExport.js.map