import BaseControl from "../baseControl";
import { globalConfirm } from "../layout/confirmWindow";
import { globalNotifier } from "../layout/notificator";
import { DefaultResizer } from "../utils/container";
import WaitModal from "../layout/waitModal";
//import { StaffLog } from "../utils/staffLog";
//import { LogAction } from "../../enums/logAction";

enum EditorMode {
    Add,
    Edit,
    View
}

interface IEditorMessages {
    postSuccess: string,
    putSuccess: string,
    cancelTitle: string,
    cancelMessage: string
}

interface IEditorEvents {
    save?: (e?) => void;
    cancel?: () => void;
}

interface IEditorConfig {
    event?: IEditorEvents;
}

abstract class BaseEditor extends BaseControl {
    //private readonly _mode: EditorMode;
    //private readonly _dataId: string | number;

    //protected get mode(): EditorMode {
    //    return this._mode;
    //}

    //protected get dataId(): string | number {
    //    return this._dataId
    //}

    get viewModel(): kendo.data.ObservableObject {
        return this._viewModel;
    }

    private resizeEvent = () => {
        this.resize();
    };


    protected resize() {
        let parentHeight = this.container.parent().height();
        let panelHeaderHeight = this.container.find("div.panel-heading").height();
        let panelFooterHeight = this.container.find("div.panel-footer").height();
        let k = panelHeaderHeight + panelFooterHeight + 16;
        this.container.find("div.panel-body").css("max-height", `${parentHeight - k}px`);
    }


    protected clickSave() {
        //if (this.validator.validate()) {
            WaitModal.show("Данные сохраняются");
           // StaffLog.Action(LogAction.SaveButtonClickedInTheInterface.toString() + `"${this.title}"`,
            //    { currentUrl: window.location.href });
            let data = this.formData;
            //if (this._mode === EditorMode.Edit) {
                this.putData(this.getId(this.initData), data,
                    () => {
                        WaitModal.hide();
                        this.notify("save");
                        globalNotifier.success(this.messages.putSuccess);
                        //StaffLog.Action(LogAction.UserChangedObjectByType.toString() +
                        //    `"${data.Type}" с id: "${data.id}"`, { currentUrl: window.location.href });
                    },
                    message => {
                        WaitModal.hide();
                        globalNotifier.error(message);

                    });
            //}
            //else {
            //    this.postData(data,
            //        id => {
            //            WaitModal.hide();
            //            this.notify("save", { id: id });
            //            globalNotifier.success(_(this.messages.postSuccess));
            //           // StaffLog.Action(LogAction.UserAddObjectByType.toString() +
            //           //     `"${data.Type}" с id: "${data.id}"`, { currentUrl: window.location.href });
            //        },
            //        message => {
            //            WaitModal.hide();
            //            globalNotifier.error(message);

            //        });
            //}
        //}
    }

    protected clickCancel() {
        //StaffLog.Action(LogAction.CancelButtonClickedInTheInterface.toString() +
        //    `"${this.title}"`, { currentUrl: window.location.href });
        if (this.viewModel.get("saveEnabled")) {
            globalConfirm.show(
                this.messages.cancelTitle,
                this.messages.cancelMessage.replace("{0}", this.getName(this.initData)),
                () => {
                    this.notify("cancel");
                });
        }
        else {
            this.notify("cancel");
        }
    }


    protected enabledSaveButton(enabled: boolean) {
        //if (this.validator.validate()) {
            this.viewModel.set("saveEnabled", enabled);
        //}
    }

    protected get title(): string {
        return this._viewModel.get("title");
    }

    protected set title(value: string) {
        this._viewModel.set("title", value);
    }

    //protected validator: Validator;


    protected get initData(): any {
        return this.viewModel.get("initData").toJSON();
    }

    protected get datasources(): any {
        return this.viewModel.get("datasources");
    }

    protected get formData(): any {
        return this.viewModel.get("formData").toJSON();
    }

    protected get metaData(): any {
        return this.viewModel.get("metaData").toJSON();
    }

    protected initField(field: string, data: any) {
        this._viewModel.set(`initData.${field}`, data);
        this._viewModel.set(`formData.${field}`, data);
    }

    protected setField(field: string, data: any) {
        this._viewModel.set(`formData.${field}`, data);
    }

    protected setMetaData(data: { [id: string]: any; });
    protected setMetaData(field: string, data: any);
    protected setMetaData(...args: any[]) {
        if (args.length === 2) {
            let field = <string>args[0];
            let data = args[1];
            this._viewModel.set(`metaData.${field}`, data);
            return;
        }
        if (args.length === 1) {
            let data = <{ [id: string]: any; }>args[0];
            if (data) {
                for (let k in data) {
                    this.setMetaData(k, data[k]);
                }
            }
            return;
        }

    }


    protected setDataSource(id: string, dataSource: kendo.data.DataSource | kendo.data.HierarchicalDataSource) {
        this._viewModel["datasources"][id] = dataSource;
        dataSource.read();
    }

    protected abstract getContent(): string;

    protected abstract getDatasources(): { [key: string]: kendo.data.DataSource | kendo.data.HierarchicalDataSource };

    protected abstract getData(id: string | number, success: (data: any) => void,
        error: (message, jqXHR: JQueryXHR) => void): void;

    protected abstract putData(id: string | number, data: any, success: () => void,
        error: (message, jqXHR: JQueryXHR) => void): void;

    protected abstract postData(data: any, success: (id?: string | number) => void,
        error: (message, jqXHR: JQueryXHR) => void): void;

    protected getListeners(): { [key: string]: (e?: any) => void } {
        return {};

    }

    protected getMetaData(): { [key: string]: any } {
        return {};
    }
    protected getCustomRule(): (elmnt: Element) => boolean {
        return null;
    }


    protected messages: IEditorMessages = {
        putSuccess: "Запись изменена успешно",
        postSuccess: "Запись добавлена успешно",
        cancelTitle: "Отмена изменений",
        cancelMessage: "По объекту \"{0}\" есть несохраненные изменения. Отменить?",
    };

    protected abstract getId(data: any): string | number;

    protected abstract getName(data: any): string;

    private _viewModel: kendo.data.ObservableObject = null;

    private readonly _config: IEditorConfig;
    protected get config(): IEditorConfig {
        return this._config;
    }

    dispose() {
        DefaultResizer.unsubscribe("resize", this.resizeEvent);
        if (this._config && this._config.event) {
            if (this._config.event.save) {
                this.unsubscribe("save", this._config.event.save);
            }
            if (this._config.event.cancel) {
                this.unsubscribe("cancel", this._config.event.cancel);
            }
        }
        //this.validator.dispose();
        kendo.unbind(this.container);
        kendo.destroy(this.container);
    }

    protected constructor(container: JQuery, id: string, dataId?: string | number, config?: IEditorConfig) {
        super(container, id);
        this._config = config;
        //this._mode = dataId ? EditorMode.Edit : EditorMode.Add;
        //this._dataId = dataId;
        this.replaceContainer(this.getContent());
        let customRule = this.getCustomRule();
       // this.validator = new Validator(this.container, { customRule: customRule });
    }


    public init() {
        DefaultResizer.subscribe("resize", this.resizeEvent);
        this.resize();
        this._viewModel = kendo.observable({
            initData: {},
            formData: {},
            metaData: this.getMetaData(),
            title: "",
            datasources: this.getDatasources(),
            listeners: this.getListeners(),
            saveEnabled: false,
            clickSave: () => this.clickSave(),
            clickCancel: () => this.clickCancel()
        });
        this.notify("binding", { viewModel: this._viewModel });
        kendo.bind(this.container, this._viewModel);
        //if (this.mode == EditorMode.Edit) {
        //    WaitModal.show("Данные загружаются", 500);
        //    this.getData(this._dataId,
        //        data => {
        //            WaitModal.hide();
        //            this._viewModel.set("initData", data);
        //            this._viewModel.set("formData", data);
        //        },
        //        message => {
        //            WaitModal.hide();
        //            globalNotifier.error(message)
        //        });
        //}
        this._viewModel.bind("change", (e) => {
            if (e.field.indexOf("formData.") === 0) {
                let se = JSON.stringify(this._viewModel.get("initData")) !== JSON.stringify(this._viewModel.get("formData"));
                //se = se && this.validator.validate();
                this._viewModel.set("saveEnabled", se);
            }
        });

        if (this._config && this._config.event) {
            if (this._config.event.save) {
                this.subscribe("save", this._config.event.save);
            }
            if (this._config.event.cancel) {
                this.subscribe("cancel", this._config.event.cancel);
            }
        }
    }

}

interface IRemoverMessages {
    success: string,
    title: string,
    message: string
}

abstract class BaseRemover {
    protected abstract getId(data: any): string | number;

    protected abstract getName(data: any): string;

    protected abstract deleteData(id: string | number, success: () => void, error: (message, jqXHR: JQueryXHR) => void): void;

    private readonly _data: any;
    private readonly _success: () => void;
    private readonly _error: (message) => void;
    protected messages: IRemoverMessages = {
        success: "Данные удалены успешно",
        title: "Удаление записи",
        message: "Удалить запись {0}?"
    };

    protected constructor(data: any, success?: () => void, error?: (message) => void) {
        this._data = data;
        this._success = success;
        this._error = error;
        this._remove();
    }

    private _remove() {
        globalConfirm.show(this.messages.title,
            this.messages.message.replace("{0}", this.getName(this._data)),
            () => {
                this.deleteData(this.getId(this._data),
                    () => {
                        globalNotifier.success(this.messages.success);
                        //StaffLog.Action(LogAction.UserDeleteObjectByType.toString() + `с id: "${this._data.id}" `, { currentUrl: window.location.href });
                        if (this._success) {
                            this._success();
                        }
                    },
                    (message) => {
                        globalNotifier.error(message);
                        if (this._error) {
                            this._error(message);
                        }
                    });
            });
    }


}


interface IEmailingMessages {
    success: string,
    title:   string,
    message: string
}


abstract class BaseEmailing {
    protected abstract getId(data: any): string | number;

    protected abstract getName(data: any): string;

    protected abstract deleteData(id: string | number, success: () => void, error: (message, jqXHR: JQueryXHR) => void): void;

    private readonly _data: any;
    private readonly _success: () => void;
    private readonly _error: (message) => void;
    protected messages: IEmailingMessages = {
        success: "Отчет отправлен успешно",
        title:   "Отправка Excel отчета",
        message: "Отправить Excel отчет {0}, всем пользователям ИС \"Агент\"?"
    };

    protected constructor(data: any, success?: () => void, error?: (message) => void) {
        this._data = data;
        this._success = success;
        this._error = error;
        this._remove();
    }

    private _remove() {
        globalConfirm.show(this.messages.title,
            this.messages.message.replace("{0}", this.getName(this._data)),
            () => {
                this.deleteData(this.getId(this._data),
                    () => {
                        globalNotifier.success(this.messages.success);
                        //StaffLog.Action(LogAction.UserDeleteObjectByType.toString() + `с id: "${this._data.id}" `, { currentUrl: window.location.href });
                        if (this._success) {
                            this._success();
                        }
                    },
                    (message) => {
                        globalNotifier.error(message);
                        if (this._error) {
                            this._error(message);
                        }
                    });
            });
    }
}

export { BaseEditor, BaseRemover, BaseEmailing, EditorMode, IEditorConfig, IEditorEvents }