//import BaseControl from "./baseControl";
//import { replaceContent } from "./utils/container";
////import { default as EventSender, TechnologicalEventType } from "./utils/event";

//interface IDocumentType {
//    id: string,
//    name: string
//}

//interface ICreateEventData {
//    title: string,
//    number?: string,
//    description?: string,
//    documentTypeId: string,
//    selectedFile?: string
//}

//interface IUploadControlInitData {
//    title?: string,
//    number?: string,
//    description?: string,
//    documentTypeId?: string,
//    fileName?: string
//}

//interface IUploadControlMessages {
//    cancel?: string,
//    save?: string
//}

//interface IUploadControlOptions {
//    saveUrl: string,
//    applicationCode: string,
//    documentType: string | IDocumentType[],
//    hideTitle?: boolean,
//    displayNumber?: boolean,
//    displayDescription?: boolean,
//    displayButtons?: boolean,
//    onCreate?: (e: ICreateEventData) => JQueryDeferred<string>,
//    //eventTypeId?: TechnologicalEventType,
//    initData?: IUploadControlInitData,
//    autoUpload?: boolean,
//    event?: {
//        onSuccess?: (e?) => void,
//        onError?: (e?) => void,
//        onCancel?: () => void,
//        onClear?: () => void,
//        onSelect?: (e: {}) => void
//    },
//    messages?: IUploadControlMessages
//}


////language=HTML
//const content = `
//    <div class="panel-body">
//        <div class="row panel-group" data-bind="visible:metaData.displayTitle">
//            <label class="col-md-3 control-label required">Заголовок</label>
//            <div class="col-md-9">
//                <input type="text" class="inputFluid k-textbox" data-bind="value:formData.title">
//            </div>
//        </div>
//        <div class="row panel-group" data-bind="visible:metaData.displayDocumentTypes">
//            <label class="col-md-3 control-label required">Тип документа</label>
//            <div class="col-md-6">
//                <input data-role="dropdownlist" class="inputFluid"
//                       data-bind="source:types, value:formData.documentTypeId"
//                       data-value-primitive="true"
//                       data-option-label="Не выбрано"
//                       data-value-field="id"
//                       data-text-field="name"
//                >
//            </div>
//        </div>
//        <div class="row panel-group" data-bind="visible:metaData.displayNumber">
//            <label class="col-md-3 control-label">Номер</label>
//            <div class="col-md-6">
//                <input type="text" id="Number" class="inputFluid k-textbox" data-bind="value:formData.number">
//            </div>
//        </div>
//        <div class="row panel-group" data-bind="visible:metaData.displayDescription">
//            <label class="col-md-3 control-label">Описание</label>
//            <div class="col-md-9">
//                <textarea class="inputFluid k-textbox" data-bind="value:formData.description"></textarea>
//            </div>
//        </div>
//        <div class="row panel-group nobuttons">
//            <div class="col-md-offset-3 col-md-9">
//                <input type="file" data-role="upload"

//                       data-async="{autoUpload:false, saveUrl:'{saveUrl}', removeUrl:'#', removeVerb:'GET'}"
//                       data-multiple="false"
//                       data-bind="events:{upload:upload, select:selectFile, clear:clearFile, remove:removeFile, success:successFile, error: errorFile}"

//                >
//            </div>
//        </div>
//    </div>
//    <div class="panel-footer" data-bind="visible:metaData.displayButtons">
//        <button class="k-button k-button-icontext" data-bind="events:{click:saveClick}, enabled:metaData.canSave}">
//            <span class="k-icon k-i-upload"></span><span data-bind="text:metaData.saveText"></span>
//        </button>
//        <button class="k-button k-button-icontext"
//                data-bind="events:{click:clearClick}, enabled:metaData.canClear}">
//            <span class="k-icon k-i-cancel-outline"></span><span data-bind="text:metaData.cancelText"></span>
//        </button>
//    </div>`;

//function getFileName(name: string): string {
//    return name.substr(0, name.lastIndexOf("."));
//}

//function getFileExtension(name: string): string {
//    return name.substr(1);
//}

//class UploadControl extends BaseControl {
//    static DefaultDocumentType = "8570eaf4-14e0-4956-ac0d-a1d75897ad3a";
//    //static DefaultEventType = TechnologicalEventType.UploadDocument;

//    private readonly _options: IUploadControlOptions;
//    private _viewModel: kendo.data.ObservableObject;
//    private _initData;
//    private _fileOnLoaded;
//    private _fileOnError;

//    protected get formData() {
//        return this._viewModel.get("formData");
//    }

//    protected get metaData() {
//        return this._viewModel.get("metaData");
//    }

//    protected setField(field: string, value: any) {
//        this._viewModel.set(`formData.${field}`, value);
//    }

//    public get title(): string {
//        return this.formData["title"];
//    }

//    public set title(value: string) {
//        this.setField("title", value);
//    }

//    public get documentTypeId(): string {
//        return this.formData["documentTypeId"];
//    }

//    public set documentTypeId(value: string) {
//        this.setField("documentTypeId", value);
//    }

//    public get number(): string {
//        return this.formData["number"];
//    }

//    public set number(value: string) {
//        this.setField("number", value);
//    }

//    public get description(): string {
//        return this.formData["description"];
//    }

//    public set description(value: string) {
//        this.setField("description", value);
//    }

//    public get isFileSelected(): boolean {
//        return this.metaData["fileSelected"];
//    }

//    protected get formChanged(): boolean {
//        let formData = this._viewModel.get("formData").toJSON();
//        return this._initData.title != formData.title || this._initData.description != formData.description ||
//            this._initData.documentTypeId != formData.documentTypeId || this._initData.number != formData.number ||
//            this._initData.selectedFile != formData.selectedFile;
//    }

//    protected get uploadControl(): kendo.ui.Upload {
//        return this.container.find("input[data-role=upload]").data("kendoUpload");
//    }

//    constructor(container: JQuery, options: IUploadControlOptions) {
//        super(container);
//        this._options = options;
//        let _content = content.replace("{saveUrl}", this._options.saveUrl).replace("{autoUpload}", this._options.autoUpload ? this._options.autoUpload.toString() : "false");
//        replaceContent(container, _content);
//        if (options.initData && options.initData.fileName) {
//            this.container.find("input[data-role=upload]").attr("data-files", `[{name:"${options.initData.fileName}",extension:""}]`);
//        }
//    }

//    dispose() {
//        this.uploadControl.destroy();
//    }

//    private _validate = () => {
//        let formData = this._viewModel.get("formData").toJSON();
//        let title = formData["title"];
//        let documentTypeId = formData["documentTypeId"];
//        let fileSelected = this._viewModel.get("metaData.fileSelected");
//        let needClear = this._viewModel.get("metaData.needClear");
//        this._viewModel.set("metaData.canSave", title && documentTypeId && fileSelected && !needClear);

//    };

//    //private _getEventId(): JQueryDeferred<string> {
//    //    if (this._options.onCreate) {
//    //        return this._options.onCreate(<ICreateEventData>this._viewModel.get("formData"));
//    //    }
//    //    let deffered = $.Deferred<string>();
//    //    //let eventTypeId = this._options.eventTypeId || UploadControl.DefaultEventType;
//    //    let title = this._viewModel.get("formData.title");
//    //    //EventSender.send(`Сохранение файла "${title}"`, eventTypeId, data => {
//    //    //    deffered.resolve(data);
//    //    //});
//    //    return deffered;
//    //}

//    public saveClick(successFunction, errorFunction?) {
//        this._fileOnLoaded = successFunction;
//        this._fileOnError = errorFunction;
//        let saveClick = this._viewModel.get("saveClick");
//        saveClick();
//    }

//    init() {
//        this._initData = {
//            documentTypeId: this._options.documentType && typeof this._options.documentType === "string"
//                ? (this._options.documentType || UploadControl.DefaultDocumentType)
//                : this._options.initData && this._options.initData.documentTypeId,
//            title: undefined,
//            number: undefined,
//            description: undefined,
//            selectedFile: undefined
//        };
//        this._viewModel = kendo.observable({
//            types: new kendo.data.DataSource({
//                data: this._options.documentType instanceof Array ? this._options.documentType : []
//            }),
//            formData: {},
//            metaData: {
//                displayDocumentTypes: (this._options.documentType instanceof Array) && this._options.documentType.length > 0,
//                displayTitle: !this._options.hideTitle,
//                displayNumber: this._options.displayNumber,
//                displayDescription: this._options.displayDescription,
//                displayButtons: this._options.displayButtons,
//                canSave: false,
//                canClear: true,
//                needClear: false,
//                fileSelected: false,
//                eventId: undefined,
//                cancelText: this._options.messages && this._options.messages.cancel || "Очистить",
//                saveText: this._options.messages && this._options.messages.save || "Отправить"
//            },
//            saveClick: () => {
//                //this._getEventId()
//                //    .done(eventId => {
//                //        this._viewModel.set("formData.eventId", eventId);
//                //        this.uploadControl.upload();
//                //    });

//            },
//            clearClick: () => {
//                this.uploadControl.clearAllFiles();
//                this._viewModel.set("formData", this._initData);
//                this._viewModel.set("metaData.fileSelected", false);
//                this._viewModel.set("metaData.needClear", false);
//                //if (this._options.event && this._options.event.onCancel) {
//                //    this._options.event.onCancel();
//                //}
//            },
//            upload: (e) => {
//                e.data = {
//                    title: this._viewModel.get("formData.title"),
//                    documentTypeId: this._viewModel.get("formData.documentTypeId"),
//                    applicationCode: this._options.applicationCode,
//                    description: this._viewModel.get("formData.description"),
//                    fullNumber: this._viewModel.get("formData.number"),
//                    //reasonEventId: this._viewModel.get("formData.eventId")
//                };
//            },
//            selectFile: (e) => {
//                if (!this._viewModel.get("formData.title")) {
//                    this._viewModel.set("formData.title", getFileName(e.files[0].name));
//                }
//                this._viewModel.set("formData.fileExtension", getFileExtension(e.files[0].extension));
//                this._viewModel.set("metaData.fileSelected", true);
//                //if (this._options.autoUpload === true) {
//                //    this._getEventId()
//                //        .done(eventId => {
//                //            this._viewModel.set("formData.eventId", eventId);
//                //            this.uploadControl.upload();
//                //        });
//                //}
//                if (this._options && this._options.event && this._options.event.onSelect) {
//                    this._options.event.onSelect({ name: e.files[0].name });
//                }
//            },
//            clearFile: () => {
//                this._viewModel.set("metaData.fileSelected", false);
//                if (this._options && this._options.event && this._options.event.onClear) {
//                    this._options.event.onClear();
//                }
//            },
//            removeFile: (e) => {
//                this._viewModel.set("metaData.fileSelected", false);
//                this.uploadControl.clearAllFiles();
//                if (this._options && this._options.event && this._options.event.onClear) {
//                    this._options.event.onClear();
//                }
//                e.preventDefault();
//            },
//            successFile: (e) => {
//                this._viewModel.set("formData.fileId", e.response);
//                this._viewModel.set("metaData.needClear", true);
//                let data = this._viewModel.get("formData").toJSON();
//                if (this._options.event && this._options.event.onSuccess) {
//                    this._options.event.onSuccess(data);
//                }
//                if (this._fileOnLoaded) {
//                    this._fileOnLoaded(data);
//                    this._fileOnLoaded = null;
//                }
//            },
//            errorFile: (e) => {
//                this._viewModel.set("formData.fileId", JSON.parse(e.XMLHttpRequest.response));
//                this._viewModel.set("metaData.needClear", true);
//                if (this._options.event && this._options.event.onError) {
//                    let data = this._viewModel.get("formData").toJSON();
//                    this._options.event.onError(data);
//                }
//                if (this._fileOnError) {
//                    this._fileOnError(e);
//                    this._fileOnError = null;
//                }
//            },
//            files: []


//        });
//        this._viewModel.set("formData", this._initData);
//        kendo.bind(this.container, this._viewModel);

//        this._viewModel.bind("change", () => {
//            this._validate();
//        });

//    }

//}

//export { UploadControl }