import {
    getAgentUsers 
} from "../data/urls";
import DataSources = require("../data/dataSources");
import Filters = require("./filters");
import { CustomGridOptions, BaseGrid, IGridOptions, IGridEvents } from "./grid";
import { UserRoleTypes } from "../../enums/userRoleTypes";
import GridHelpers = require("../utils/gridHelpers");
import GridCommandHelper = GridHelpers.GridCommandHelper;

interface IUsersGridEvents extends IGridEvents {
    dataBound?: (e) => void;
    dataBinding?: (e) => void;
}

interface UsersGridSettings extends IGridOptions {
    selectedIds?: string[];
    event?: IUsersGridEvents;
}


class UsersGrid extends BaseGrid {

    get sort(): kendo.data.DataSourceParameterMapDataSort[] {
        return this._sort;
    }

    set sort(value: kendo.data.DataSourceParameterMapDataSort[]) {
        this._sort = value;
    }

    public dispose() {
        super.dispose();
    }

    private _selectedIds: string[];
    private _filters: kendo.data.DataSourceParameterMapDataFilter;
    private _sort: kendo.data.DataSourceParameterMapDataSort[];

    private get dataSource(): kendo.data.DataSource {
        return new kendo.data.DataSource({
            serverPaging: true,
            pageSize: 15,
            page: 1,
            transport: {
                read: {
                    url: getAgentUsers,
                    crossDomain: true,
                    xhrFields: { withCredentials: true },
                    type: "GET",
                    cache: false
                },
                parameterMap: (data) => {
                    data["IsSelected"] = this._selectedIds;
                    this._filters = data["filter"] ? data["filter"] : null;
                    this._sort = data["sort"] ? data["sort"] : null;
                    return data;
                }
            },
            schema: {
                model: {
                    id: "Id",
                    fields: {
                        IsSelected: { type: "bool", field: "IsSelected"},
                        Id: { type: "string", field: "Id" },
                        FirstName: { type: "string", field: "FirstName" },
                        SecondName: { type: "string", field: "SecondName" },
                        LastName: { type: "string", field: "LastName" },
                        Phone: { type: "string", field: "Phone" },
                        Email: { type: "string", field: "Email" },
                        ContractorName: { type: "string", field: "ContractorName" },
                        PositionName: { type: "string", field: "PositionName" }
                    }
                },
                data: "dataResult",
                total: "totalCount"
            }
        });
    }

    protected get gridOptions() {
        return {
            autoBind: false,
            size: "80%",
            dataSource: this.dataSource,
            selectable: "multiple, row",
            pageable: {
                input: true,
                refresh: true,
                pageSizes: [ 15, 20, 25, 30, 50, 100, "All"],
                buttonCount: 15
            },
            dataBound: this.options.event["dataBound"],
            dataBinding: this.options.event["dataBinding"],
            persistSelection: true,
            resizable: true,
            sortable: {
                mode: "multiple"
            },
            columns: [
                {
                    field: "IsSelected",
                    headerTemplate: "<input type='checkbox' class='headCheckbox' />",
                    template: "<input type='checkbox' class='checkbox' />",
                    width: "2%",
                    style:"text-align:center;"
                },
                {
                    field: "LastName",
                    title: "Фамилия",
                    filterable: false,
                    width: "10%"
                },
                {
                    field: "FirstName",
                    title: "Имя",
                    filterable: true,
                    width: "10%"
                },
                {
                    field: "SecondName",
                    title: "Отчество",
                    filterable: true,
                    width: "10%"
                },
                {
                    field: "Phone",
                    title: "Телефон",
                    filterable: false,
                    width: "7%"
                },
                {
                    field: "Email",
                    title: "Адрес почты",
                    filterable: false,
                    width: "18%"
                },
                {
                    field: "PositionName",
                    title: "Должность",
                    filterable: false,
                    width: "15%"
                },
                {
                    field: "ContractorName",
                    title: "Отдел",
                    filterable: false,
                    width: "45%"
                }
            ],
            editable: "inline"
        };
    };

    set path(data) {
        this._selectedIds = data["selectedIds"];
    }

    constructor(container: JQuery, id: string, options?: UsersGridSettings) {
        super(container, id, options);
        this._selectedIds = options.selectedIds;
    }

    init() {
        super.init();

    }
}

export { UsersGrid, UsersGridSettings }
export default UsersGrid;