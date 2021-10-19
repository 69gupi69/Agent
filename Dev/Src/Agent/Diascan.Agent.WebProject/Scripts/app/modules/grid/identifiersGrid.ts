import DataSources = require("../data/dataSources");
import Filters = require("./filters");
import { CustomGridOptions, BaseGrid, IGridOptions, IGridEvents } from "./grid";
import { UserRoleTypes } from "../../enums/userRoleTypes";
import GridHelpers = require("../utils/gridHelpers");
import GridCommandHelper = GridHelpers.GridCommandHelper;
import {
    getCarrierDataFromDataBaseUrl,
    exportFromExcelUrl
} from "../data/urls";


interface IdentifiersGridSettings extends IGridOptions {
    contractorId?: string;
    pipelineId?: string;
    routeId?: string;
    calculationIds?: string[];
    event?: IGridEvents;
}

class BaseIdentifiersGrid extends BaseGrid {
    protected gridOptions: CustomGridOptions;
    protected kendoGrid: any;

    protected get baseGridOptions():IdentifiersGridSettings {
        return <IdentifiersGridSettings>this.options;
    }
}


class IdentifiersGrid extends BaseIdentifiersGrid {
    get sort(): kendo.data.DataSourceParameterMapDataSort[] {
        return this._sort;
    }

    set sort(value: kendo.data.DataSourceParameterMapDataSort[]) {
        this._sort = value;
    }

    public dispose() {
        super.dispose();
    }

    private _sort: kendo.data.DataSourceParameterMapDataSort[];

    constructor(container: JQuery, id: string, options?: IdentifiersGridSettings) {
        super(container, id, options);
    }

    init() {
        super.init();
    }

    private get dataSource(): kendo.data.DataSource {
        return new kendo.data.DataSource({
            serverPaging: true,
            serverSorting: true,
            serverFiltering: true,
            pageSize: 10,
            page: 1,
            transport: {
                read: {
                    dataType: "JSON",
                    url: getCarrierDataFromDataBaseUrl,
                    xhrFields: { withCredentials: true },
                    type: "GET",
                    cache: false,
                },
                parameterMap: (data) => {
                    this._sort = data["sort"] ? data["sort"] : null;
                    return data;
                }
            },
            schema: {
                model: {
                    id: "id",
                    fields: {
                        Type: { type: "DataTypesExt", field: "Type" },
                        Defectoscope: { type: "string", field: "Defectoscope" },
                        CarrierDiameter: { type: "int", field: "CarrierDiameter" },
                        Sensorcount: { type: "int", field: "Sensorcount" },
                        SpeedMin: { type: "double", field: "SpeedMin" },
                        SpeedMax: { type: "double", field: "SpeedMax" },
                        Change: { type: "bool", field: "Change" },
                        NumberSensorsBlock: { type: "int", field: "NumberSensorsBlock" },
                        Id: { type: "int", field: "Id" }
                    }
                },
                data: "heads",
                total: "Count"
            }
        });
    }

    protected  onUpload(e) {
        if (e.files[0].extension.toLowerCase() != ".xlsx") {
            alert("Только .xlsx файлы могут быть импортированы.");
            e.preventDefault();
        }
    }

protected afterInit() {
    super.afterInit();
    this.setUploader("exportFromExcel", exportFromExcelUrl, this.onUpload);
}
    protected get gridOptions() {
      return {
            autoBind: false,
            size: "80%",
            dataSource: this.dataSource,
            columnMenu: true,
            resizable: true,
            reorderable: true,
            filterable: true,
            selectable: true,
            sortable: {
                mode: "multiple"
            },
            pageable: {
                input: true,
                refresh: true,
                pageSizes: [10, 15, 20, 25, 30, 50, 100, "All"],
                buttonCount: 10
            },
            toolbar: [
                { name: "addIdentifiers", text: "Добавить идентификатор" },
                { name: "importToExcel", text: "Импорт списка идентификаторов в Excel"},
                { name: "exportFromExcel", text: "Экспорт списка идентификаторов из Excel" }
            ],
          columns: [
                      {
                  field: "Type",
                          title: "Тип носителя",
                          sortable: false,
                          filterable: {
                              extra: false,
                              operators: {
                                  string: {
                                      endswith: "оканчивается на",
                                      eq: "равно",
                                      neq: "не равно",
                                      startswith: "начинающимися на",
                                      contains: "содержащими"
                                  }
                              }
                          },
                          width: "13%"
                      },
                      {
                          field: "Defectoscope",
                          title: "Наименование дефектоскопа",
                          filterable: {
                              extra: false,
                              operators: {
                                  string: {
                                      endswith: "оканчивается на",
                                      eq: "равно",
                                      neq: "не равно",
                                      startswith: "начинающимися на",
                                      contains: "содержащими"
                                  }
                              }
                          },
                          width: "10%"
                      },
                      {
                          field: "CarrierDiameter",
                          title: "Диаметр носителя, дюймы",
                          sortable: false,
                          filterable: false,
                          width: "18%"
                      },
                      {
                          field: "Sensorcount",
                          title: "Количество датчиков",
                          sortable: false,
                          filterable: {
                              extra: false,
                              operators: {
                                  string: {
                                      endswith: "оканчивается на",
                                      eq: "равно",
                                      neq: "не равно",
                                      startswith: "начинающимися на",
                                      contains: "содержащими"
                                  }
                              }
                          },
                          width: "13%"
                      },
                      {
                          field: "SpeedMin",
                          title: "Минимальное значение допустимой скорости",
                          filterable: {
                              extra: false,
                              operators: {
                                  string: {
                                      endswith: "оканчивается на",
                                      eq: "равно",
                                      neq: "не равно",
                                      startswith: "начинающимися на",
                                      contains: "содержащими"
                                  }
                              }
                          },
                          width: "15%"
                      },
                      {
                          field: "SpeedMax",
                          title: "Максимальное значение допустимой скорости",

                          filterable: {
                              extra: false,
                              operators: {
                                  string: {
                                      endswith: "оканчивается на",
                                      eq: "равно",
                                      neq: "не равно",
                                      startswith: "начинающимися на",
                                      contains: "содержащими"
                                  }
                              }
                          },
                          width: "15%"
                      },
                    {
                        field: "Change",
                        title: "Изменение (CDL/CDS)",
                        filterable: {
                            extra: false,
                        },
                        width: "12%"
                    },
                    {
                        field: "NumberSensorsBlock",
                        title: "Количество датчиков в блоке датчиков",
                        filterable: {
                            extra: false,
                            operators: {
                                string: {
                                    endswith: "оканчивается на",
                                    eq: "равно",
                                    neq: "не равно",
                                    startswith: "начинающимися на",
                                    contains: "содержащими"
                                }
                            }
                        },
                        width: "13%"
              },
                     
            {
            title: "Команды",
            command:
            [
                {
                    name: "editCommand",
                    text: "",
                    className: "k-icon k-i-track-changes-enable gridButton",
                    title: "Редактировать запись в Excel"
                },
                {
                    name: "deleteCommand",
                    text: "",
                    className: "k-icon k-i-delete gridButton",
                    title: "Удалить запись",
                }
            ],
            width: "8%"
      },
             
                ]
        };
    };
}

export { IdentifiersGrid, IdentifiersGridSettings }
export default IdentifiersGrid;