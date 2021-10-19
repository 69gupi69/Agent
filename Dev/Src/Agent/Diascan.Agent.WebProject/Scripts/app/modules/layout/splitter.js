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
define(["require", "exports", "../utils/container", "../baseControl"], function (require, exports, container_1, baseControl_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.HorizontalSplitter = void 0;
    var paneContent = "<div style=\"height: 100%; width: 100%; \"></div>";
    var content = "<div style=\"width: 100%;height: 100%;box-sizing: border-box\">\n<div>" + paneContent + "</div><div>" + paneContent + "</div>\n</div>";
    var HorizontalSplitter = /** @class */ (function (_super) {
        __extends(HorizontalSplitter, _super);
        function HorizontalSplitter(container, config) {
            var _this = _super.call(this, container) || this;
            _this.resizeEvent = function () {
                _this.panes[0].height(_this.container.height());
                _this.panes[1].height(_this.container.height());
                _this.separator.height(_this.container.height());
            };
            _this._config = config;
            _this.replaceContainer(content);
            return _this;
        }
        HorizontalSplitter.prototype.dispose = function () {
            if (this._left)
                this._left.dispose();
            if (this._right)
                this._right.dispose();
            container_1.DefaultResizer.unsubscribe("resize", this.resizeEvent);
        };
        Object.defineProperty(HorizontalSplitter.prototype, "leftPaneContentContainer", {
            get: function () {
                return this.panes[0].children("div:first");
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(HorizontalSplitter.prototype, "rightPaneContentContainer", {
            get: function () {
                return this.panes[1].children("div:first");
            },
            enumerable: false,
            configurable: true
        });
        HorizontalSplitter.prototype._clearLeft = function () {
            this.panes[0].children("div:first").replaceWith(paneContent);
        };
        HorizontalSplitter.prototype._clearRight = function () {
            this.panes[1].children("div:first").replaceWith(paneContent);
        };
        Object.defineProperty(HorizontalSplitter.prototype, "panes", {
            get: function () {
                return [this.container.find("div.k-pane:first"), this.container.find("div.k-pane:last")];
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(HorizontalSplitter.prototype, "separator", {
            get: function () {
                return this.container.children("div[role=separator]");
            },
            enumerable: false,
            configurable: true
        });
        HorizontalSplitter.prototype.init = function () {
            var _this = this;
            var config = this._config;
            this.container.kendoSplitter({
                orientation: "horizontal",
                panes: [{
                        size: config.leftPane.size,
                        collapsible: true,
                        resizable: config.leftPane.resizable
                    }, {
                        size: config.rightPane.size,
                        collapsible: config.rightPane.collapsible,
                        resizable: config.rightPane.resizable,
                        scrollable: false
                    }],
                resize: function () {
                    _this.panes[1].width(_this.container.width() - _this.panes[1].offset().left + _this.separator.width() + 6);
                }
            });
            container_1.DefaultResizer.subscribe("resize", this.resizeEvent);
            this._left = this._config.leftPane.create(this.leftPaneContentContainer);
            this._right = this._config.rightPane.create(this.rightPaneContentContainer);
            this.resizeEvent();
        };
        HorizontalSplitter.prototype.setRight = function (f) {
            if (this._right) {
                this._right.dispose();
                this._clearRight();
            }
            this._right = f(this.rightPaneContentContainer);
        };
        Object.defineProperty(HorizontalSplitter.prototype, "right", {
            get: function () {
                return this._right;
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(HorizontalSplitter.prototype, "left", {
            get: function () {
                return this._left;
            },
            enumerable: false,
            configurable: true
        });
        HorizontalSplitter.prototype.setLeft = function (f) {
            if (this._left) {
                this._left.dispose();
                this._clearLeft();
            }
            this._left = f(this.leftPaneContentContainer);
        };
        HorizontalSplitter.prototype.refresh = function () {
            _super.prototype.refresh.call(this);
            if (this._config && this._config.leftPane && this._config.leftPane.refresh) {
                if (this._left != null)
                    this._left.refresh();
            }
            if (this._config.rightPane != null) {
                if (this._config && this._config.rightPane && this._config.rightPane.refresh) {
                    if (this._right != null)
                        this._right.refresh();
                }
            }
        };
        return HorizontalSplitter;
    }(baseControl_1.default));
    exports.HorizontalSplitter = HorizontalSplitter;
    exports.default = HorizontalSplitter;
});
//# sourceMappingURL=splitter.js.map