define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var content = "\n<div class=\"waitModal\" style=\"display: none;\">\n    <span>Message</span>\n    <div class=\"sk-fading-circle\">\n      <div class=\"sk-circle1 sk-circle\"></div>\n      <div class=\"sk-circle2 sk-circle\"></div>\n      <div class=\"sk-circle3 sk-circle\"></div>\n      <div class=\"sk-circle4 sk-circle\"></div>\n      <div class=\"sk-circle5 sk-circle\"></div>\n      <div class=\"sk-circle6 sk-circle\"></div>\n      <div class=\"sk-circle7 sk-circle\"></div>\n      <div class=\"sk-circle8 sk-circle\"></div>\n      <div class=\"sk-circle9 sk-circle\"></div>\n      <div class=\"sk-circle10 sk-circle\"></div>\n      <div class=\"sk-circle11 sk-circle\"></div>\n      <div class=\"sk-circle12 sk-circle\"></div>\n    </div>\n</div>";
    var WaitModal = /** @class */ (function () {
        function WaitModal() {
            this._init();
        }
        WaitModal.prototype._init = function () {
            this._container = $(content).appendTo("body");
        };
        WaitModal.prototype._show = function (message) {
            if (message) {
                this._container.find("span").text(message).show();
            }
            else {
                this._container.find("span").text('').hide();
            }
            this._container.show();
        };
        WaitModal.prototype.show = function (message, duration) {
            var _this = this;
            if (duration) {
                this._timer = setTimeout(function () { return _this._show(message); }, duration);
            }
            else {
                this._show(message);
            }
        };
        WaitModal.prototype.hide = function () {
            if (this._timer) {
                clearTimeout(this._timer);
                this._timer = undefined;
            }
            this._container.hide();
        };
        Object.defineProperty(WaitModal, "wait", {
            get: function () {
                if (!WaitModal._wait) {
                    WaitModal._wait = new WaitModal();
                }
                return WaitModal._wait;
            },
            enumerable: false,
            configurable: true
        });
        WaitModal.show = function (message, duration) {
            WaitModal.wait.show(message, duration);
        };
        WaitModal.hide = function () {
            WaitModal.wait.hide();
        };
        WaitModal._wait = undefined;
        return WaitModal;
    }());
    exports.default = WaitModal;
});
//# sourceMappingURL=waitModal.js.map