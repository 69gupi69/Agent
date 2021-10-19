import { DisposeManager } from "./types/idisposable";
import { initNavigation } from "./navigator";
import { DefaultResizer } from "./modules/utils/container";

interface IInitSettings {
    offset?: number,
    windowResize?: (h: number) => void
}

export let viewContainer: JQuery;
export let disposeManager: DisposeManager;
export let router: kendo.Router;
const offsetMain = $("header").height() + $("footer").height();
const offsetDefault = 5;

export function initLayout(currentPage: string, settings?: IInitSettings) {
    kendo.culture("ru-RU");
    disposeManager = new DisposeManager();
    viewContainer = $("#main");
    let menuDiv = $("#mainMenu");
    router = new kendo.Router({
        hashBang: true
    });
    initNavigation(currentPage, menuDiv, router);
    router.bind("change", () => {
        disposeManager.dispose();
    });

    DefaultResizer.action = h => {
        let offset = (settings && settings.offset) ? settings.offset : offsetDefault;
        if (settings && settings.windowResize) {
            let rh = h - offset - offsetMain;
            settings.windowResize(rh);
        }
        else {
            viewContainer.height(h - offset - offsetMain);
        }
    };
    DefaultResizer.resize();
    window.onunload = () => {
    }

}

export function parseNullableString(s: string): string {
    if (!s) {
        return null;
    }
    if (s.toLowerCase() === "null") {
        return null;
    }
    return s;
}

export function guid(): string {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }

    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
}
