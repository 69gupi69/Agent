import { initLayout, router } from "./shared";
import MultiView from "./modules/layout/multiView";
import { HorizontalSplitter } from "./modules/layout/splitter";
import { EquipmentTreeToolBarPanel } from "./modules/navigation/equipmentTree";
import { CalculationsGrid } from "./modules/grid/calculationsGrid";
import { makeUrlParametersPart } from "./navigator";
import { ReportExport } from "./reportExport/reportExport";
import { diagnosticsTreeUrl, mailingExcelFileUrl } from "./modules/data/urls";
import Shared = require("./shared");
import parseNullableString = Shared.parseNullableString;
import { CalculationEditor, CalculationRemover, CalculationEmailing } from "./modules/editors/calculationEditor";
import Urls = require("./modules/data/urls");

const currentPage = "calculations";

let multiView = new MultiView($("#main"));
multiView.init();
let _calculationIds = [];


initLayout(currentPage, {
    windowResize: (h: number) => {
        multiView.container.height(h);
    }
});


router.route("calculationsGrid",
    (arg) => {
        let initData = {
            contractorId: parseNullableString(arg["cid"]),
            pipelineId: parseNullableString(arg["pid"]),
            routeId: parseNullableString(arg["rid"])
        };
        multiView.pushTo(c => {
            splitter = new HorizontalSplitter(c,
                {
                    leftPane: {
                        collapsible: true,
                        resizable: true,
                        size: "20%",
                        create: (sc: JQuery) => {
                            let toolBarPanel = new EquipmentTreeToolBarPanel(sc,
                                {
                                    initData: initData,
                                    needAll: true,
                                    onSelect: (e) => {
                                        router.navigate(`grid?cid=${e.data.mappedData.contractorId}` +
                                            `&pid=${e.data.mappedData.pipelineId}` +
                                            `&rid=${e.data.mappedData.routeId}`);
                                    },
                                    treeurl: diagnosticsTreeUrl,
                                    settings: null
                                });
                            toolBarPanel.init();
                            return toolBarPanel;
                        }
                    },
                    rightPane: {
                        resizable: true,
                        collapsible: false,
                        create: () => {
                            return null;
                        },
                        refresh: true
                    }
                });

            splitter.init();
            return splitter;
        },
            HorizontalSplitter);

        let splitter = <HorizontalSplitter>multiView.current;
        splitter.setRight(c => {
            let grid = new CalculationsGrid(c,
                "CalculationsGrid",
                {
                    contractorId: parseNullableString(arg["cid"]),
                    pipelineId: parseNullableString(arg["pid"]),
                    routeId: parseNullableString(arg["rid"]),

                    event: {
                        mailingSettings: (e) => {
                            _calculationIds = e.data._calculationIds;
                            router.navigate(`exportSettings?${makeUrlParametersPart(arg)}`);
                        },
                        //mailing: (e) => {
                        //    new CalculationEmailing(e.data, () => { grid.refresh(false); });
                        //    grid.refresh();

                        //},
                        remove: (e) => {
                            new CalculationRemover(e.data, () => { grid.refresh(false); });
                            grid.refresh();
                        },
                        download: (e) => {
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

router.route("exportSettings", (arg) => {
    let controlId = "exportSettings";
    multiView.pushTo(c => {
        let editor = new CalculationEditor(c, controlId, null, {
            contractorId: parseNullableString(arg["cid"]),
            pipelineId: parseNullableString(arg["pid"]),
            routeId: parseNullableString(arg["rid"]),
            calculationIds: _calculationIds,
            viewObject: (equipmentId) => {
                //router.navigate(`new/objects/view/${equipmentId}?${makeUrlParametersPart(arg)}`);
            },
            event: {
                save: () => {
                    //let id = e.data.id;
                    router.navigate(`grid?${makeUrlParametersPart(arg)}`);
                    //router.navigate(`view/${id}?${makeUrlParametersPart(arg)}`);
                },
                cancel: () => {
                    router.navigate(`grid?${makeUrlParametersPart(arg)}`);
                }
            }
        });
        editor.init();
        return editor;
    }, CalculationEditor, controlId);
});

router.start();