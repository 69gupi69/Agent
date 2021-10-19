import ObserverControl from "./observer";

export function replaceContent(container: JQuery, content: string) {
    return container.html(content);
}

export function replaceElement(container: JQuery, content: string) {
    let contEl = $(content);
    container.replaceWith(contEl);
    return contEl;
}

export class Resizer extends ObserverControl {
    action: (number) => void;

    constructor(action?: (height: number) => void) {
        super();
        this.action = action;
        this.init();
    }

    resize() {
        let windowHeight = $(window).height();
        if (this.action) {
            this.action(windowHeight);

        }
        this.notify("resize", { windowHeight: windowHeight });
    }

    private init() {
        $(window).resize(() => {
            this.resize();
        });
        this.resize();
    }
}

export let DefaultResizer = new Resizer();