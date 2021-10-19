import BaseControl from "../baseControl";

abstract class BaseTree extends BaseControl {

    private _selectedNode: Element;

    protected constructor(container: JQuery) {
        super(container);
    }

    protected get tree(): kendo.ui.TreeView {
        return this.container.data("kendoTreeView") as kendo.ui.TreeView;
    }

    protected abstract get dataSource(): kendo.data.HierarchicalDataSource;

    protected abstract get textField(): string;

    protected mapPath: (pathData: { [nodeType: string]: {} }) => { [nodeType: string]: {} };
    protected autoBind = true;

    public init() {
        $(this.container).kendoTreeView({
            dataSource: this.dataSource,
            dataTextField: this.textField,
            select: e => {
                this._selectedNode = e.node;
                this.notify("select", {
                    sender: e.sender, node: e.node,
                    data: e.sender.dataItem(e.node),
                    path: this.selectedPath,
                    pathData: this.selectedPathData,
                    mappedData: this.mapPath ? this.mapPath(this.selectedPathData) : {}
                });
            },
            autoBind: this.autoBind
        });
    }

    get selectedPath(): string[] {
        let path = [];
        let item = this.tree.dataItem(this._selectedNode);
        while (item && item.id) {
            path.unshift(item.id);
            item = item.parentNode();
        }
        return path;
    }

    get selectedPathText(): string[] {
        let path = [];
        let item = this.tree.dataItem(this._selectedNode);
        while (item && item.id) {
            let text = this.tree.text(this.tree.findByUid(item.uid));
            path.unshift(text);
            item = item.parentNode();
        }
        return path;
    }

    get selectedPathData(): { [nodeType: string]: {} } {
        let path = {};
        let item = this.tree.dataItem(this._selectedNode);
        while (item && item.id) {
            let t = item["Type"];
            if (t) {
                if (path[t]) {
                    path[t] = [path[t]];
                    path[t].push(item);
                }
                else {
                    path[t] = item
                }
            }
            item = item.parentNode();
        }
        return path;
    }

    private _selectItem(id: string) {
        let item = this.dataSource.get(id);
        if (item) {
            let node = this.tree.findByUid(item.uid);
            this.tree.select(node);
            this._selectedNode = node.get(0);
        }
    }

    protected selectPath(path: string[]) {
        if (path && path.length > 0) {
            let last = path.pop();
            if (path.length > 0) {
                this.tree.expandPath(path, () => {
                    this._selectItem(last);
                });
            } else {
                this._selectItem(last);
            }
        }
    }
}

export { BaseTree }
export default BaseTree
