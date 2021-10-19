import { DefaultResizer } from "../utils/container";
import ToolBar, { IToolBarConfig } from "../navigation/toolBar";
import BaseControl from "../baseControl";

interface IToolBarPanelConfig {
    toolBarHeight?: number;
    toolBarConfig?: IToolBarConfig;
    createContent: (c: JQuery) => BaseControl;
}

class ToolBarPanel extends BaseControl {
    private _toolBarElement: JQuery;
    private _contentElement: JQuery;
    private readonly _toolBarHeight: number;
    private _toolBar: ToolBar;
    private _contentControl: BaseControl;
    private readonly _toolBarConfig: IToolBarConfig;
    private readonly _createContent: (c: JQuery) => BaseControl;

    public get contentControl(): BaseControl {
        return this._contentControl;
    }

    dispose() {
        DefaultResizer.unsubscribe("resize", this._resizeEvent);
        this._contentControl.dispose();
        this._toolBar.unsubscribeGlobal(this._toolBarEvent);
    }

    constructor(container: JQuery, config?: IToolBarPanelConfig) {
        super(container);
        this._toolBarConfig = config && config.toolBarConfig;
        this._createContent = config && config.createContent;
        this.container.empty();
    }

    public init() {

        this._toolBarElement = $(`<div style="border: none; box-sizing:border-box;">`);
        this._toolBarElement.height(this._toolBarHeight);
        this._toolBarElement.appendTo(this.container);
        this._toolBar = new ToolBar(this._toolBarElement, this._toolBarConfig);
        this._contentElement = $("<div>");
        this._resize();
        this._contentElement.appendTo(this.container);
        DefaultResizer.subscribe("resize", this._resizeEvent);
        this._toolBar.subscribeGlobal(this._toolBarEvent);
        this._toolBar.init();
        if (this._createContent) {
            this._contentControl = this._createContent(this._contentElement);
        }
    }

    private _toolBarEvent = (e) => {
        this.notify(e.type, e.data);
    };

    private _resizeEvent = () => {
        this._resize();
    };

    private _resize() {
        let contentHeight = this.container.height() - this._toolBarHeight;
        this._contentElement.height(contentHeight);
    }

    public getCheked(id: string): boolean {
        return this._toolBar.getCheked(id);
    }

}

export { ToolBarPanel }
export default ToolBarPanel
