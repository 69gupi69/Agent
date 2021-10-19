import { DefaultResizer } from "../utils/container";
import BaseControl from "../baseControl";

const paneContent = `<div style="height: 100%; width: 100%; "></div>`;
const content = `<div style="width: 100%;height: 100%;box-sizing: border-box">
<div>${paneContent}</div><div>${paneContent}</div>
</div>`;

interface PaneConfig {
    size?: string;
    collapsible?: boolean;
    resizable?: boolean;
    create: (c: JQuery) => BaseControl;
    refresh?: boolean;
}

interface HorizontalSplitterConfig {
    leftPane: PaneConfig;
    rightPane: PaneConfig;
}

class HorizontalSplitter extends BaseControl {
    dispose() {
        if (this._left)
            this._left.dispose();
        if (this._right)
            this._right.dispose();
        DefaultResizer.unsubscribe("resize", this.resizeEvent);
    }

    private _left: BaseControl;
    private _right: BaseControl;
    private readonly _config: HorizontalSplitterConfig;

    protected get leftPaneContentContainer(): JQuery {
        return this.panes[0].children("div:first");
    }

    protected get rightPaneContentContainer(): JQuery {
        return this.panes[1].children("div:first");
    }

    private _clearLeft() {
        this.panes[0].children("div:first").replaceWith(paneContent);
    }

    private _clearRight() {
        this.panes[1].children("div:first").replaceWith(paneContent);
    }

    get panes(): JQuery[] {
        return [this.container.find("div.k-pane:first"), this.container.find("div.k-pane:last")];
    }

    protected get separator(): JQuery {
        return this.container.children("div[role=separator]");
    }


    constructor(container: JQuery, config: HorizontalSplitterConfig) {
        super(container);
        this._config = config;
        this.replaceContainer(content);
    }

    public init() {
        let config = this._config;
        this.container.kendoSplitter({
            orientation: "horizontal",
            panes: [{
                size: config.leftPane.size,
                collapsible: true,
                resizable: config.leftPane.resizable
            }, {
                size: config.rightPane.size,
                collapsible: config.rightPane.collapsible,
                resizable: config.rightPane.resizable,
                scrollable: false
            }],
            resize: () => {
                this.panes[1].width(this.container.width() - this.panes[1].offset().left + this.separator.width() + 6);
            }
        });
        DefaultResizer.subscribe("resize", this.resizeEvent);

        this._left = this._config.leftPane.create(this.leftPaneContentContainer);
        this._right = this._config.rightPane.create(this.rightPaneContentContainer);
        this.resizeEvent();
    }

    public setRight(f: (c: JQuery) => BaseControl) {
        if (this._right) {
            this._right.dispose();
            this._clearRight();

        }
        this._right = f(this.rightPaneContentContainer);
    }

    public get right(): BaseControl {
        return this._right;
    }

    public get left(): BaseControl {
        return this._left;
    }

    public setLeft(f: (c: JQuery) => BaseControl) {
        if (this._left) {
            this._left.dispose();
            this._clearLeft();
        }
        this._left = f(this.leftPaneContentContainer);
    }


    public resizeEvent = () => {
        this.panes[0].height(this.container.height());
        this.panes[1].height(this.container.height());
        this.separator.height(this.container.height());
    };

    refresh() {
        super.refresh();

        if (this._config && this._config.leftPane && this._config.leftPane.refresh) {
            if (this._left != null)
                this._left.refresh();

        }
        if (this._config.rightPane != null) {
            if (this._config && this._config.rightPane && this._config.rightPane.refresh) {
                if (this._right != null)
                    this._right.refresh();
            }
        }
    }

}


export { HorizontalSplitterConfig, PaneConfig, HorizontalSplitter }
export default HorizontalSplitter