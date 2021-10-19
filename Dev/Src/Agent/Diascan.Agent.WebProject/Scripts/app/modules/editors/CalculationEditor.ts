import { BaseEditor, IEditorConfig } from "./editor";
import {
    SelectiveMailingExcelFileUrl,
    deleteJsontUrl,
    mailingExcelFileUrl,
    contractorsApiUrl
} from "../data/urls";
import {
    UsersGrid
} from "../grid/usersGrid";
import { EditorMode, BaseRemover, BaseEmailing } from "./editor";
import GridHelpers = require("../utils/gridHelpers");
import GridCommandHelper = GridHelpers.GridCommandHelper;
import { allUserIdsClient } from "../data/clients";
import { globalNotifier } from "../layout/notificator";

enum DiagnosticType {
    RouteDiagnostic = 0,
    PmpsDiagnostic = 1
}

const mainContent = `
    <div data-app-role="objectlist">
        <div class="demo-section k-content wide">
        </div>
    </div>
    <div data-app-role="splitter"></div>`;
const footer = `<div class="panel-footer">
                        <button class="k-button k-button-icontext" data-bind="enabled:saveEnabled,events:{click:clickSave}"><span class="k-icon k-i-save "></span>${"Отправить"}</button>
                        <button class="k-button k-button-icontext" data-bind="events:{click:clickCancel}"><span class="k-icon k-i-cancel"></span>${"Отмена"}</button>
                </div>`;
const content = `
<div class="panel panel-default" style="height: 100%;">
<div class="panel-heading"><span data-bind="text:title"></span></div>
<div class="panel-body">
    ${mainContent}
</div>
<div>${footer}</div>
</div>`;

interface ICalculationEditorConfig extends IEditorConfig {
    contractorId?: string,
    pipelineId?: string,
    routeId?: string,
    calculationIds?: string[];
    viewObject?: (equipmentId?) => void;
    userId?: string;
}

class CalculationEditor extends BaseEditor {
    private _selectedIds: string[];
    private _userGrid: UsersGrid;
    private _calculationIds: string[];

    protected createDiagnosticGrid = () => {
        let that = this;
        return new UsersGrid(this.container.find("div[data-app-role=objectlist]"),
            "UsersGrid",
            ({
                selectedIds: that._selectedIds,
                event: {
                    dataBound: (e) => {
                        $(".headCheckbox").bind("click",
                            function(e) {
                                let val = $(e.target).prop('checked');
                                e.stopPropagation();
                                $(e.target).closest("tr").toggleClass("k-state-selected");
                                $('tr').find('[type=checkbox]').prop('checked', val);
                                if (val) {
                                    allUserIdsClient.getMany((data) => {
                                    }).done((data) => {
                                        for (var i = 0; i < data.length; i++) {
                                            that._selectedIds = [];
                                            that._selectedIds.push(data[i].toString());
                                        }
                                        if (that._selectedIds.length > 0) {
                                            that.enabledSaveButton(true);
                                        }
                                    });
                                } else {
                                    that._selectedIds = [];
                                    that.enabledSaveButton(false);
                                }
                            });
                        $(".checkbox").bind("click",
                            function(e) {
                                let val = $(e.target).prop('checked');
                                e.stopPropagation();
                                $(e.target).closest("tr").toggleClass("k-state-selected");
                                let row = $("div[data-app-role=objectlist]").data("kendoGrid")
                                    .dataItem($(e.target).closest("tr"));
                                if (val) {
                                    that._selectedIds.push(row["Id"]);
                                } else {
                                    GridCommandHelper.removeA(that._selectedIds, row["Id"]);
                                }
                                if (that._selectedIds.length > 0) {
                                    that.enabledSaveButton(true);
                                } else {
                                    that.enabledSaveButton(false);
                                }
                            });
                        for (let i = 0; i < e.sender._data.length; i++) {
                            if (that._selectedIds.indexOf(e.sender._data[i].Id) !== -1) {
                                let ch = e.sender.tbody.find("[data-uid=" + e.sender._data[i].uid + "]").closest("tr")
                                    .toggleClass("k-state-selected").prevObject.prevObject["0"].children[i];
                                ch.childNodes["0"].children["0"].checked = true;
                            }
                        }
                        if (that._selectedIds.length > 0) {
                            that.enabledSaveButton(true);
                        } else {
                            that.enabledSaveButton(false);
                        }
                    }
                }
            }) as any);
    };

    protected clickSave() {
        globalNotifier.success("Рассылка расчётов!");
        let that = this; 
        $.ajax(
            {
                url: SelectiveMailingExcelFileUrl,
                crossDomain: true,
                xhrFields: { withCredentials: true },
                type: "GET",
                cache: false,
                data: {
                    UserIds: that._selectedIds,
                    CalculationIds: that._calculationIds
                },
                success: function () {
                    that.notify("save");
                }
            }
        );
    }

    constructor(container: JQuery, id?: string, data?: string, config?: ICalculationEditorConfig) {
        super(container, id, data, config);
        this._calculationIds = config ? config.calculationIds : [];
        this._selectedIds = [];
    }

    public init() {
        super.init();
        this.title = "Отправка отчетов пользователям ИС \"Агент\"";
        this.messages.cancelMessage = "Выйти из меню отправки отчетов?";
        this._userGrid = this.createDiagnosticGrid();
        this._userGrid.init();
        this._userGrid.refresh();
        this.resize();
    }

    protected getId(data) {
        return data.Id;
    }

    protected getName(data) {
        return data.Name;
    }

    protected getContent() {
        return content;
    }

    protected ajaxSetField(url, field, column) {
        $.ajax({
            url: url,
            cache: false,
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: (data) => {
                this.setField(field, data[column]);
            }
        });
    }

    protected initFormData() {
        let getCustomerNameUrl = contractorsApiUrl + "/" + (<ICalculationEditorConfig>this.config).contractorId;
        this.ajaxSetField(getCustomerNameUrl, "Customer", "ShortName");
    }

    protected initMetaData() {
    }

    protected getDatasources(): { [key: string]: kendo.data.DataSource; } {
        return {
        }
    }

    protected getData(id: string, success: (data: any) => void, error: (message: any, jqXHR: JQueryXHR) => void): void {
        let jsonString: string, jsonData: any;
    }

    protected putData(diagnsoticId: string,
        data: any,
        success: () => void,
        error: (message: any, jqXHR: JQueryXHR) => void): void {
    }

    protected postData(data: any,
        success: (diagnsoticId?: string) => void,
        error: (message: any, jqXHR: JQueryXHR) => void): void {
    }

    dispose() {
        super.dispose();
    }

    protected resize() {
        super.resize();
    }
}


class CalculationEmailing extends BaseEmailing {
    constructor(data: any, success?: () => void, error?: (message) => void) {
        super(data, success, error);
    }

    protected getId(data) {
        return data.Id;
    }

    protected getName(data) {
        return data.Name;
    }

    protected deleteData(id: string | number,
        success: () => void,
        error: (message: any, jqXHR: JQueryXHR) => void): void {
        globalNotifier.success("Рассылка отчетов!");
        $.ajax(
            {
                url: mailingExcelFileUrl + "?id=" + id,
                dataType: 'json',
                crossDomain: true,
                xhrFields: { withCredentials: true },
                type: "GET",
                cache: false,
                success: success
            }
        );
    }
}

class CalculationRemover extends BaseRemover {
    constructor(data: any, success?: () => void, error?: (message) => void) {
        super(data, success, error);
    }

    protected getId(data) {
        return data.Id;
    }

    protected getName(data) {
        return data.Name;
    }

    protected deleteData(id: string | number,
        success: () => void,
        error: (message: any, jqXHR: JQueryXHR) => void): void {
        $.ajax(
            {
                url: deleteJsontUrl + "?id=" + id,
                dataType: 'json',
                crossDomain: true,
                xhrFields: { withCredentials: true },
                type: "GET",
                cache: false,
                success: success
            }
        );
    }
}

export { CalculationEditor, CalculationRemover, CalculationEmailing  };
