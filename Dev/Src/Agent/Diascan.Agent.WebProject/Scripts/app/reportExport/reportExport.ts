import { globalNotifier } from "../modules/layout/notificator";

export class ReportExport {

    private static _getRound(column: any) {
        if (column.round) {
            return column.round;
        } else if (column.format && column.format.substring(0, 4) == "{0:n") {
            return column.format.substring(4, 5);
        }
        return undefined;
    }



    static downloadDataset(kendoGrid: any, title: string) {
        let that = this;

        let columns = kendoGrid.grid.columns.filter(c => (c.hidden !== true && c.field))
            .map(c => ({ width: c.width, field: c.field, title: c.title, round: that._getRound(c) }));


        let dataSource = kendoGrid.dataSource;

        let gridOptions = {
            take: 0,
            skip: 0,
            page: 0,
            pageSize: 0,
            sort: kendoGrid.sort,
            filter: kendoGrid.filters
        };
        gridOptions = dataSource.transport.parameterMap(gridOptions);

        let data = {
            inputRequestParameters: jQuery.param(gridOptions),
            columnsInput: jQuery.param({ columns: columns, headerDoubleRow: false }),
            url: dataSource.transport.options.read.url,
            header: title
        };
    }


    private static _showError(message, jqXhr) {

        if (jqXhr.responseText.indexOf('OutOfMemoryException') !== -1) {
            globalNotifier.error(" Не удалось создать документ. Попробуйте выгрузить меньше данных");
        }
        else if (jqXhr.responseText.indexOf('WebClientException') !== -1) {
            globalNotifier.error(" Не удалось получить данные");
        } else {
            globalNotifier.error(" Ошибка при формировании документа");
        }
    }

    private static _downloadDataset() {
        window.location.href = "/DownloadDataset/DownloadDataset";
    }
}
