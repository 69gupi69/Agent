import { menuConfig } from "./mainMenu";
import { MenuItem } from "./types/menu";
import { Stack } from "./types/datastructures";
import { redirect } from "./modules/utils/redirect";
const routeHistoryDepth = 100;

class RouteHistory extends Stack<string> {
    constructor(depth: number) {
        super(depth);
    }

    get current() {
        return this.get(0);
    }

    get previous() {
        return this.get(1);
    }
}

export let routeHistory = new RouteHistory(routeHistoryDepth);

function getRouteTitles(currentPage: string, items: MenuItem[]): { [route: string]: string } {
    let result: { [route: string]: string } = {};
    for (let item of items) {
        if (item.page === currentPage) {
            result[item.url] = item.title;
        }
        if (item.items) {
            let subItems = getRouteTitles(currentPage, item.items);
            if (subItems) {
                for (let route in subItems) {
                    result[route] = subItems[route];
                }
            }
        }
    }
    return result;
}

export function makeUrlParametersPart(args: { [id: string]: any }): string {
    let data: string[] = [];
    for (let k in args) {
        if (k[0] !== "_")
            data.push(`${k}=${args[k]}`);
    }
    return data.join("&");
}

export function getMenuDataSource(items: MenuItem[], currentPage?: string): any[] {
    let result = [];
    for (let item of items) {
        let data = {};
        data["attr"] = {};
        data["text"] = item.text;
        data["attr"]["id"] = item.id || '';
        data["attr"]["data-url"] = item.url || '';
        data["attr"]["data-page"] = item.page || '';
        data["attr"]["data-title"] = item.title || '';
        if (currentPage && item.page === currentPage) {

        }
        if (item.items) {
            data["items"] = getMenuDataSource(item.items);
        }
       
        result.push(data);
    }
    return result;
}

export function initNavigation(currentPage: string, container: JQuery, router: kendo.Router) {
    let titles = getRouteTitles(currentPage, menuConfig.items);
    let setTitle = (title?: string) => {
        if (title) {
            container.find("[data-role=title]").text(title);
            container.find("[data-role=title]").show();
        } else {
            container.find("[data-role=title]").text();
            container.find("[data-role=title]").hide();
        }
    };
    let menuData = getMenuDataSource(menuConfig.items, currentPage);
    let ul = $("<ul>").appendTo(container.find("[data-role=navigation]"));
    ul.kendoMenu({
        dataSource: menuData,
        select: e => {
            let url = $(e.item).attr("data-url");
            if (url === "none") {
            }
            else {
                let page = $(e.item).attr("data-page");
                let title = $(e.item).attr("data-title");
                let id = $(e.item).attr("id");
                setTitle(title);
                if (url && page) {
                    if (page === currentPage) {
                        router.navigate(url);
                        setTitle(title);
                    } else {
                        if (id != undefined) {
                            switch (id) {
                                case "menuCalculations": {
                                    setTitle("Результаты расчетов");
                                    redirect("calculations", "calculationsGrid");
                                    break;
                                }
                                case "menuIdentifiers": {
                                    setTitle("Список идентификаторов");
                                    redirect("identifiers", "identifiersGrid");
                                    break;
                                }
                            }
                        }
                        else {
                            if ($(e.item).attr("aria-haspopup")) {
                                console.log("aria-haspopup")
                                e.preventDefault();
                            } else {
                                console.warn(`There are no modules for id ${id}`);
                            }
                        }
                    }
                }
            }
        }
    });
    let currentItem: MenuItem = undefined;
    for (let item of menuConfig.items) {
        currentItem = item;
        }
    router.bind("change",
        e => {
            let url = e.url.indexOf("?") > -1 ? e.url.substring(0, e.url.indexOf("?")) : e.url;
            let baseUrl = (url && url.indexOf("/") > 0) ? url.split("/")[0] : url;
            if (titles[baseUrl]) {
                setTitle(titles[url]);
            } else {
                setTitle(currentItem.title);
            }
            if (e.url.indexOf("/") > 0) {
                ul.data("kendoMenu").enable("li", false);
            } else {
                ul.data(`kendoMenu`).enable(`li:not([data-active=1])`, true);
            }
            if (currentItem.items && currentItem.items.length) {
                for (let item of currentItem.items) {
                    if (item.url === baseUrl) {
                        ul.data("kendoMenu").enable(`#${item.id}`, false);
                        ul.find(`#${item.id}`).addClass("k-state-active");
                        ul.find(`#${item.id}`).attr("data-active", 1);
                    } else {
                        ul.data("kendoMenu").enable(`#${item.id}`, true);
                        ul.find(`#${item.id}`).removeClass("k-state-active");
                        ul.find(`#${item.id}`).removeAttr("data-active");
                    }
                }
            }
        });
    return router;
}