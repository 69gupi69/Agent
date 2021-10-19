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
define(["require", "exports", "./tree", "../baseControl", "../layout/toolBarPanel"], function (require, exports, tree_1, baseControl_1, toolBarPanel_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EquipmentTreeToolBarPanel = exports.EquipmentTree = void 0;
    var EnEquipmentTreeItemType;
    (function (EnEquipmentTreeItemType) {
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["Top"] = 0] = "Top";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["All"] = 1] = "All";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["Contractor"] = 2] = "Contractor";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["Pipeline"] = 3] = "Pipeline";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["RegionalPipelineManagement"] = 4] = "RegionalPipelineManagement";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["PumpStation"] = 5] = "PumpStation";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["Route"] = 6] = "Route";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["EquipmentGroup"] = 7] = "EquipmentGroup";
        EnEquipmentTreeItemType[EnEquipmentTreeItemType["EquipmentType"] = 8] = "EquipmentType";
    })(EnEquipmentTreeItemType || (EnEquipmentTreeItemType = {}));
    var EquipmentTree = /** @class */ (function (_super) {
        __extends(EquipmentTree, _super);
        function EquipmentTree(container, config) {
            var _this = _super.call(this, container) || this;
            _this._dataSource = null;
            _this._extractId = function (item) {
                return item ? item.id : null;
            };
            _this._extractListId = function (item) {
                var listId = [];
                var level = 0;
                while (level < item.length) {
                    listId[level] = item[level].id;
                    level++;
                }
                return listId.toString();
            };
            _this.mapPath = function (pathData) {
                return {
                    contractorId: _this._extractId(pathData[EnEquipmentTreeItemType.Contractor]),
                    pipelineId: _this._extractId(pathData[EnEquipmentTreeItemType.Pipeline]),
                    routeId: _this._extractId(pathData[EnEquipmentTreeItemType.Route])
                };
            };
            _this._needAll = config && config.needAll;
            _this._initData = config && config.initData;
            _this._treeurl = config.treeurl;
            _this._settings = config.settings;
            _this.autoBind = false;
            return _this;
        }
        EquipmentTree.prototype.dispose = function () {
        };
        Object.defineProperty(EquipmentTree.prototype, "dataSource", {
            get: function () {
                var _this = this;
                if (!this._dataSource) {
                    this._dataSource = new kendo.data.HierarchicalDataSource({
                        transport: {
                            read: {
                                url: this._treeurl,
                                type: "GET",
                                crossDomain: true,
                                xhrFields: {
                                    withCredentials: true
                                },
                                cache: false
                            },
                            parameterMap: function (data, type) {
                                if (type === "read") {
                                    var result = {};
                                    if (data && data["Id"]) {
                                        var item = _this._dataSource.get(data["Id"]);
                                        if (item) {
                                            result["id"] = item["Id"];
                                            result["type"] = item["Type"];
                                            result["contractorId"] = item["ContractorId"];
                                        }
                                    }
                                    else {
                                        result["type"] = _this._needAll ? EnEquipmentTreeItemType.Top : EnEquipmentTreeItemType.All;
                                    }
                                    return result;
                                }
                            }
                        },
                        schema: {
                            model: {
                                id: "Id",
                                hasChildren: "HasChildren",
                                fields: {
                                    Name: {
                                        type: "string"
                                    },
                                    HasChildren: {
                                        type: "boolean"
                                    },
                                    Id: {
                                        type: "string"
                                    },
                                    Type: {
                                        type: "string"
                                    },
                                    ContractorId: {
                                        type: "string"
                                    }
                                }
                            },
                            parse: function (response) {
                                return response;
                            }
                        }
                    });
                }
                return this._dataSource;
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(EquipmentTree.prototype, "textField", {
            get: function () {
                return "Name";
            },
            enumerable: false,
            configurable: true
        });
        EquipmentTree.prototype._formatPmpsid = function (pmpsid) {
            if (pmpsid) {
                if (pmpsid.id) {
                    return this._extractId(pmpsid);
                }
                else
                    return this._extractListId(pmpsid);
            }
            else
                return null;
        };
        EquipmentTree._makePath = function (data, needAll) {
            var result = [];
            if (needAll) {
                result.push("all");
            }
            if (data.contractorId) {
                result.push(data.contractorId);
            }
            if (data.pipelineId) {
                result.push(data.pipelineId);
                if (data.routeId) {
                    result.push(data.routeId);
                }
                else {
                    return result;
                }
            }
            else {
                return result;
            }
            return result;
        };
        EquipmentTree.prototype.init = function () {
            var _this = this;
            _super.prototype.init.call(this);
            this.dataSource.read().then(function () {
                if (_this._initData && _this._initData.contractorId) {
                    var path = EquipmentTree._makePath(_this._initData, _this._needAll);
                    _this.selectPath(path);
                }
            });
        };
        return EquipmentTree;
    }(tree_1.default));
    exports.EquipmentTree = EquipmentTree;
    var EquipmentTreeToolBarPanel = /** @class */ (function (_super) {
        __extends(EquipmentTreeToolBarPanel, _super);
        function EquipmentTreeToolBarPanel(container, config) {
            var _this = _super.call(this, container) || this;
            _this._toolBarPanel = new toolBarPanel_1.default(container, {
                createContent: function (tbc) {
                    var tree;
                    tree = new EquipmentTree(tbc, config);
                    tree.init();
                    if (config.onSelect) {
                        tree.subscribe("select", config.onSelect);
                    }
                    return tree;
                }
            });
            return _this;
        }
        Object.defineProperty(EquipmentTreeToolBarPanel.prototype, "tree", {
            get: function () {
                return this._toolBarPanel.contentControl;
            },
            enumerable: false,
            configurable: true
        });
        EquipmentTreeToolBarPanel.prototype.dispose = function () {
            this._toolBarPanel.dispose();
        };
        EquipmentTreeToolBarPanel.prototype.init = function () {
            this._toolBarPanel.init();
        };
        return EquipmentTreeToolBarPanel;
    }(baseControl_1.default));
    exports.EquipmentTreeToolBarPanel = EquipmentTreeToolBarPanel;
    exports.default = EquipmentTree;
});
//# sourceMappingURL=equipmentTree.js.map