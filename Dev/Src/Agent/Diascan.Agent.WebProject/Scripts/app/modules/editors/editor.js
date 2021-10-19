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
define(["require", "exports", "../baseControl", "../layout/confirmWindow", "../layout/notificator", "../utils/container", "../layout/waitModal"], function (require, exports, baseControl_1, confirmWindow_1, notificator_1, container_1, waitModal_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EditorMode = exports.BaseEmailing = exports.BaseRemover = exports.BaseEditor = void 0;
    //import { StaffLog } from "../utils/staffLog";
    //import { LogAction } from "../../enums/logAction";
    var EditorMode;
    (function (EditorMode) {
        EditorMode[EditorMode["Add"] = 0] = "Add";
        EditorMode[EditorMode["Edit"] = 1] = "Edit";
        EditorMode[EditorMode["View"] = 2] = "View";
    })(EditorMode || (EditorMode = {}));
    exports.EditorMode = EditorMode;
    var BaseEditor = /** @class */ (function (_super) {
        __extends(BaseEditor, _super);
        function BaseEditor(container, id, dataId, config) {
            var _this = _super.call(this, container, id) || this;
            _this.resizeEvent = function () {
                _this.resize();
            };
            _this.messages = {
                putSuccess: "Запись изменена успешно",
                postSuccess: "Запись добавлена успешно",
                cancelTitle: "Отмена изменений",
                cancelMessage: "По объекту \"{0}\" есть несохраненные изменения. Отменить?",
            };
            _this._viewModel = null;
            _this._config = config;
            //this._mode = dataId ? EditorMode.Edit : EditorMode.Add;
            //this._dataId = dataId;
            _this.replaceContainer(_this.getContent());
            var customRule = _this.getCustomRule();
            return _this;
            // this.validator = new Validator(this.container, { customRule: customRule });
        }
        Object.defineProperty(BaseEditor.prototype, "viewModel", {
            //private readonly _mode: EditorMode;
            //private readonly _dataId: string | number;
            //protected get mode(): EditorMode {
            //    return this._mode;
            //}
            //protected get dataId(): string | number {
            //    return this._dataId
            //}
            get: function () {
                return this._viewModel;
            },
            enumerable: false,
            configurable: true
        });
        BaseEditor.prototype.resize = function () {
            var parentHeight = this.container.parent().height();
            var panelHeaderHeight = this.container.find("div.panel-heading").height();
            var panelFooterHeight = this.container.find("div.panel-footer").height();
            var k = panelHeaderHeight + panelFooterHeight + 16;
            this.container.find("div.panel-body").css("max-height", parentHeight - k + "px");
        };
        BaseEditor.prototype.clickSave = function () {
            var _this = this;
            //if (this.validator.validate()) {
            waitModal_1.default.show("Данные сохраняются");
            // StaffLog.Action(LogAction.SaveButtonClickedInTheInterface.toString() + `"${this.title}"`,
            //    { currentUrl: window.location.href });
            var data = this.formData;
            //if (this._mode === EditorMode.Edit) {
            this.putData(this.getId(this.initData), data, function () {
                waitModal_1.default.hide();
                _this.notify("save");
                notificator_1.globalNotifier.success(_this.messages.putSuccess);
                //StaffLog.Action(LogAction.UserChangedObjectByType.toString() +
                //    `"${data.Type}" с id: "${data.id}"`, { currentUrl: window.location.href });
            }, function (message) {
                waitModal_1.default.hide();
                notificator_1.globalNotifier.error(message);
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
        };
        BaseEditor.prototype.clickCancel = function () {
            var _this = this;
            //StaffLog.Action(LogAction.CancelButtonClickedInTheInterface.toString() +
            //    `"${this.title}"`, { currentUrl: window.location.href });
            if (this.viewModel.get("saveEnabled")) {
                confirmWindow_1.globalConfirm.show(this.messages.cancelTitle, this.messages.cancelMessage.replace("{0}", this.getName(this.initData)), function () {
                    _this.notify("cancel");
                });
            }
            else {
                this.notify("cancel");
            }
        };
        BaseEditor.prototype.enabledSaveButton = function (enabled) {
            //if (this.validator.validate()) {
            this.viewModel.set("saveEnabled", enabled);
            //}
        };
        Object.defineProperty(BaseEditor.prototype, "title", {
            get: function () {
                return this._viewModel.get("title");
            },
            set: function (value) {
                this._viewModel.set("title", value);
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseEditor.prototype, "initData", {
            //protected validator: Validator;
            get: function () {
                return this.viewModel.get("initData").toJSON();
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseEditor.prototype, "datasources", {
            get: function () {
                return this.viewModel.get("datasources");
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseEditor.prototype, "formData", {
            get: function () {
                return this.viewModel.get("formData").toJSON();
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseEditor.prototype, "metaData", {
            get: function () {
                return this.viewModel.get("metaData").toJSON();
            },
            enumerable: false,
            configurable: true
        });
        BaseEditor.prototype.initField = function (field, data) {
            this._viewModel.set("initData." + field, data);
            this._viewModel.set("formData." + field, data);
        };
        BaseEditor.prototype.setField = function (field, data) {
            this._viewModel.set("formData." + field, data);
        };
        BaseEditor.prototype.setMetaData = function () {
            var args = [];
            for (var _i = 0; _i < arguments.length; _i++) {
                args[_i] = arguments[_i];
            }
            if (args.length === 2) {
                var field = args[0];
                var data = args[1];
                this._viewModel.set("metaData." + field, data);
                return;
            }
            if (args.length === 1) {
                var data = args[0];
                if (data) {
                    for (var k in data) {
                        this.setMetaData(k, data[k]);
                    }
                }
                return;
            }
        };
        BaseEditor.prototype.setDataSource = function (id, dataSource) {
            this._viewModel["datasources"][id] = dataSource;
            dataSource.read();
        };
        BaseEditor.prototype.getListeners = function () {
            return {};
        };
        BaseEditor.prototype.getMetaData = function () {
            return {};
        };
        BaseEditor.prototype.getCustomRule = function () {
            return null;
        };
        Object.defineProperty(BaseEditor.prototype, "config", {
            get: function () {
                return this._config;
            },
            enumerable: false,
            configurable: true
        });
        BaseEditor.prototype.dispose = function () {
            container_1.DefaultResizer.unsubscribe("resize", this.resizeEvent);
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
        };
        BaseEditor.prototype.init = function () {
            var _this = this;
            container_1.DefaultResizer.subscribe("resize", this.resizeEvent);
            this.resize();
            this._viewModel = kendo.observable({
                initData: {},
                formData: {},
                metaData: this.getMetaData(),
                title: "",
                datasources: this.getDatasources(),
                listeners: this.getListeners(),
                saveEnabled: false,
                clickSave: function () { return _this.clickSave(); },
                clickCancel: function () { return _this.clickCancel(); }
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
            this._viewModel.bind("change", function (e) {
                if (e.field.indexOf("formData.") === 0) {
                    var se = JSON.stringify(_this._viewModel.get("initData")) !== JSON.stringify(_this._viewModel.get("formData"));
                    //se = se && this.validator.validate();
                    _this._viewModel.set("saveEnabled", se);
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
        };
        return BaseEditor;
    }(baseControl_1.default));
    exports.BaseEditor = BaseEditor;
    var BaseRemover = /** @class */ (function () {
        function BaseRemover(data, success, error) {
            this.messages = {
                success: "Данные удалены успешно",
                title: "Удаление записи",
                message: "Удалить запись {0}?"
            };
            this._data = data;
            this._success = success;
            this._error = error;
            this._remove();
        }
        BaseRemover.prototype._remove = function () {
            var _this = this;
            confirmWindow_1.globalConfirm.show(this.messages.title, this.messages.message.replace("{0}", this.getName(this._data)), function () {
                _this.deleteData(_this.getId(_this._data), function () {
                    notificator_1.globalNotifier.success(_this.messages.success);
                    //StaffLog.Action(LogAction.UserDeleteObjectByType.toString() + `с id: "${this._data.id}" `, { currentUrl: window.location.href });
                    if (_this._success) {
                        _this._success();
                    }
                }, function (message) {
                    notificator_1.globalNotifier.error(message);
                    if (_this._error) {
                        _this._error(message);
                    }
                });
            });
        };
        return BaseRemover;
    }());
    exports.BaseRemover = BaseRemover;
    var BaseEmailing = /** @class */ (function () {
        function BaseEmailing(data, success, error) {
            this.messages = {
                success: "Отчет отправлен успешно",
                title: "Отправка Excel отчета",
                message: "Отправить Excel отчет {0}, всем пользователям ИС \"Агент\"?"
            };
            this._data = data;
            this._success = success;
            this._error = error;
            this._remove();
        }
        BaseEmailing.prototype._remove = function () {
            var _this = this;
            confirmWindow_1.globalConfirm.show(this.messages.title, this.messages.message.replace("{0}", this.getName(this._data)), function () {
                _this.deleteData(_this.getId(_this._data), function () {
                    notificator_1.globalNotifier.success(_this.messages.success);
                    //StaffLog.Action(LogAction.UserDeleteObjectByType.toString() + `с id: "${this._data.id}" `, { currentUrl: window.location.href });
                    if (_this._success) {
                        _this._success();
                    }
                }, function (message) {
                    notificator_1.globalNotifier.error(message);
                    if (_this._error) {
                        _this._error(message);
                    }
                });
            });
        };
        return BaseEmailing;
    }());
    exports.BaseEmailing = BaseEmailing;
});
//# sourceMappingURL=editor.js.map