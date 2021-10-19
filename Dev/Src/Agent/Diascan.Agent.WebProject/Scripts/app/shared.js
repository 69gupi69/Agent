define(["require", "exports", "./types/idisposable", "./navigator", "./modules/utils/container"], function (require, exports, idisposable_1, navigator_1, container_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.guid = exports.parseNullableString = exports.initLayout = exports.router = exports.disposeManager = exports.viewContainer = void 0;
    var offsetMain = $("header").height() + $("footer").height();
    var offsetDefault = 5;
    function initLayout(currentPage, settings) {
        kendo.culture("ru-RU");
        exports.disposeManager = new idisposable_1.DisposeManager();
        exports.viewContainer = $("#main");
        var menuDiv = $("#mainMenu");
        exports.router = new kendo.Router({
            hashBang: true
        });
        navigator_1.initNavigation(currentPage, menuDiv, exports.router);
        exports.router.bind("change", function () {
            exports.disposeManager.dispose();
        });
        container_1.DefaultResizer.action = function (h) {
            var offset = (settings && settings.offset) ? settings.offset : offsetDefault;
            if (settings && settings.windowResize) {
                var rh = h - offset - offsetMain;
                settings.windowResize(rh);
            }
            else {
                exports.viewContainer.height(h - offset - offsetMain);
            }
        };
        container_1.DefaultResizer.resize();
        window.onunload = function () {
        };
    }
    exports.initLayout = initLayout;
    function parseNullableString(s) {
        if (!s) {
            return null;
        }
        if (s.toLowerCase() === "null") {
            return null;
        }
        return s;
    }
    exports.parseNullableString = parseNullableString;
    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    }
    exports.guid = guid;
});
//# sourceMappingURL=shared.js.map