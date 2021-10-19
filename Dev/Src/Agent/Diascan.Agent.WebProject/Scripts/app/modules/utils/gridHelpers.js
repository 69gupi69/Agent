define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.GridCommandHelper = void 0;
    var GridCommandHelper = /** @class */ (function () {
        function GridCommandHelper(grid, commandHandler) {
            var _this = this;
            this._grid = grid;
            grid.bind("dataBound", function () {
                _this._setTitles();
            });
            var opts = grid.getOptions();
            if (opts.columns) {
                for (var _i = 0, _a = opts.columns; _i < _a.length; _i++) {
                    var col = _a[_i];
                    if (col.command) {
                        if (Array.isArray(col.command)) {
                            for (var _b = 0, _c = col.command; _b < _c.length; _b++) {
                                var cmd = _c[_b];
                                this._setHandler(grid, cmd, commandHandler);
                            }
                        }
                        else {
                            this._setHandler(grid, col.command, commandHandler);
                        }
                    }
                }
            }
            grid.setOptions(opts);
        }
        GridCommandHelper.prototype._setHandler = function (grid, command, commandHandler) {
            command.click = function (e) {
                e.preventDefault();
                var cmd = command.name;
                var data = grid.dataItem($(e.currentTarget).closest("tr"));
                if (commandHandler) {
                    commandHandler(cmd, data, e.currentTarget);
                }
            };
        };
        GridCommandHelper.prototype._setTitles = function () {
            for (var i in this._grid.columns) {
                var col = this._grid.columns[i];
                if (col.command) {
                    if (Array.isArray(col.command)) {
                        for (var j in col.command) {
                            this._setTitle(col.command[j], i);
                        }
                    }
                    else {
                        this._setTitle(col.command, i);
                    }
                }
            }
        };
        GridCommandHelper.prototype._setTitle = function (c, i) {
            if (c.title) {
                var clsFilter = c.className.replace(/\s+/g, ".");
                this._grid.wrapper
                    .find("tr")
                    .find("td:eq(" + i + ")")
                    .find("a." + clsFilter)
                    .attr("title", c.title);
            }
        };
        GridCommandHelper.compareArrays = function (array1, array2) {
            if (array1.length !== array2.length)
                return false;
            for (var i = 0; i < array1.length; i++) {
                if (array1[i] !== array2[i])
                    return false;
            }
            return true;
        };
        ;
        GridCommandHelper.getContainer = function (id) {
            return $('#' + id);
        };
        ;
        GridCommandHelper.getFullElementId = function (containerId, id) {
            return containerId + id;
        };
        ;
        GridCommandHelper.createTemplateScript = function (id, body) {
            return '<script id="' + id + '" type="text/x-kendo-template">' + body + '</script>';
        };
        GridCommandHelper.pgDateToString = function (date, kendoFormat) {
            return kendo.toString((kendo.parseDate(date, "yyyy-MM-ddTHH:mm:ss")), kendoFormat);
        };
        GridCommandHelper.getGridSelectedElements = function (grid) {
            var selectedElements = [];
            grid.select()
                .each(function (e) {
                var dataItem = grid.dataItem(this);
                selectedElements.push(dataItem);
            });
            return selectedElements;
        };
        GridCommandHelper.getGridSelectedElementIds = function (grid) {
            if (grid != undefined) {
                var selectedElements_1 = [];
                grid.select()
                    .each(function (e) {
                    var dataItem = grid.dataItem(this);
                    selectedElements_1.push(dataItem);
                });
                return selectedElements_1.map(function (i) {
                    return i.Id;
                });
            }
            return undefined;
        };
        GridCommandHelper.removeA = function (arr, item) {
            var index = arr.indexOf(item);
            if (index > -1) {
                arr.splice(index, 1);
            }
            return arr;
        };
        return GridCommandHelper;
    }());
    exports.GridCommandHelper = GridCommandHelper;
});
//# sourceMappingURL=gridHelpers.js.map