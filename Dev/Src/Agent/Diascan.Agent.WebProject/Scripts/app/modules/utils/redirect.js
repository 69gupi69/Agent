define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.redirect = void 0;
    var modules = {};
    function init(m) {
        modules = m;
    }
    function redirect(module, url) {
        var moduleUrl = modules[module];
        if (moduleUrl) {
            window.location.href = url ? moduleUrl + "#!" + url : moduleUrl;
        }
        else {
            console.error("Url for module " + module + " not found");
        }
    }
    exports.redirect = redirect;
    init(window["AppPages"]);
});
//# sourceMappingURL=redirect.js.map