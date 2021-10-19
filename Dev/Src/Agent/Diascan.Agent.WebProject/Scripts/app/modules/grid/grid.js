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
define(["require", "exports", "../baseControl", "../utils/container", "../utils/gridHelpers"], function (require, exports, baseControl_1, container_1, gridHelpers_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.BaseGrid = void 0;
    var BaseGrid = /** @class */ (function (_super) {
        __extends(BaseGrid, _super);
        function BaseGrid(container, id, options) {
            var _this = _super.call(this, container, id) || this;
            _this.columnCommand = function (command, data, target) {
                var row = $(target).closest("tr");
                _this.grid.select(row);
                _this._selectedRowIndex = row.index();
                _this.notify(command, data);
            };
            _this.resizeEvent = function () {
                if (_this.grid) {
                    _this.grid.refresh();
                }
            };
            _this._options = options;
            return _this;
        }
        Object.defineProperty(BaseGrid.prototype, "options", {
            get: function () {
                return this._options;
            },
            enumerable: false,
            configurable: true
        });
        BaseGrid.prototype.disableCommand = function (id) {
            var sel = "a.k-grid-" + id;
            this.container.find("div.k-grid-toolbar").find(sel).attr("disabled", "disabled");
        };
        BaseGrid.prototype.enableCommand = function (id) {
            var sel = "a.k-grid-" + id;
            this.container.find("div.k-grid-toolbar").find(sel).removeAttr("disabled");
        };
        BaseGrid.prototype.hideCommand = function (id) {
            var sel = "a.k-grid-" + id;
            this.container.find("div.k-grid-toolbar").find(sel).hide();
        };
        BaseGrid.prototype.showCommand = function (id) {
            var sel = "a.k-grid-" + id;
            this.container.find("div.k-grid-toolbar").find(sel).show();
        };
        Object.defineProperty(BaseGrid.prototype, "grid", {
            get: function () {
                return this.container.data("kendoGrid");
            },
            enumerable: false,
            configurable: true
        });
        BaseGrid.prototype.dispose = function () {
            container_1.DefaultResizer.unsubscribe("resize", this.resizeEvent);
            if (this._options && this._options.event) {
                if (this._options.event.remove) {
                    this.unsubscribe("deleteCommand", this._options.event.remove);
                }
                if (this._options.event.mailingSettings) {
                    this.unsubscribe("mailingSettingsCommand", this._options.event.mailingSettings);
                }
                if (this._options.event.download) {
                    this.unsubscribe("downloadCommand", this._options.event.download);
                }
            }
            if (this.grid !== undefined)
                this.grid.destroy();
        };
        BaseGrid.prototype.afterInit = function () {
        };
        BaseGrid.prototype.setToolbarHandler = function (id, handler) {
            var sel = "a.k-grid-" + id;
            this.container.find("div.k-grid-toolbar").find(sel).click(function (e) {
                e.preventDefault();
                if (!e.target.attributes["disabled"])
                    handler();
            });
        };
        BaseGrid.prototype.setToolbarImageClass = function (id, cls) {
            var sel = "a.k-grid-" + id;
            this.container.find("div.k-grid-toolbar").find(sel)
                .prepend("<span class=\"k-icon " + cls + "\"></span>");
        };
        BaseGrid.prototype.init = function () {
            var _this = this;
            this.container.kendoGrid(this.gridOptions);
            container_1.DefaultResizer.subscribe("resize", this.resizeEvent);
            this.grid.bind("dataBound", function () {
                if (_this._selectedRowIndex >= 0) {
                    _this.grid.select("tr:eq(" + _this._selectedRowIndex + ")");
                }
            });
            this.grid.bind("page", function () {
                _this._selectedRowIndex = -1;
            });
            this.grid.bind("columnResize", function () {
                _this.grid.refresh();
            });
            if (this._options && this._options.event) {
                if (this._options.event.remove) {
                    this.subscribe("deleteCommand", this._options.event.remove);
                }
                if (this._options.event.mailing) {
                    this.subscribe("mailingCommand", this._options.event.mailing);
                }
                if (this._options.event.mailingSettings) {
                    this.subscribe("mailingSettingsCommand", this._options.event.mailingSettings);
                }
                if (this._options.event.download) {
                    this.subscribe("downloadCommand", this._options.event.download);
                }
            }
            new gridHelpers_1.GridCommandHelper(this.grid, this.columnCommand);
            //let that = this;
            this.setToolbarHandler("mailingSettings", function () {
                //that.
                _this.notify("mailingSettingsCommand", _this);
            });
            this.afterInit();
        };
        BaseGrid.prototype.refresh = function (noData, firstPage) {
            if (!noData) {
                if (firstPage) {
                    this.grid.dataSource.page(0);
                }
                else {
                    this.grid.dataSource.read();
                }
            }
            else {
                this.grid.refresh();
            }
        };
        ;
        return BaseGrid;
    }(baseControl_1.default));
    exports.BaseGrid = BaseGrid;
    exports.default = BaseGrid;
});
//# sourceMappingURL=grid.js.map