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
define(["require", "exports", "./mainMenu", "./types/datastructures"], function (require, exports, mainMenu_1, datastructures_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.initNavigation = exports.makeUrlParametersPart = exports.routeHistory = void 0;
    var routeHistoryDepth = 100;
    var RouteHistory = /** @class */ (function (_super) {
        __extends(RouteHistory, _super);
        function RouteHistory(depth) {
            return _super.call(this, depth) || this;
        }
        Object.defineProperty(RouteHistory.prototype, "current", {
            get: function () {
                return this.get(0);
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(RouteHistory.prototype, "previous", {
            get: function () {
                return this.get(1);
            },
            enumerable: false,
            configurable: true
        });
        return RouteHistory;
    }(datastructures_1.Stack));
    exports.routeHistory = new RouteHistory(routeHistoryDepth);
    function createMenu(items) {
        var ul = $("<ul/>");
        for (var _i = 0, items_1 = items; _i < items_1.length; _i++) {
            var item = items_1[_i];
            var li = item.id ? $("<li id=\"" + item.id + "\">" + item.text + "</li>") : $("<li>" + item.text + "</li>");
            li.data("navigation", item);
            if (item.items) {
                li.append(createMenu(item.items));
            }
            ul.append(li);
        }
        return ul;
    }
    function getRouteTitles(currentPage, items) {
        var result = {};
        for (var _i = 0, items_2 = items; _i < items_2.length; _i++) {
            var item = items_2[_i];
            if (item.page === currentPage) {
                result[item.url] = item.title;
            }
            if (item.items) {
                var subItems = getRouteTitles(currentPage, item.items);
                if (subItems) {
                    for (var route in subItems) {
                        result[route] = subItems[route];
                    }
                }
            }
        }
        return result;
    }
    function makeUrlParametersPart(args) {
        var data = [];
        for (var k in args) {
            if (k[0] !== "_")
                data.push(k + "=" + args[k]);
        }
        return data.join("&");
    }
    exports.makeUrlParametersPart = makeUrlParametersPart;
    function initNavigation(currentPage, container, router) {
        var titles = getRouteTitles(currentPage, mainMenu_1.menuConfig.items);
        var ul = createMenu(mainMenu_1.menuConfig.items);
        container.find("[data-role=navigation]").append(ul);
        var setTitle = function (title) {
            if (title) {
                container.find("[data-role=title]").text(title);
                container.find("[data-role=title]").show();
            }
            else {
                container.find("[data-role=title]").text();
                container.find("[data-role=title]").hide();
            }
            //StaffLog.Action(LogAction.UserVisitedThePage.toString() + ` "${title}"`, { currentUrl: window.location.href });
        };
        ul.kendoMenu({
            select: function (e) {
                var item = $(e.item).data("navigation");
                if (item.url && item.page) {
                    if (item.page === currentPage) {
                        router.navigate(item.url);
                        setTitle(item.title);
                    }
                    else {
                        if (mainMenu_1.appPages[item.page]) {
                            var pageUrl = mainMenu_1.appPages[item.page];
                            window.location.href = pageUrl + "#!" + item.url;
                        }
                        else {
                            console.error("Page " + item.page + " not found");
                        }
                    }
                }
                else {
                    console.warn("There are no modules for id " + item.id);
                }
                ;
            }
        });
        router.bind("change", function (e) {
            exports.routeHistory.push(e.url);
            var url = e.url.indexOf("?") > -1 ? e.url.substring(0, e.url.indexOf("?")) : e.url;
            if (titles[url]) {
                setTitle(titles[url]);
            }
            else if (url && url.indexOf("/") > 0) {
                var baseUrl = url.split("/")[0];
                if (titles[baseUrl]) {
                    setTitle(titles[baseUrl]);
                }
            }
            else {
                setTitle();
            }
        });
        return router;
    }
    exports.initNavigation = initNavigation;
});
//# sourceMappingURL=navigator.js.map