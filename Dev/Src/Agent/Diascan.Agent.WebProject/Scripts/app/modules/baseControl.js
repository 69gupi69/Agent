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
define(["require", "exports", "./utils/container"], function (require, exports, container_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var BaseObserver = /** @class */ (function () {
        function BaseObserver() {
            this.observers = [];
        }
        BaseObserver.prototype.subscribe = function (observer) {
            this.observers.push(observer);
        };
        BaseObserver.prototype.unsubscribe = function (observer) {
            var index = this.observers.indexOf(observer);
            if (index > -1) {
                this.observers.splice(index, 1);
            }
        };
        return BaseObserver;
    }());
    var Observer = /** @class */ (function (_super) {
        __extends(Observer, _super);
        function Observer(type) {
            var _this = _super.call(this) || this;
            _this._type = type;
            return _this;
        }
        Object.defineProperty(Observer.prototype, "type", {
            get: function () {
                return this._type;
            },
            enumerable: false,
            configurable: true
        });
        Observer.prototype.notifyAll = function (data) {
            for (var _i = 0, _a = this.observers; _i < _a.length; _i++) {
                var o = _a[_i];
                o({ type: this._type, data: data });
            }
        };
        return Observer;
    }(BaseObserver));
    var GlobalObserver = /** @class */ (function (_super) {
        __extends(GlobalObserver, _super);
        function GlobalObserver() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        GlobalObserver.prototype.notifyAll = function (type, data) {
            for (var _i = 0, _a = this.observers; _i < _a.length; _i++) {
                var o = _a[_i];
                o({ type: type, data: data });
            }
        };
        return GlobalObserver;
    }(BaseObserver));
    var BaseControl = /** @class */ (function () {
        function BaseControl(container, id, dataId) {
            if (dataId === void 0) { dataId = ""; }
            this._observers = {};
            this._globalObserver = new GlobalObserver();
            this._container = container;
            this._id = id;
            this._dataId = dataId;
        }
        Object.defineProperty(BaseControl.prototype, "id", {
            get: function () {
                return this._id;
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseControl.prototype, "dataId", {
            get: function () {
                return this._dataId;
            },
            enumerable: false,
            configurable: true
        });
        BaseControl.prototype.setDataId = function (dataId) {
            this._dataId = dataId;
        };
        BaseControl.prototype.subscribe = function (eventType, observer) {
            if (!this._observers[eventType]) {
                this._observers[eventType] = new Observer(eventType);
            }
            this._observers[eventType].subscribe(observer);
        };
        BaseControl.prototype.unsubscribe = function (eventType, observer) {
            if (this._observers[eventType]) {
                this._observers[eventType].unsubscribe(observer);
            }
        };
        BaseControl.prototype.subscribeGlobal = function (observer) {
            this._globalObserver.subscribe(observer);
        };
        BaseControl.prototype.unsubscribeGlobal = function (observer) {
            this._globalObserver.unsubscribe(observer);
        };
        BaseControl.prototype.notify = function (eventType, data) {
            if (this._observers[eventType]) {
                this._observers[eventType].notifyAll(data);
            }
            this._globalObserver.notifyAll(eventType, data);
        };
        Object.defineProperty(BaseControl.prototype, "container", {
            get: function () {
                return this._container;
            },
            enumerable: false,
            configurable: true
        });
        BaseControl.prototype.replaceContainer = function (content) {
            this._container = container_1.replaceElement(this._container, content);
        };
        BaseControl.prototype.refresh = function () {
        };
        ;
        return BaseControl;
    }());
    exports.default = BaseControl;
});
//# sourceMappingURL=baseControl.js.map