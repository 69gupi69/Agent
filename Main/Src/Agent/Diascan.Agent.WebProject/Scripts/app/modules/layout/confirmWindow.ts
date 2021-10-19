const content = `<div style="display: none;" 
     data-role="window" 
     data-modal="true" 
     data-actions=""
     data-min-width="500" data-bind="events:{open:open, deactivate:close}"  >
     <div class="modal-body" data-bind="text:message" style="font-size: large"></div>
     <div class="modal-footer">
        <button class="k-button k-button-icontext" data-bind="events:{click:yesClick}"><span class="k-icon k-i-check-outline"></span>Да</button>
        <button class="k-button k-button-icontext" data-bind="events:{click:noClick}"><span class="k-icon k-i-cancel"></span>Нет</button>
    </div>
</div>`;

class ConfirmWindow {
    private _container: JQuery;

    private _viewModel = kendo.observable({
        open: () => {
            this.window.center();
        },
        message: "",
        noClick: () => {
            this.window.close();
            if (this._cancelHandler) {
                this._cancelHandler();
            }
        },
        yesClick: () => {
            this.window.close();
            if (this._okHandler) {
                this._okHandler();
            }
        },
        close: () => {
            this.window.title("");
            this._viewModel.set("message", "");
        }
    });
    private _okHandler: () => void;
    private _cancelHandler: () => void;

    protected get window(): kendo.ui.Window {
        return this._container.data("kendoWindow") as kendo.ui.Window;
    }

    constructor() {
        this._init();
    }

    private _init() {
        this._container = $(content).appendTo("body");
        kendo.bind(this._container, this._viewModel);
    }

    show(title: string, message: string, okHandler: () => void, cancelHandler?: () => void) {
        this._okHandler = okHandler;
        this._cancelHandler = cancelHandler;
        this.window.title(title);
        this._viewModel.set("message", message);
        this.window.open();
    }
}

let globalConfirm = new ConfirmWindow();
export { ConfirmWindow, globalConfirm }
export default ConfirmWindow