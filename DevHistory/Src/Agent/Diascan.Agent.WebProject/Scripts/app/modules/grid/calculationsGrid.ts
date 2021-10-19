import {
    getJsonsHeadAndCountFromDataBaseUrl
} from "../data/urls";
import DataSources = require("../data/dataSources");
import Filters = require("./filters");
import { CustomGridOptions, BaseGrid, IGridOptions, IGridEvents } from "./grid";
import { UserRoleTypes } from "../../enums/userRoleTypes";
import GridHelpers = require("../utils/gridHelpers");
import GridCommandHelper = GridHelpers.GridCommandHelper;


interface CalculationsGridSettings extends IGridOptions {
    contractorId?: string;
    pipelineId?: string;
    routeId?: string;
    calculationIds?: string[];
    event?: IGridEvents;
}

class BaseCalculationsGrid extends BaseGrid {
    protected gridOptions: CustomGridOptions;
    protected kendoGrid: any;

    protected get baseGridOptions(): CalculationsGridSettings {
        return <CalculationsGridSettings>this.options;
    }   

    protected getToolbar() {
        let toolbar = [];
        if (window["UserRoles"].some(role => role === UserRoleTypes.SuperUser)) {
            toolbar.push({ name: "mailingSettings", text: "Отправка отчетов пользователям ИС \"Агент\""});
        }
        let exportRow =
            '<p>aefawfew</p><div class="scriptRightForMailingSettingsPanelbar"><style> .k-template.k-grid-mailingSettings {float: right;} </style></div>';
        $(".scriptRightForMailingSettingsPanelbar").replaceWith(exportRow);
        return toolbar;
    }
}


class CalculationsGrid extends BaseCalculationsGrid {
    get sort(): kendo.data.DataSourceParameterMapDataSort[] {
        return this._sort;
    }

    set sort(value: kendo.data.DataSourceParameterMapDataSort[]) {
        this._sort = value;
    }

    public dispose() {
        super.dispose();
    }

    protected getToolbar() {
        return super.getToolbar();
    }

    private _contractorId: string;
    private _pipelineId: string;
    private _routeId: string;
    private _filters: kendo.data.DataSourceParameterMapDataFilter;
    private _sort: kendo.data.DataSourceParameterMapDataSort[];
    private _calculationIds: string[];
    private mailingEnabled: boolean;

    private get dataSource(): kendo.data.DataSource {
        return new kendo.data.DataSource({
            serverPaging: true,
            serverSorting: true,
            serverFiltering: true,
            pageSize: 10,
            page: 1,
            transport: {
                read: {
                    url: getJsonsHeadAndCountFromDataBaseUrl,
                    crossDomain: true,
                    xhrFields: { withCredentials: true },
                    type: "GET",
                    cache: false
                },
                parameterMap: (data) => {
                    data["contractorId"] = this._contractorId ? this._contractorId : null;
                    data["pipelineId"] = this._pipelineId ? this._pipelineId : null;
                    data["routeId"] = this._routeId ? this._routeId : null;
                    this._filters = data["filter"] ? data["filter"] : null;
                    this._sort = data["sort"] ? data["sort"] : null;
                    return data;
                }
            },
            schema: {
                model: {
                    id: "Id",
                    fields: {
                        Id: { type: "string", field: "Id" },
                        WorkItemName: { type: "string", field: "WorkItemName" },
                        ContractorId: { type: "string", field: "ContractorId" },
                        ContractorName: { type: "string", field: "ContractorName" },
                        PipeLineId: { type: "string", field: "PipeLineId" },
                        PipeLineName: { type: "string", field: "PipeLineName" },
                        RouteId: { type: "string", field: "RouteId" },
                        RouteName: { type: "string", field: "RouteName" },
                        AccountUserName: { type: "string", field: "AccountUserName" },
                        ComputerName: { type: "string", field: "ComputerName" },
                        ResponsibleWorkItem: { type: "string", field: "ResponsibleWorkItem" },
                        DateWorkItem: { type: "date", field: "DateWorkItem" }
                    }
                },
                data: "dataResult",
                total: "totalCount"
            }
        });
    }

    protected get gridOptions() {
        let that = this;
        return {
            autoBind: false,
            size: "80%",
            dataSource: this.dataSource,
            columnMenu: true,
            resizable: true,
            reorderable: true,
            filterable: true,
            selectable: true,
            sortable: {
                mode: "multiple"
            },
            pageable: {
                input: true,
                refresh: true,
                pageSizes: [10, 15, 20, 25, 30, 50, 100, "All"],
                buttonCount: 10
            },
            toolbar: this.getToolbar(),
            dataBound: (e) => {
                $(".checkbox").bind("click",
                    function(e) {
                        let val = $(e.target).prop('checked');
                        e.stopPropagation();
                        $(e.target).closest("tr").toggleClass("k-state-selected");
                        let row = $("div[data-role=grid]").data("kendoGrid")
                            .dataItem($(e.target).closest("tr"));
                        if (val) {
                            that._calculationIds.push(row["Id"]);
                        } else {
                            GridCommandHelper.removeA(that._calculationIds, row["Id"]);
                        }
                        if (that._calculationIds.length > 0) {
                            that.enableCommand("mailingSettings");
                        } else {
                            that.disableCommand("mailingSettings");
                        }
                    });
                for (let i = 0; i < e.sender._data.length; i++) {
                    if (that._calculationIds.indexOf(e.sender._data[i].Id) !== -1) {
                        let ch = e.sender.tbody.find("[data-uid=" + e.sender._data[i].uid + "]").closest("tr")
                            .toggleClass("k-state-selected").prevObject.prevObject["0"].children[i];
                        ch.childNodes["9"].children["0"].checked = true; // childNodes["9"] - номер колонки!!!!
                    }
                }
                if (that._calculationIds.length > 0) {
                    that.enableCommand("mailingSettings");
                } else {
                    that.disableCommand("mailingSettings");
                }
            },
            columns: (window["UserRoles"].some(role => role === UserRoleTypes.SuperUser))
                ? [
                    {
                        command:
                        [
                            {
                                name: "deleteCommand",
                                text: "",
                                className: "k-icon k-i-close-outline gridButton",
                                title: "Удалить запись",
                            },
                            {
                                name: "downloadCommand",
                                text: "",
                                className: "k-icon k-i-download gridButton",
                                title: "Экспорт в Excel"
                                }
                            //    ,
                            //{
                            //    name: "mailingCommand",
                            //    text: "",
                            //    className: "k-icon k-i-email gridButton",
                            //    title: "Отправить отчет в виде Excel на почту"
                            //}
                        ],
                        width: "8%"
                    },
                    {
                        title: "Выбрать",
                        template: "<input type='checkbox' class='checkbox' />",
                        width: "6%",
                        style: "text-align:center;"
                    },
                    {
                        field: "Name",
                        title: "Название (код) прогона",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "10%"
                    },
                    {
                        field: "ResponsibleWorkItem",
                        title: "Ответственный за пропуск",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "12%"
                    },
                    {
                        field: "ContractorName",
                        title: "Заказчик",
                        filterable: false,
                        sortable: false,
                        width: "18%"
                    },
                    {
                        field: "PipeLineName",
                        title: "Трубопровод",
                        filterable: false,
                        sortable: false,
                        width: "18%"
                    },
                    {
                        field: "RouteName",
                        title: "Участок",
                        filterable: false,
                        sortable: false,
                        width: "18%"
                    },
                    {
                        field: "AccountUserName",
                        title: "Учетная запись",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "14%"
                    },
                    {
                        field: "ComputerName",
                        title: "Имя компьютера",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "12%"
                    },
                    {
                        field: "DateWorkItem",
                        title: "Дата пропуска",
                        format: "{0:dd.MM.yyyy}",
                        filterable: {
                            extra: false,
                            operators: {
                                date: {
                                    eq: "равна",
                                    gte: "после или равна",
                                    gt: "после",
                                    lte: "до или равна",
                                    lt: "до"
                                }
                            }
                        },
                        width: "8%"
                    }
                ]
                : [
                    {
                        command:
                        [
                            {
                                name: "downloadCommand",
                                text: "",
                                className: "k-icon k-i-download gridButton",
                                title: "Экспорт в Excel"
                            }
                        ],
                        width: "5%"
                    },
                    {
                        field: "Name",
                        title: "Название (код) прогона",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "10%"
                    },
                    {
                        field: "ResponsibleWorkItem",
                        title: "Ответственный за пропуск",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "12%"
                    },
                    {
                        field: "ContractorName",
                        title: "Заказчик",
                        sortable: false,
                        filterable: false,
                        width: "18%"
                    },
                    {
                        field: "PipeLineName",
                        title: "Трубопровод",
                        sortable: false,
                        filterable: false,
                        width: "18%"
                    },
                    {
                        field: "RouteName",
                        title: "Участок",
                        sortable: false,
                        filterable: false,
                        width: "18%"
                    },
                    {
                        field: "AccountUserName",
                        title: "Учетная запись",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "18%"
                    },
                    {
                        field: "ComputerName",
                        title: "Имя компьютера",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "12%"
                    },
                    {
                        field: "DateWorkItem",
                        title: "Дата пропуска",
                        format: "{0:dd.MM.yyyy}",
                        filterable: {
                            extra: false,
                            operators: {
                                date: {
                                    eq: "равна",
                                    gte: "после или равна",
                                    gt: "после",
                                    lte: "до или равна",
                                    lt: "до"
                                }
                            }
                        },
                        width: "8%"
                    }
                ]
        };
    };

    set path(data) {
        this._contractorId = data["contractorId"];
        this._pipelineId = data["pipelineId"];
        this._routeId = data["routeId"];
        this._calculationIds = data["calculationIds"];
    }

    constructor(container: JQuery, id: string, options?: CalculationsGridSettings) {
        super(container, id, options);
        this._contractorId = options.contractorId;
        this._pipelineId = options.pipelineId;
        this._routeId = options.routeId;
        this._calculationIds = [];
    }

    init() {
        super.init();
        this.mailingEnabled = false;
        if (this._calculationIds.length > 0) {
            this.enableCommand("mailingSettings");
        } else {
            this.disableCommand("mailingSettings");
        }
    }
}

export { CalculationsGrid, CalculationsGridSettings }
export default CalculationsGrid;