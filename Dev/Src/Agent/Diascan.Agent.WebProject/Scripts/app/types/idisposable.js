define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.DisposeManager = void 0;
    var DisposeManager = /** @class */ (function () {
        function DisposeManager() {
            this.items = [];
        }
        DisposeManager.prototype.push = function (item) {
            this.items.push(item);
            return this;
        };
        DisposeManager.prototype.dispose = function () {
            if (this.items) {
                while (this.items.length > 0) {
                    var item = this.items.pop();
                    item.dispose();
                }
            }
        };
        return DisposeManager;
    }());
    exports.DisposeManager = DisposeManager;
});
//# sourceMappingURL=idisposable.js.map