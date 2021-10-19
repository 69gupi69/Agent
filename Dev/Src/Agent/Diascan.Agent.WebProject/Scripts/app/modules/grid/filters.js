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
define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.getMultiFilterTemplate = exports.DropdownGridFilter = exports.DropDownListOptions = exports.GridFilter = void 0;
    var GridFilter = /** @class */ (function () {
        function GridFilter() {
        }
        return GridFilter;
    }());
    exports.GridFilter = GridFilter;
    var DropDownListOptions = /** @class */ (function () {
        function DropDownListOptions() {
        }
        return DropDownListOptions;
    }());
    exports.DropDownListOptions = DropDownListOptions;
    var DropdownGridFilter = /** @class */ (function (_super) {
        __extends(DropdownGridFilter, _super);
        function DropdownGridFilter(dataSource, options) {
            var _this = _super.call(this) || this;
            _this._dataSource = dataSource;
            _this._options = options;
            return _this;
        }
        DropdownGridFilter.prototype.ui = function (element) {
            element.kendoDropDownList({
                dataSource: this._dataSource,
                dataValueField: this._options && this._options.dataValueField ? this._options.dataValueField : "id",
                dataTextField: this._options && this._options.dataTextField ? this._options.dataTextField : "name"
            });
        };
        return DropdownGridFilter;
    }(GridFilter));
    exports.DropdownGridFilter = DropdownGridFilter;
    function getMultiFilterTemplate(field, value, text) {
        return "<li class=\"filterItem\" class=\"k-item k-state-default\">\n    <label class=\"k-link\" ><input type=\"checkbox\" name=\"" + field + "\" value=\"#= data." + value + " #\" >\n    # if(typeof data." + text + " === \"undefined\") { #\n        <span style=\"font-weight: bold\">\u0412\u044B\u0431\u0440\u0430\u0442\u044C \u0432\u0441\u0435</span>\n    # }else{ #\n        <span>#= data." + text + " #</span>\n    # } #\n    </label>\n</li>";
    }
    exports.getMultiFilterTemplate = getMultiFilterTemplate;
});
//# sourceMappingURL=filters.js.map