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
define(["require", "exports", "./observer"], function (require, exports, observer_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.DefaultResizer = exports.Resizer = exports.replaceElement = exports.replaceContent = void 0;
    function replaceContent(container, content) {
        return container.html(content);
    }
    exports.replaceContent = replaceContent;
    function replaceElement(container, content) {
        var contEl = $(content);
        container.replaceWith(contEl);
        return contEl;
    }
    exports.replaceElement = replaceElement;
    var Resizer = /** @class */ (function (_super) {
        __extends(Resizer, _super);
        function Resizer(action) {
            var _this = _super.call(this) || this;
            _this.action = action;
            _this.init();
            return _this;
        }
        Resizer.prototype.resize = function () {
            var windowHeight = $(window).height();
            if (this.action) {
                this.action(windowHeight);
            }
            this.notify("resize", { windowHeight: windowHeight });
        };
        Resizer.prototype.init = function () {
            var _this = this;
            $(window).resize(function () {
                _this.resize();
            });
            this.resize();
        };
        return Resizer;
    }(observer_1.default));
    exports.Resizer = Resizer;
    exports.DefaultResizer = new Resizer();
});
//# sourceMappingURL=container.js.map