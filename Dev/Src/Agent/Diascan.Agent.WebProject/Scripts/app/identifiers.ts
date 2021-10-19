import { initLayout, router } from "./shared";
import MultiView from "./modules/layout/multiView";
import { IdentifiersGrid } from "./modules/grid/identifiersGrid";
import { IdentifiersCreator } from "./modules/creator/identifiersCreator";
import { IdentifiersEditor } from "./modules/editors/IdentifiersEditor";
import WaitModal from "./modules/layout/waitModal";
import {
    createCarrierIdentifier,
    editCarrierIdentifier,
    deleteCarrier,
    importExcel
} from "./modules/data/clients";
const currentPage = "identifiers";

let multiView = new MultiView($("#main"));
multiView.init();

initLayout(currentPage, {
    windowResize: (h: number) => {
        multiView.container.height(h);
    }
});

router.route("identifiersGrid",
    () => {
        multiView.pushTo(c => {
            let grid = new IdentifiersGrid(c,
                "IdentifiersGrid",
                {
                    event:
                    {
                        import: (e) => {
                            WaitModal.show("Загрузка файла...");
                            importExcel();
                            WaitModal.hide();
                        },
                        edit: (e) => {
                            router.navigate(`identifiers/${e.data.Id}/edit`);
                           
                        },
                        add: (e) => {
                            
                            router.navigate("identifiers/add");
                        },
                        remove: (e) => {
                            deleteCarrier(e.data.Id);
                            kendo.alert("Идентификатор удален ");
                        }
                    }
                });
            grid.init();
            grid.refresh();
            return grid;
        },
            IdentifiersGrid);
    });

router.route("identifiers/add",
    () => {
        multiView.pushAfter(c => {
            let creator = new IdentifiersCreator(c,
                {
                    events:
                    {
                        create: (e) => {
                            WaitModal.show("Добавление идентификатора...");
                            createCarrierIdentifier(String(e.id), e.change, String(e.type), String(e.sensorCount),
                                String(e.carrierDiametr), String(e.numberSensorBlock), String(e.speedMin), String(e.speedMax), String(e.defectoscope));
                            WaitModal.hide();
                            router.navigate(`identifiersGrid`);
                            creator.refresh();
                        },
                        cancel: () => {
                            router.navigate(`identifiersGrid`);
                        }
                    }
                });
            creator.init();
            return creator;
        },
            IdentifiersCreator);
    });
router.route("identifiers/:id/edit",
    (id: string) => {
        multiView.pushAfter(c => {
            let edit = new IdentifiersEditor(c,
                {
                    events:
                    {
                        create: (e) => {
                            WaitModal.show("Редактирование идентификатора...");

                            editCarrierIdentifier(id, e.change, String(e.type), String(e.sensorCount),
                                String(e.carrierDiametr), String(e.numberSensorBlock), String(e.speedMin), String(e.speedMax), String(e.defectoscope));
                            WaitModal.hide();
                            router.navigate(`identifiersGrid`);
                            edit.refresh();
                        },
                        cancel: () => {
                            router.navigate(`identifiersGrid`);
                        }
                    }
                });
            edit.init();
            return edit;
        },
            IdentifiersEditor);
    });
router.start();