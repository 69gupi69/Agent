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
define(["require", "exports", "../data/urls", "./grid"], function (require, exports, urls_1, grid_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.UsersGrid = void 0;
    var UsersGrid = /** @class */ (function (_super) {
        __extends(UsersGrid, _super);
        function UsersGrid(container, id, options) {
            var _this = _super.call(this, container, id, options) || this;
            _this._selectedIds = options.selectedIds;
            return _this;
        }
        Object.defineProperty(UsersGrid.prototype, "sort", {
            get: function () {
                return this._sort;
            },
            set: function (value) {
                this._sort = value;
            },
            enumerable: false,
            configurable: true
        });
        UsersGrid.prototype.dispose = function () {
            _super.prototype.dispose.call(this);
        };
        Object.defineProperty(UsersGrid.prototype, "dataSource", {
            get: function () {
                var _this = this;
                return new kendo.data.DataSource({
                    serverPaging: true,
                    pageSize: 15,
                    page: 1,
                    transport: {
                        read: {
                            url: urls_1.getAgentUsers,
                            crossDomain: true,
                            xhrFields: { withCredentials: true },
                            type: "GET",
                            cache: false
                        },
                        parameterMap: function (data) {
                            data["IsSelected"] = _this._selectedIds;
                            _this._filters = data["filter"] ? data["filter"] : null;
                            _this._sort = data["sort"] ? data["sort"] : null;
                            return data;
                        }
                    },
                    schema: {
                        model: {
                            id: "Id",
                            fields: {
                                IsSelected: { type: "bool", field: "IsSelected" },
                                Id: { type: "string", field: "Id" },
                                FirstName: { type: "string", field: "FirstName" },
                                SecondName: { type: "string", field: "SecondName" },
                                LastName: { type: "string", field: "LastName" },
                                Phone: { type: "string", field: "Phone" },
                                Email: { type: "string", field: "Email" },
                                ContractorName: { type: "string", field: "ContractorName" },
                                PositionName: { type: "string", field: "PositionName" }
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
        Object.defineProperty(UsersGrid.prototype, "gridOptions", {
            get: function () {
                return {
                    autoBind: false,
                    size: "80%",
                    dataSource: this.dataSource,
                    selectable: "multiple, row",
                    pageable: {
                        input: true,
                        refresh: true,
                        pageSizes: [15, 20, 25, 30, 50, 100, "All"],
                        buttonCount: 15
                    },
                    dataBound: this.options.event["dataBound"],
                    dataBinding: this.options.event["dataBinding"],
                    persistSelection: true,
                    resizable: true,
                    sortable: {
                        mode: "multiple"
                    },
                    columns: [
                        {
                            field: "IsSelected",
                            headerTemplate: "<input type='checkbox' class='headCheckbox' />",
                            template: "<input type='checkbox' class='checkbox' />",
                            width: "2%",
                            style: "text-align:center;"
                        },
                        {
                            field: "LastName",
                            title: "Фамилия",
                            filterable: false,
                            width: "10%"
                        },
                        {
                            field: "FirstName",
                            title: "Имя",
                            filterable: true,
                            width: "10%"
                        },
                        {
                            field: "SecondName",
                            title: "Отчество",
                            filterable: true,
                            width: "10%"
                        },
                        {
                            field: "Phone",
                            title: "Телефон",
                            filterable: false,
                            width: "7%"
                        },
                        {
                            field: "Email",
                            title: "Адрес почты",
                            filterable: false,
                            width: "18%"
                        },
                        {
                            field: "PositionName",
                            title: "Должность",
                            filterable: false,
                            width: "15%"
                        },
                        {
                            field: "ContractorName",
                            title: "Отдел",
                            filterable: false,
                            width: "45%"
                        }
                    ],
                    editable: "inline"
                };
            },
            enumerable: false,
            configurable: true
        });
        ;
        Object.defineProperty(UsersGrid.prototype, "path", {
            set: function (data) {
                this._selectedIds = data["selectedIds"];
            },
            enumerable: false,
            configurable: true
        });
        UsersGrid.prototype.init = function () {
            _super.prototype.init.call(this);
        };
        return UsersGrid;
    }(grid_1.BaseGrid));
    exports.UsersGrid = UsersGrid;
    exports.default = UsersGrid;
});
//# sourceMappingURL=usersGrid.js.map