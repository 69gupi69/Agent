define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.globalNotifier = void 0;
    var content = "<div style='display: none'></div>";
    var container = $(content).appendTo("body");
    container.kendoNotification({});
    var globalNotifier = container.data("kendoNotification");
    exports.globalNotifier = globalNotifier;
});
//# sourceMappingURL=notificator.js.map