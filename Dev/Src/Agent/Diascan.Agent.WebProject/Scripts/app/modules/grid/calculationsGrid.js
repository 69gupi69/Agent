var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "../data/urls", "./grid", "../utils/gridHelpers"], function (require, exports, urls_1, grid_1, GridHelpers) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.CalculationsGrid = void 0;
    var GridCommandHelper = GridHelpers.GridCommandHelper;
    var BaseCalculationsGrid = /** @class */ (function (_super) {
        __extends(BaseCalculationsGrid, _super);
        function BaseCalculationsGrid() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        Object.defineProperty(BaseCalculationsGrid.prototype, "baseGridOptions", {
            get: function () {
                return this.options;
            },
            enumerable: false,
            configurable: true
        });
        BaseCalculationsGrid.prototype.getToolbar = function () {
            var toolbar = [];
            if (window["UserRoles"].some(function (role) { return role === "SuperUser" /* SuperUser */; })) {
                toolbar.push({ name: "mailingSettings", text: "Отправка отчетов пользователям ИС \"Агент\"" });
            }
            var exportRow = '<p>aefawfew</p><div class="scriptRightForMailingSettingsPanelbar"><style> .k-template.k-grid-mailingSettings {float: right;} </style></div>';
            $(".scriptRightForMailingSettingsPanelbar").replaceWith(exportRow);
            return toolbar;
        };
        return BaseCalculationsGrid;
    }(grid_1.BaseGrid));
    var CalculationsGrid = /** @class */ (function (_super) {
        __extends(CalculationsGrid, _super);
        function CalculationsGrid(container, id, options) {
            var _this = _super.call(this, container, id, options) || this;
            _this._contractorId = options.contractorId;
            _this._pipelineId = options.pipelineId;
            _this._routeId = options.routeId;
            _this._calculationIds = [];
            return _this;
        }
        Object.defineProperty(CalculationsGrid.prototype, "sort", {
            get: function () {
                return this._sort;
            },
            set: function (value) {
                this._sort = value;
            },
            enumerable: false,
            configurable: true
        });
        CalculationsGrid.prototype.dispose = function () {
            _super.prototype.dispose.call(this);
        };
        CalculationsGrid.prototype.getToolbar = function () {
            return _super.prototype.getToolbar.call(this);
        };
        Object.defineProperty(CalculationsGrid.prototype, "dataSource", {
            get: function () {
                var _this = this;
                return new kendo.data.DataSource({
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    pageSize: 10,
                    page: 1,
                    transport: {
                        read: {
                            url: urls_1.getJsonsHeadAndCountFromDataBaseUrl,
                            crossDomain: true,
                            xhrFields: { withCredentials: true },
                            type: "GET",
                            cache: false
                        },
                        parameterMap: function (data) {
                            data["contractorId"] = _this._contractorId ? _this._contractorId : null;
                            data["pipelineId"] = _this._pipelineId ? _this._pipelineId : null;
                            data["routeId"] = _this._routeId ? _this._routeId : null;
                            _this._filters = data["filter"] ? data["filter"] : null;
                            _this._sort = data["sort"] ? data["sort"] : null;
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
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(CalculationsGrid.prototype, "gridOptions", {
            get: function () {
                var that = this;
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
                    dataBound: function (e) {
                        $(".checkbox").bind("click", function (e) {
                            var val = $(e.target).prop('checked');
                            e.stopPropagation();
                            $(e.target).closest("tr").toggleClass("k-state-selected");
                            var row = $("div[data-role=grid]").data("kendoGrid")
                                .dataItem($(e.target).closest("tr"));
                            if (val) {
                                that._calculationIds.push(row["Id"]);
                            }
                            else {
                                GridCommandHelper.removeA(that._calculationIds, row["Id"]);
                            }
                            if (that._calculationIds.length > 0) {
                                that.enableCommand("mailingSettings");
                            }
                            else {
                                that.disableCommand("mailingSettings");
                            }
                        });
                        for (var i = 0; i < e.sender._data.length; i++) {
                            if (that._calculationIds.indexOf(e.sender._data[i].Id) !== -1) {
                                var ch = e.sender.tbody.find("[data-uid=" + e.sender._data[i].uid + "]").closest("tr")
                                    .toggleClass("k-state-selected").prevObject.prevObject["0"].children[i];
                                ch.childNodes["9"].children["0"].checked = true; // childNodes["9"] - номер колонки!!!!
                            }
                        }
                        if (that._calculationIds.length > 0) {
                            that.enableCommand("mailingSettings");
                        }
                        else {
                            that.disableCommand("mailingSettings");
                        }
                    },
                    columns: (window["UserRoles"].some(function (role) { return role === "SuperUser" /* SuperUser */; }))
                        ? [
                            {
                                command: [
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
                                command: [
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
            },
            enumerable: false,
            configurable: true
        });
        ;
        Object.defineProperty(CalculationsGrid.prototype, "path", {
            set: function (data) {
                this._contractorId = data["contractorId"];
                this._pipelineId = data["pipelineId"];
                this._routeId = data["routeId"];
                this._calculationIds = data["calculationIds"];
            },
            enumerable: false,
            configurable: true
        });
        CalculationsGrid.prototype.init = function () {
            _super.prototype.init.call(this);
            this.mailingEnabled = false;
            if (this._calculationIds.length > 0) {
                this.enableCommand("mailingSettings");
            }
            else {
                this.disableCommand("mailingSettings");
            }
        };
        return CalculationsGrid;
    }(BaseCalculationsGrid));
    exports.CalculationsGrid = CalculationsGrid;
    exports.default = CalculationsGrid;
});
//# sourceMappingURL=calculationsGrid.js.map