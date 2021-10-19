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
define(["require", "exports", "../baseControl"], function (require, exports, baseControl_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.BaseTree = void 0;
    var BaseTree = /** @class */ (function (_super) {
        __extends(BaseTree, _super);
        function BaseTree(container) {
            var _this = _super.call(this, container) || this;
            _this.autoBind = true;
            return _this;
        }
        Object.defineProperty(BaseTree.prototype, "tree", {
            get: function () {
                return this.container.data("kendoTreeView");
            },
            enumerable: false,
            configurable: true
        });
        BaseTree.prototype.init = function () {
            var _this = this;
            $(this.container).kendoTreeView({
                dataSource: this.dataSource,
                dataTextField: this.textField,
                select: function (e) {
                    _this._selectedNode = e.node;
                    _this.notify("select", {
                        sender: e.sender, node: e.node,
                        data: e.sender.dataItem(e.node),
                        path: _this.selectedPath,
                        pathData: _this.selectedPathData,
                        mappedData: _this.mapPath ? _this.mapPath(_this.selectedPathData) : {}
                    });
                },
                autoBind: this.autoBind
            });
        };
        Object.defineProperty(BaseTree.prototype, "selectedPath", {
            get: function () {
                var path = [];
                var item = this.tree.dataItem(this._selectedNode);
                while (item && item.id) {
                    path.unshift(item.id);
                    item = item.parentNode();
                }
                return path;
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseTree.prototype, "selectedPathText", {
            get: function () {
                var path = [];
                var item = this.tree.dataItem(this._selectedNode);
                while (item && item.id) {
                    var text = this.tree.text(this.tree.findByUid(item.uid));
                    path.unshift(text);
                    item = item.parentNode();
                }
                return path;
            },
            enumerable: false,
            configurable: true
        });
        Object.defineProperty(BaseTree.prototype, "selectedPathData", {
            get: function () {
                var path = {};
                var item = this.tree.dataItem(this._selectedNode);
                while (item && item.id) {
                    var t = item["Type"];
                    if (t) {
                        if (path[t]) {
                            path[t] = [path[t]];
                            path[t].push(item);
                        }
                        else {
                            path[t] = item;
                        }
                    }
                    item = item.parentNode();
                }
                return path;
            },
            enumerable: false,
            configurable: true
        });
        BaseTree.prototype._selectItem = function (id) {
            var item = this.dataSource.get(id);
            if (item) {
                var node = this.tree.findByUid(item.uid);
                this.tree.select(node);
                this._selectedNode = node.get(0);
            }
        };
        BaseTree.prototype.selectPath = function (path) {
            var _this = this;
            if (path && path.length > 0) {
                var last_1 = path.pop();
                if (path.length > 0) {
                    this.tree.expandPath(path, function () {
                        _this._selectItem(last_1);
                    });
                }
                else {
                    this._selectItem(last_1);
                }
            }
        };
        return BaseTree;
    }(baseControl_1.default));
    exports.BaseTree = BaseTree;
    exports.default = BaseTree;
});
//# sourceMappingURL=tree.js.map