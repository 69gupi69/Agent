
export abstract class GridFilter {
    abstract ui(element);
}

export class DropDownListOptions {
    dataTextField?: string;
    dataValueField?: string;
}

export class DropdownGridFilter extends GridFilter {
    private readonly _dataSource: kendo.data.DataSource;
    private readonly _options: DropDownListOptions;

    constructor(dataSource: kendo.data.DataSource, options?: DropDownListOptions) {
        super();
        this._dataSource = dataSource;
        this._options = options;
    }

    ui(element) {
        element.kendoDropDownList({
            dataSource: this._dataSource,
            dataValueField: this._options && this._options.dataValueField ? this._options.dataValueField : "id",
            dataTextField: this._options && this._options.dataTextField ? this._options.dataTextField : "name"
        });
    }


}

export function getMultiFilterTemplate(field: string, value: string, text: string): string {
    return `<li class="filterItem" class="k-item k-state-default">
    <label class="k-link" ><input type="checkbox" name="${field}" value="#= data.${value} #" >
    # if(typeof data.${text} === "undefined") { #
        <span style="font-weight: bold">Выбрать все</span>
    # }else{ #
        <span>#= data.${text} #</span>
    # } #
    </label>
</li>`;
}
