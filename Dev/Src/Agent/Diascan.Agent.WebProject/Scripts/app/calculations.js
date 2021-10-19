define(["require", "exports", "./shared", "./modules/layout/multiView", "./modules/layout/splitter", "./modules/navigation/equipmentTree", "./modules/grid/calculationsGrid", "./navigator", "./modules/data/urls", "./shared", "./modules/editors/calculationEditor", "./modules/data/urls"], function (require, exports, shared_1, multiView_1, splitter_1, equipmentTree_1, calculationsGrid_1, navigator_1, urls_1, Shared, calculationEditor_1, Urls) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var parseNullableString = Shared.parseNullableString;
    //import Data = kendo.data;
    //import Urls = require("./modules/data/urls");
    var currentPage = "calculations";
    var multiView = new multiView_1.default($("#main"));
    multiView.init();
    var _calculationIds = [];
    shared_1.initLayout(currentPage, {
        windowResize: function (h) {
            multiView.container.height(h);
        }
    });
    shared_1.router.route("grid", function (arg) {
        var initData = {
            contractorId: parseNullableString(arg["cid"]),
            pipelineId: parseNullableString(arg["pid"]),
            routeId: parseNullableString(arg["rid"])
        };
        multiView.pushTo(function (c) {
            splitter = new splitter_1.HorizontalSplitter(c, {
                leftPane: {
                    collapsible: true,
                    resizable: true,
                    size: "20%",
                    create: function (sc) {
                        var toolBarPanel = new equipmentTree_1.EquipmentTreeToolBarPanel(sc, {
                            initData: initData,
                            needAll: true,
                            onSelect: function (e) {
                                shared_1.router.navigate("grid?cid=" + e.data.mappedData.contractorId +
                                    ("&pid=" + e.data.mappedData.pipelineId) +
                                    ("&rid=" + e.data.mappedData.routeId));
                            },
                            treeurl: urls_1.diagnosticsTreeUrl,
                            settings: null
                        });
                        toolBarPanel.init();
                        return toolBarPanel;
                    }
                },
                rightPane: {
                    resizable: true,
                    collapsible: false,
                    create: function () {
                        return null;
                    },
                    refresh: true
                }
            });
            splitter.init();
            return splitter;
        }, splitter_1.HorizontalSplitter);
        var splitter = multiView.current;
        splitter.setRight(function (c) {
            var grid = new calculationsGrid_1.CalculationsGrid(c, "CalculationsGrid", {
                contractorId: parseNullableString(arg["cid"]),
                pipelineId: parseNullableString(arg["pid"]),
                routeId: parseNullableString(arg["rid"]),
                event: {
                    mailingSettings: function (e) {
                        _calculationIds = e.data._calculationIds;
                        shared_1.router.navigate("exportSettings?" + navigator_1.makeUrlParametersPart(arg));
                    },
                    //mailing: (e) => {
                    //    new CalculationEmailing(e.data, () => { grid.refresh(false); });
                    //    grid.refresh();
                    //},
                    remove: function (e) {
                        new calculationEditor_1.CalculationRemover(e.data, function () { grid.refresh(false); });
                        grid.refresh();
                    },
                    download: function (e) {
                        window.location.href = Urls.createTableDatasetUrl + "?id=" + e.data.Id;
                    }
                }
            });
            grid.init();
            grid.refresh();
            return grid;
        });
        $(window).trigger('resize');
    });
    shared_1.router.route("exportSettings", function (arg) {
        var controlId = "exportSettings";
        multiView.pushTo(function (c) {
            var editor = new calculationEditor_1.CalculationEditor(c, controlId, null, {
                contractorId: parseNullableString(arg["cid"]),
                pipelineId: parseNullableString(arg["pid"]),
                routeId: parseNullableString(arg["rid"]),
                calculationIds: _calculationIds,
                viewObject: function (equipmentId) {
                    //router.navigate(`new/objects/view/${equipmentId}?${makeUrlParametersPart(arg)}`);
                },
                event: {
                    save: function () {
                        //let id = e.data.id;
                        shared_1.router.navigate("grid?" + navigator_1.makeUrlParametersPart(arg));
                        //router.navigate(`view/${id}?${makeUrlParametersPart(arg)}`);
                    },
                    cancel: function () {
                        shared_1.router.navigate("grid?" + navigator_1.makeUrlParametersPart(arg));
                    }
                }
            });
            editor.init();
            return editor;
        }, calculationEditor_1.CalculationEditor, controlId);
    });
    shared_1.router.start();
});
//# sourceMappingURL=calculations.js.map