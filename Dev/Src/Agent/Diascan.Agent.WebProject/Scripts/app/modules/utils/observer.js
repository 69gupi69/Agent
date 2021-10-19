define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ObserverControl = exports.Observer = void 0;
    var Observer = /** @class */ (function () {
        function Observer() {
            this.observers = [];
        }
        Observer.prototype.subscribe = function (observer) {
            this.observers.push(observer);
        };
        Observer.prototype.unsubscribe = function (observer) {
            var index = this.observers.indexOf(observer);
            if (index > -1) {
                this.observers.splice(index, 1);
            }
        };
        ;
        Observer.prototype.notifyAll = function (e) {
            for (var _i = 0, _a = this.observers; _i < _a.length; _i++) {
                var o = _a[_i];
                o(e);
            }
        };
        return Observer;
    }());
    exports.Observer = Observer;
    var ObserverControl = /** @class */ (function () {
        function ObserverControl() {
            this.observers = {};
        }
        ObserverControl.prototype.subscribe = function (eventType, observer) {
            if (!this.observers[eventType]) {
                this.observers[eventType] = new Observer();
            }
            this.observers[eventType].subscribe(observer);
        };
        ObserverControl.prototype.unsubscribe = function (eventType, observer) {
            if (this.observers[eventType]) {
                this.observers[eventType].unsubscribe(observer);
            }
        };
        ObserverControl.prototype.subscribeAll = function (observer) {
            for (var k in this.observers) {
                this.observers[k].subscribe(observer);
            }
        };
        ObserverControl.prototype.unsubscribeAll = function (observer) {
            for (var k in this.observers) {
                this.observers[k].unsubscribe(observer);
            }
        };
        ObserverControl.prototype.notify = function (eventType, e) {
            if (this.observers[eventType]) {
                this.observers[eventType].notifyAll(e);
            }
        };
        return ObserverControl;
    }());
    exports.ObserverControl = ObserverControl;
    exports.default = ObserverControl;
});
//# sourceMappingURL=observer.js.map