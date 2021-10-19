define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.Stack = void 0;
    var Stack = /** @class */ (function () {
        function Stack(depth) {
            this.depth = depth;
            this.data = [];
        }
        Stack.prototype.push = function (item) {
            if (this.depth && this.data.length === this.depth) {
                this.data.shift();
            }
            this.data.push(item);
        };
        Stack.prototype.pop = function () {
            if (this.data.length > 0) {
                return this.data.pop();
            }
            return null;
        };
        Stack.prototype.get = function (index) {
            if (index < this.data.length) {
                return this.data[this.data.length - 1 - index];
            }
            return null;
        };
        Stack.prototype.find = function (selector) {
            for (var i = 0; i < this.data.length; i++) {
                var item = this.get(i);
                if (selector(item)) {
                    return item;
                }
            }
            return null;
        };
        return Stack;
    }());
    exports.Stack = Stack;
});
//# sourceMappingURL=datastructures.js.map