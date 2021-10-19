define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.appPages = exports.menuConfig = void 0;
    exports.menuConfig = {
        stateful: true,
        items: [
            {
                text: "Результаты расчётов",
                id: "menuCalculations",
                page: "calculations",
                url: "grid",
                title: "Результаты расчётов",
            },
        ]
    };
    exports.appPages = window["AppPages"];
});
//# sourceMappingURL=mainMenu.js.map