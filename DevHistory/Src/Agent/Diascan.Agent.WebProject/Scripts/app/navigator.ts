import { appPages, menuConfig } from "./mainMenu";
import { MenuItem } from "./types/menu";
import { Stack } from "./types/datastructures";

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


function createMenu(items: MenuItem[]): JQuery {
    let ul = $("<ul/>");
    for (let item of items) {
        let li = item.id ? $(`<li id="${item.id}">${item.text}</li>`) : $(`<li>${item.text}</li>`);
        li.data("navigation", item);
        if (item.items) {
            li.append(createMenu(item.items));
        }
        ul.append(li);
    }
    return ul;
}

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

export function initNavigation(currentPage: string, container: JQuery, router: kendo.Router) {
    let titles = getRouteTitles(currentPage, menuConfig.items);
    let ul = createMenu(menuConfig.items);
    container.find("[data-role=navigation]").append(ul);
    let setTitle = (title?: string) => {

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
        select: e => {
            let item = $(e.item).data("navigation") as MenuItem;
            if (item.url && item.page) {
                if (item.page === currentPage) {
                    router.navigate(item.url);
                    setTitle(item.title);
                }
                else {
                    if (appPages[item.page]) {
                        let pageUrl = appPages[item.page];
                        window.location.href = `${pageUrl}#!${item.url}`;
                    }
                    else {
                        console.error(`Page ${item.page} not found`);
                    }
                }
            }
            else {
                console.warn(`There are no modules for id ${item.id}`);
            };
        }
    });
    router.bind("change", e => {
        routeHistory.push(e.url);
        let url = e.url.indexOf("?") > -1 ? e.url.substring(0, e.url.indexOf("?")) : e.url;
        if (titles[url]) {
            setTitle(titles[url]);
        }
        else if (url && url.indexOf("/") > 0) {
            let baseUrl = url.split("/")[0];
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
