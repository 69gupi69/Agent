define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.globalConfirm = exports.ConfirmWindow = void 0;
    var content = "<div style=\"display: none;\" \n     data-role=\"window\" \n     data-modal=\"true\" \n     data-actions=\"\"\n     data-min-width=\"500\" data-bind=\"events:{open:open, deactivate:close}\"  >\n     <div class=\"modal-body\" data-bind=\"text:message\" style=\"font-size: large\"></div>\n     <div class=\"modal-footer\">\n        <button class=\"k-button k-button-icontext\" data-bind=\"events:{click:yesClick}\"><span class=\"k-icon k-i-check-outline\"></span>\u0414\u0430</button>\n        <button class=\"k-button k-button-icontext\" data-bind=\"events:{click:noClick}\"><span class=\"k-icon k-i-cancel\"></span>\u041D\u0435\u0442</button>\n    </div>\n</div>";
    var ConfirmWindow = /** @class */ (function () {
        function ConfirmWindow() {
            var _this = this;
            this._viewModel = kendo.observable({
                open: function () {
                    _this.window.center();
                },
                message: "",
                noClick: function () {
                    _this.window.close();
                    if (_this._cancelHandler) {
                        _this._cancelHandler();
                    }
                },
                yesClick: function () {
                    _this.window.close();
                    if (_this._okHandler) {
                        _this._okHandler();
                    }
                },
                close: function () {
                    _this.window.title("");
                    _this._viewModel.set("message", "");
                }
            });
            this._init();
        }
        Object.defineProperty(ConfirmWindow.prototype, "window", {
            get: function () {
                return this._container.data("kendoWindow");
            },
            enumerable: false,
            configurable: true
        });
        ConfirmWindow.prototype._init = function () {
            this._container = $(content).appendTo("body");
            kendo.bind(this._container, this._viewModel);
        };
        ConfirmWindow.prototype.show = function (title, message, okHandler, cancelHandler) {
            this._okHandler = okHandler;
            this._cancelHandler = cancelHandler;
            this.window.title(title);
            this._viewModel.set("message", message);
            this.window.open();
        };
        return ConfirmWindow;
    }());
    exports.ConfirmWindow = ConfirmWindow;
    var globalConfirm = new ConfirmWindow();
    exports.globalConfirm = globalConfirm;
    exports.default = ConfirmWindow;
});
//# sourceMappingURL=confirmWindow.js.map