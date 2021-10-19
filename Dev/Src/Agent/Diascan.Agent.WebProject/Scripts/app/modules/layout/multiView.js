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
define(["require", "exports", "../../types/datastructures", "../baseControl"], function (require, exports, datastructures_1, baseControl_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.MultiView = void 0;
    var panContent = "<div style=\"height: 100%;width: 100%; box-sizing: border-box\"></div>";
    var MultiView = /** @class */ (function (_super) {
        __extends(MultiView, _super);
        function MultiView(container) {
            var _this = _super.call(this, container) || this;
            _this._stack = new datastructures_1.Stack();
            return _this;
        }
        MultiView.prototype.find = function (constructor) {
            return this._stack.find(function (i) { return i instanceof constructor; });
        };
        MultiView.prototype.dispose = function () {
            var item = this._stack.pop();
            while (item) {
                item.dispose();
                item = this._stack.pop();
            }
        };
        MultiView.prototype.init = function () {
        };
        MultiView.prototype.push = function (create) {
            if (this._stack.get(0)) {
                this._stack.get(0).container.hide();
            }
            var newItem = $(panContent);
            newItem.appendTo(this.container);
            this._stack.push(create(newItem));
        };
        MultiView.prototype.pushTo = function (create, constructor, controlId) {
            var result = undefined;
            result = this.find(constructor);
            if (result && (!controlId || result.id === controlId)) {
                return this.popTo(result);
            }
            this.push(create);
            return this.current;
        };
        MultiView.prototype.pushAfter = function (create, constructor) {
            this.popTo(constructor, true);
            this.push(create);
        };
        MultiView.prototype.pop = function (silent) {
            if (silent === void 0) { silent = false; }
            var current = this._stack.pop();
            if (current) {
                current.container.hide();
                current.dispose();
                current.container.remove();
            }
            if (this.current) {
                this.current.container.show();
                if (!silent)
                    this.current.refresh();
            }
        };
        MultiView.prototype.popTo = function (itemOrConstructor, silent) {
            if (silent === void 0) { silent = false; }
            if (itemOrConstructor instanceof baseControl_1.default) {
                var item = itemOrConstructor;
                while (this.current && !(this.current == item)) {
                    this.pop(silent);
                }
                return this.current;
            }
            else {
                var constructor = itemOrConstructor;
                while (this.current && !(this.current instanceof constructor)) {
                    this.pop(silent);
                }
                return this.current;
            }
        };
        Object.defineProperty(MultiView.prototype, "current", {
            get: function () {
                return this._stack.get(0);
            },
            enumerable: false,
            configurable: true
        });
        MultiView.createControlId = function (controlId, objectId) {
            if (objectId) {
                return controlId + "_" + objectId;
            }
            else {
                return controlId;
            }
        };
        return MultiView;
    }(baseControl_1.default));
    exports.MultiView = MultiView;
    exports.default = MultiView;
});
//# sourceMappingURL=multiView.js.map