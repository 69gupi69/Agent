import BaseControl from "../baseControl";
import { DefaultResizer } from "../utils/container";
import { GridCommandHelper } from "../utils/gridHelpers";



interface CustomGridColumnCommandItem extends kendo.ui.GridColumnCommandItem {
    title?: string;
    text?: any;
}

interface CustomGridColumn extends kendo.ui.GridColumn {
    command?: CustomGridColumnCommandItem[];
}

interface CustomGridOptions extends kendo.ui.GridOptions {
    columns?: CustomGridColumn[];
}



interface IGridEvents {
    remove?: (e) => void;
    mailing?: (e) => void;
    download?: (e) => void;
    mailingSettings?: (e) => void;
    export?: (e) => void;
    import?: (e) => void;
    edit?: (e) => void;
    add?: (e) => void;
}

interface IGridOptions {
    event?: IGridEvents;
}

abstract class BaseGrid extends BaseControl {

    public get options(): IGridOptions {
        return this._options;
    }

    private readonly _options: IGridOptions;
    private _selectedRowIndex?: number;

    public disableCommand(id: string) {
        let sel = `a.k-grid-${id}`;
        this.container.find("div.k-grid-toolbar").find(sel).attr("disabled", "disabled");
    }

    public enableCommand(id: string) {
        let sel = `a.k-grid-${id}`;
        this.container.find("div.k-grid-toolbar").find(sel).removeAttr("disabled");
    }

    public hideCommand(id: string) {
        let sel = `a.k-grid-${id}`;
        this.container.find("div.k-grid-toolbar").find(sel).hide();
    }

    public showCommand(id: string) {
        let sel = `a.k-grid-${id}`;
        this.container.find("div.k-grid-toolbar").find(sel).show();
    }

    private columnCommand = (command: string, data: any, target: HTMLElement) => {
        let row = $(target).closest("tr");
        this.grid.select(row);
        this._selectedRowIndex = row.index();
        this.notify(command, data);
    };

    protected get grid(): kendo.ui.Grid {
        return this.container.data("kendoGrid") as kendo.ui.Grid;
    }

    protected abstract get gridOptions(): CustomGridOptions;

    protected constructor(container: JQuery, id: string, options: IGridOptions) {
        super(container, id);
        this._options = options;
    }

    dispose() {
        DefaultResizer.unsubscribe("resize", this.resizeEvent);

        if (this._options && this._options.event) {
            if (this._options.event.remove) {
                this.unsubscribe("deleteCommand", this._options.event.remove);
            }
            if (this._options.event.mailingSettings) {
                this.unsubscribe("mailingSettingsCommand", this._options.event.mailingSettings);
            }
            if (this._options.event.download) {
                this.unsubscribe("downloadCommand", this._options.event.download);
            }
        }
        if (this.grid !== undefined)
            this.grid.destroy();
    }

    protected afterInit() {

    }

    protected setToolbarHandler(id: string, handler: () => void) {
        let sel = `a.k-grid-${id}`;
        this.container.find("div.k-grid-toolbar").find(sel).click(e => {
            e.preventDefault();
            if (!e.target.attributes["disabled"])
                handler();
        });
    }

    protected setToolbarImageClass(id: string, cls: string) {
        let sel = `a.k-grid-${id}`;
        this.container.find("div.k-grid-toolbar").find(sel)
            .prepend(`<span class="k-icon ${cls}"></span>`);
    }


    protected setUploader(id: string, url: string, onUpload: (e)=> void){
        let sel = `a.k-grid-${id}`;
        let button = this.container.find("div.k-grid-toolbar").find(sel);
        button.prepend(`<input name="files" id="files" type="file" aria-label="files" style="display: none" />`);

        let uploader = $(`#files`).kendoUpload(
            {
                async: {
                    saveUrl: url,
                    autoUpload: true,
                },
                showFileList: false,
                upload: onUpload
            }).get(0);

        button.children().get(0).style.display = "none";
        button.click(()=> {
                uploader.click();
            } 
        );
    }
    init() {
        this.container.kendoGrid(this.gridOptions);
        DefaultResizer.subscribe("resize", this.resizeEvent);
        this.grid.bind("dataBound", () => {
            if (this._selectedRowIndex >= 0) {
                this.grid.select(`tr:eq(${this._selectedRowIndex})`);
            }
        });
        this.grid.bind("page", () => {
            this._selectedRowIndex = -1;
        });
        this.grid.bind("columnResize", () => {
            this.grid.refresh();
        });
        if (this._options && this._options.event) {
            if (this._options.event.remove) {
                this.subscribe("deleteCommand", this._options.event.remove);
            }
            if (this._options.event.mailing) {
                this.subscribe("mailingCommand", this._options.event.mailing);
            }
            if (this._options.event.mailingSettings) {
                this.subscribe("mailingSettingsCommand", this._options.event.mailingSettings);
            }
            if (this._options.event.download) {
                this.subscribe("downloadCommand", this._options.event.download);
            }
            if (this._options.event.export) {
                this.subscribe("exportCommand", this._options.event.export);
            }
            if (this._options.event.import) {
                this.subscribe("importCommand", this._options.event.import);
            }
            if (this._options.event.edit) {
                this.subscribe("editCommand", this._options.event.edit);
            }
            if (this._options.event.add) {
                this.subscribe("addCommand", this._options.event.add);
            }
        }


        new GridCommandHelper(this.grid, this.columnCommand);
        //let that = this;
        this.setToolbarHandler("mailingSettings", () => {
            //that.
            this.notify("mailingSettingsCommand", this);
        });

        this.setToolbarHandler("importToExcel", () => {
            this.notify("importCommand");
        });

        this.setToolbarHandler("editId", () => {
            this.notify("editCommand");
        });
        this.setToolbarHandler("addIdentifiers", () => {
            this.notify("addCommand");
        });
        this.afterInit();
    }


    refresh(): void;
    refresh(noData: boolean, firstPage?: boolean): void;
    refresh(noData?: boolean, firstPage?: boolean): void {
        if (!noData) {
            if (firstPage) {
                this.grid.dataSource.page(0);
            }
            else {
                this.grid.dataSource.read();
            }
        } else {
            this.grid.refresh();
        }
    };

    protected resizeEvent = () => {
        if (this.grid) {
            this.grid.refresh();
        }
    };
}

export { BaseGrid, IGridOptions, CustomGridOptions, IGridEvents }
export default BaseGrid