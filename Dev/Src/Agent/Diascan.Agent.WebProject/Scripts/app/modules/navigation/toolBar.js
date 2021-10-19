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
define(["require", "exports", "../baseControl"], function (require, exports, baseControl_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ToolBar = void 0;
    var ToolBar = /** @class */ (function (_super) {
        __extends(ToolBar, _super);
        function ToolBar(container, config) {
            var _this = _super.call(this, container) || this;
            _this._clickHandler = function (e) {
                _this.notify("click", { id: e.id });
                _this.notify("click#" + e.id);
            };
            _this._toggleHandler = function (e) {
                _this.notify("toggle", { id: e.id });
                _this.notify("toggle#" + e.id);
            };
            _this._config = config;
            return _this;
        }
        ToolBar._mapToKendoItems = function (items) {
            var result = [];
            for (var _i = 0, items_1 = items; _i < items_1.length; _i++) {
                var item = items_1[_i];
                if (item.type === "Button") {
                    result.push({
                        id: item.id,
                        type: "button",
                        text: item.text,
                        togglable: false
                    });
                    continue;
                }
                if (item.type === "ToggleButton") {
                    result.push({
                        id: item.id,
                        type: "button",
                        text: item.text,
                        togglable: true,
                        selected: item.checked,
                        hidden: !item.visible
                    });
                }
            }
            return result;
        };
        ToolBar.prototype.dispose = function () {
        };
        Object.defineProperty(ToolBar.prototype, "toolBar", {
            get: function () {
                return this.container.data("kendoToolBar");
            },
            enumerable: false,
            configurable: true
        });
        ToolBar.prototype.init = function () {
            this.container.kendoToolBar({
                items: ToolBar._mapToKendoItems(this._config && this._config.items ? this._config.items : []),
                click: this._clickHandler,
                toggle: this._toggleHandler
            });
        };
        ToolBar.prototype.getCheked = function (id) {
            return this.container.find("#" + id).hasClass("k-state-active");
        };
        return ToolBar;
    }(baseControl_1.default));
    exports.ToolBar = ToolBar;
    exports.default = ToolBar;
});
//# sourceMappingURL=toolBar.js.map