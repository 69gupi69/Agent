import BaseTree from "./tree";
import BaseControl from "../baseControl";
import ToolBarPanel from "../layout/toolBarPanel";

enum EnEquipmentTreeItemType {
    Top = 0,
    All,
    Contractor,
    Pipeline,
    RegionalPipelineManagement,
    PumpStation,
    Route,
    EquipmentGroup,
    EquipmentType
}

interface IEquipmentTreeInitData {
    contractorId?: string,
    pipelineId?: string,
    routeId?: string
}

interface IEquipmentTreeConfig {
    needAll?: boolean,
    initData?: IEquipmentTreeInitData,
    treeurl?: string,
    onSelect: (e: any) => void;
    settings?: {}
}

class EquipmentTree extends BaseTree {
    private readonly _needAll: boolean;
    private readonly _initData: IEquipmentTreeInitData;
    private readonly _treeurl: string;
    private _settings: {};
    private _isArea: boolean;

    dispose() {
    }

    private _dataSource: kendo.data.HierarchicalDataSource = null;
    protected get dataSource(): kendo.data.HierarchicalDataSource {
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
                    parameterMap: (data, type) => {
                        if (type === "read") {
                            let result = {};
                            if (data && data["Id"]) {
                                let item = this._dataSource.get(data["Id"]);
                                if (item) {
                                    result["id"] = item["Id"];
                                    result["type"] = item["Type"];
                                    result["contractorId"] = item["ContractorId"];                                    
                                }
                            } else {
                                result["type"] = this._needAll ? EnEquipmentTreeItemType.Top : EnEquipmentTreeItemType.All;
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
                    parse: (response) => {  
                                return response;                           
                    }
                }
            });
        }
        return this._dataSource;

    }

    protected get textField(): string {
        return "Name";
    }

    private _extractId = (item: any) => {
        return item ? item.id : null;
    };
    private _extractListId = (item: any) => {
        let listId = [];
        let level = 0;
        while (level < item.length) {
            listId[level] = item[level].id;
            level++;
        }
        return listId.toString();
    };

    private _formatPmpsid(pmpsid) {
        if (pmpsid) {
            if (pmpsid.id) {
                return this._extractId(pmpsid);
            }
            else
                return this._extractListId(pmpsid);
        }
        else
            return null
    }

    protected mapPath = (pathData) => {
        return {
            contractorId: this._extractId(pathData[EnEquipmentTreeItemType.Contractor]),
            pipelineId: this._extractId(pathData[EnEquipmentTreeItemType.Pipeline]),
            routeId: this._extractId(pathData[EnEquipmentTreeItemType.Route])
        };
    };

    constructor(container: JQuery, config?: IEquipmentTreeConfig) {
        super(container);
        this._needAll = config && config.needAll;
        this._initData = config && config.initData;
        this._treeurl = config.treeurl;
        this._settings = config.settings;
        this.autoBind = false;
    }

    private static _makePath(data: IEquipmentTreeInitData, needAll?: boolean): string[] {
        let result: string[] = [];
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
            } else {
                return result;
            }       
        } else {
            return result;
        }
        return result;
    }


    public init() {
        super.init();
        this.dataSource.read().then(() => {
            if (this._initData && this._initData.contractorId) {
                let path = EquipmentTree._makePath(this._initData, this._needAll);
                this.selectPath(path);

            }
        });

    }
}

interface IEquipmentTreeToolbarConfig extends IEquipmentTreeConfig {
    onSelect: (e: any) => void;
}

class EquipmentTreeToolBarPanel extends BaseControl {

    private _toolBarPanel: ToolBarPanel;

    protected get tree(): EquipmentTree {
        return <EquipmentTree>this._toolBarPanel.contentControl;
    }

    constructor(container: JQuery, config: IEquipmentTreeToolbarConfig) {
        super(container);       
        this._toolBarPanel = new ToolBarPanel(container, {
            createContent: tbc => {
                let tree: EquipmentTree;
                tree = new EquipmentTree(tbc, config);
                tree.init();
                if (config.onSelect) {
                    tree.subscribe("select", config.onSelect);
                }
                return tree;
            }
        });
    }

    dispose() {
        this._toolBarPanel.dispose();
    }

    init() {
        this._toolBarPanel.init();
    }

}

export { EquipmentTree, EquipmentTreeToolBarPanel }
export default EquipmentTree