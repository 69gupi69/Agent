var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "./editor", "../data/urls", "../grid/usersGrid", "./editor", "../utils/gridHelpers", "../data/clients", "../layout/notificator"], function (require, exports, editor_1, urls_1, usersGrid_1, editor_2, GridHelpers, clients_1, notificator_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.CalculationEmailing = exports.CalculationRemover = exports.CalculationEditor = void 0;
    var GridCommandHelper = GridHelpers.GridCommandHelper;
    var DiagnosticType;
    (function (DiagnosticType) {
        DiagnosticType[DiagnosticType["RouteDiagnostic"] = 0] = "RouteDiagnostic";
        DiagnosticType[DiagnosticType["PmpsDiagnostic"] = 1] = "PmpsDiagnostic";
    })(DiagnosticType || (DiagnosticType = {}));
    var mainContent = "\n    <div data-app-role=\"objectlist\">\n        <div class=\"demo-section k-content wide\">\n        </div>\n    </div>\n    <div data-app-role=\"splitter\"></div>";
    var footer = "<div class=\"panel-footer\">\n                        <button class=\"k-button k-button-icontext\" data-bind=\"enabled:saveEnabled,events:{click:clickSave}\"><span class=\"k-icon k-i-save \"></span>" + "Отправить" + "</button>\n                        <button class=\"k-button k-button-icontext\" data-bind=\"events:{click:clickCancel}\"><span class=\"k-icon k-i-cancel\"></span>" + "Отмена" + "</button>\n                </div>";
    var content = "\n<div class=\"panel panel-default\" style=\"height: 100%;\">\n<div class=\"panel-heading\"><span data-bind=\"text:title\"></span></div>\n<div class=\"panel-body\">\n    " + mainContent + "\n</div>\n<div>" + footer + "</div>\n</div>";
    var CalculationEditor = /** @class */ (function (_super) {
        __extends(CalculationEditor, _super);
        function CalculationEditor(container, id, data, config) {
            var _this = _super.call(this, container, id, data, config) || this;
            _this.createDiagnosticGrid = function () {
                var that = _this;
                return new usersGrid_1.UsersGrid(_this.container.find("div[data-app-role=objectlist]"), "UsersGrid", ({
                    selectedIds: that._selectedIds,
                    event: {
                        dataBound: function (e) {
                            $(".headCheckbox").bind("click", function (e) {
                                var val = $(e.target).prop('checked');
                                e.stopPropagation();
                                $(e.target).closest("tr").toggleClass("k-state-selected");
                                $('tr').find('[type=checkbox]').prop('checked', val);
                                if (val) {
                                    clients_1.allUserIdsClient.getMany(function (data) {
                                    }).done(function (data) {
                                        for (var i = 0; i < data.length; i++) {
                                            that._selectedIds = [];
                                            that._selectedIds.push(data[i].toString());
                                        }
                                        if (that._selectedIds.length > 0) {
                                            that.enabledSaveButton(true);
                                        }
                                    });
                                }
                                else {
                                    that._selectedIds = [];
                                    that.enabledSaveButton(false);
                                }
                            });
                            $(".checkbox").bind("click", function (e) {
                                var val = $(e.target).prop('checked');
                                e.stopPropagation();
                                $(e.target).closest("tr").toggleClass("k-state-selected");
                                var row = $("div[data-app-role=objectlist]").data("kendoGrid")
                                    .dataItem($(e.target).closest("tr"));
                                if (val) {
                                    that._selectedIds.push(row["Id"]);
                                }
                                else {
                                    GridCommandHelper.removeA(that._selectedIds, row["Id"]);
                                }
                                if (that._selectedIds.length > 0) {
                                    that.enabledSaveButton(true);
                                }
                                else {
                                    that.enabledSaveButton(false);
                                }
                            });
                            for (var i = 0; i < e.sender._data.length; i++) {
                                if (that._selectedIds.indexOf(e.sender._data[i].Id) !== -1) {
                                    var ch = e.sender.tbody.find("[data-uid=" + e.sender._data[i].uid + "]").closest("tr")
                                        .toggleClass("k-state-selected").prevObject.prevObject["0"].children[i];
                                    ch.childNodes["0"].children["0"].checked = true;
                                }
                            }
                            if (that._selectedIds.length > 0) {
                                that.enabledSaveButton(true);
                            }
                            else {
                                that.enabledSaveButton(false);
                            }
                        }
                    }
                }));
            };
            _this._calculationIds = config ? config.calculationIds : [];
            _this._selectedIds = [];
            return _this;
        }
        CalculationEditor.prototype.clickSave = function () {
            notificator_1.globalNotifier.success("Рассылка расчётов!");
            var that = this;
            $.ajax({
                url: urls_1.SelectiveMailingExcelFileUrl,
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
            });
        };
        CalculationEditor.prototype.init = function () {
            _super.prototype.init.call(this);
            this.title = "Отправка отчетов пользователям ИС \"Агент\"";
            this.messages.cancelMessage = "Выйти из меню отправки отчетов?";
            this._userGrid = this.createDiagnosticGrid();
            this._userGrid.init();
            this._userGrid.refresh();
            this.resize();
        };
        CalculationEditor.prototype.getId = function (data) {
            return data.Id;
        };
        CalculationEditor.prototype.getName = function (data) {
            return data.Name;
        };
        CalculationEditor.prototype.getContent = function () {
            return content;
        };
        CalculationEditor.prototype.ajaxSetField = function (url, field, column) {
            var _this = this;
            $.ajax({
                url: url,
                cache: false,
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function (data) {
                    _this.setField(field, data[column]);
                }
            });
        };
        CalculationEditor.prototype.initFormData = function () {
            var getCustomerNameUrl = urls_1.contractorsApiUrl + "/" + this.config.contractorId;
            this.ajaxSetField(getCustomerNameUrl, "Customer", "ShortName");
        };
        CalculationEditor.prototype.initMetaData = function () {
        };
        CalculationEditor.prototype.getDatasources = function () {
            return {};
        };
        CalculationEditor.prototype.getData = function (id, success, error) {
            var jsonString, jsonData;
        };
        CalculationEditor.prototype.putData = function (diagnsoticId, data, success, error) {
        };
        CalculationEditor.prototype.postData = function (data, success, error) {
        };
        CalculationEditor.prototype.dispose = function () {
            _super.prototype.dispose.call(this);
        };
        CalculationEditor.prototype.resize = function () {
            _super.prototype.resize.call(this);
        };
        return CalculationEditor;
    }(editor_1.BaseEditor));
    exports.CalculationEditor = CalculationEditor;
    var CalculationEmailing = /** @class */ (function (_super) {
        __extends(CalculationEmailing, _super);
        function CalculationEmailing(data, success, error) {
            return _super.call(this, data, success, error) || this;
        }
        CalculationEmailing.prototype.getId = function (data) {
            return data.Id;
        };
        CalculationEmailing.prototype.getName = function (data) {
            return data.Name;
        };
        CalculationEmailing.prototype.deleteData = function (id, success, error) {
            notificator_1.globalNotifier.success("Рассылка отчетов!");
            $.ajax({
                url: urls_1.mailingExcelFileUrl + "?id=" + id,
                dataType: 'json',
                crossDomain: true,
                xhrFields: { withCredentials: true },
                type: "GET",
                cache: false,
                success: success
            });
        };
        return CalculationEmailing;
    }(editor_2.BaseEmailing));
    exports.CalculationEmailing = CalculationEmailing;
    var CalculationRemover = /** @class */ (function (_super) {
        __extends(CalculationRemover, _super);
        function CalculationRemover(data, success, error) {
            return _super.call(this, data, success, error) || this;
        }
        CalculationRemover.prototype.getId = function (data) {
            return data.Id;
        };
        CalculationRemover.prototype.getName = function (data) {
            return data.Name;
        };
        CalculationRemover.prototype.deleteData = function (id, success, error) {
            $.ajax({
                url: urls_1.deleteJsontUrl + "?id=" + id,
                dataType: 'json',
                crossDomain: true,
                xhrFields: { withCredentials: true },
                type: "GET",
                cache: false,
                success: success
            });
        };
        return CalculationRemover;
    }(editor_2.BaseRemover));
    exports.CalculationRemover = CalculationRemover;
});
//# sourceMappingURL=calculationEditor.js.map