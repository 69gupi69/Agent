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
define(["require", "exports", "../utils/container", "../navigation/toolBar", "../baseControl"], function (require, exports, container_1, toolBar_1, baseControl_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ToolBarPanel = void 0;
    var ToolBarPanel = /** @class */ (function (_super) {
        __extends(ToolBarPanel, _super);
        function ToolBarPanel(container, config) {
            var _this = _super.call(this, container) || this;
            _this._toolBarEvent = function (e) {
                _this.notify(e.type, e.data);
            };
            _this._resizeEvent = function () {
                _this._resize();
            };
            _this._toolBarConfig = config && config.toolBarConfig;
            _this._createContent = config && config.createContent;
            _this.container.empty();
            return _this;
        }
        Object.defineProperty(ToolBarPanel.prototype, "contentControl", {
            get: function () {
                return this._contentControl;
            },
            enumerable: false,
            configurable: true
        });
        ToolBarPanel.prototype.dispose = function () {
            container_1.DefaultResizer.unsubscribe("resize", this._resizeEvent);
            this._contentControl.dispose();
            this._toolBar.unsubscribeGlobal(this._toolBarEvent);
        };
        ToolBarPanel.prototype.init = function () {
            this._toolBarElement = $("<div style=\"border: none; box-sizing:border-box;\">");
            this._toolBarElement.height(this._toolBarHeight);
            this._toolBarElement.appendTo(this.container);
            this._toolBar = new toolBar_1.default(this._toolBarElement, this._toolBarConfig);
            this._contentElement = $("<div>");
            this._resize();
            this._contentElement.appendTo(this.container);
            container_1.DefaultResizer.subscribe("resize", this._resizeEvent);
            this._toolBar.subscribeGlobal(this._toolBarEvent);
            this._toolBar.init();
            if (this._createContent) {
                this._contentControl = this._createContent(this._contentElement);
            }
        };
        ToolBarPanel.prototype._resize = function () {
            var contentHeight = this.container.height() - this._toolBarHeight;
            this._contentElement.height(contentHeight);
        };
        ToolBarPanel.prototype.getCheked = function (id) {
            return this._toolBar.getCheked(id);
        };
        return ToolBarPanel;
    }(baseControl_1.default));
    exports.ToolBarPanel = ToolBarPanel;
    exports.default = ToolBarPanel;
});
//# sourceMappingURL=toolBarPanel.js.map