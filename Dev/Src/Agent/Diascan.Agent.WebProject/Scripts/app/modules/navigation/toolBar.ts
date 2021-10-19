import BaseControl from "../baseControl";


interface IToolBarItem {
    type: 'ToggleButton' | 'Button',
    id: string,

}

interface IToolBarButton extends IToolBarItem {
    text?: string

}

interface IToolBarToggleButton extends IToolBarButton {
    checked?: boolean
    visible?: boolean
}

interface IToolBarConfig {
    items?: (IToolBarButton | IToolBarToggleButton)[]
}

class ToolBar extends BaseControl {
    private static _mapToKendoItems(items: IToolBarItem[]): kendo.ui.ToolBarItem[] {
        let result: kendo.ui.ToolBarItem[] = [];
        for (let item of items) {
            if (item.type === "Button") {
                result.push({
                    id: item.id,
                    type: "button",
                    text: (<IToolBarButton>item).text,
                    togglable: false
                });
                continue;
            }
            if (item.type === "ToggleButton") {
                result.push({
                    id: item.id,
                    type: "button",
                    text: (<IToolBarToggleButton>item).text,
                    togglable: true,
                    selected: (<IToolBarToggleButton>item).checked,
                    hidden: !(<IToolBarToggleButton>item).visible
                });
            }
        }
        return result;
    }

    dispose() {
    }

    private _clickHandler = (e) => {
        this.notify("click", { id: e.id });
        this.notify(`click#${e.id}`);
    };

    private _toggleHandler = (e) => {
        this.notify("toggle", { id: e.id });
        this.notify(`toggle#${e.id}`);
    };
    private readonly _config: IToolBarConfig;

    protected get toolBar(): kendo.ui.ToolBar {
        return <kendo.ui.ToolBar>this.container.data("kendoToolBar");
    }

    constructor(container: JQuery, config?: IToolBarConfig) {
        super(container);
        this._config = config;
    }

    public init() {
        this.container.kendoToolBar({
            items: ToolBar._mapToKendoItems(this._config && this._config.items ? this._config.items : []),
            click: this._clickHandler,
            toggle: this._toggleHandler
        });
    }

    public getCheked(id: string): boolean {
        return this.container.find(`#${id}`).hasClass("k-state-active");
    }
}

export { ToolBar, IToolBarConfig }
export default ToolBar