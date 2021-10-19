export class GridCommandHelper {
    private _grid: kendo.ui.Grid;

    constructor(grid: kendo.ui.Grid, commandHandler?: (command: string, data: any, target: HTMLElement) => void) {
        this._grid = grid;
        grid.bind("dataBound", () => {
            this._setTitles();
        });
        let opts = grid.getOptions();
        if (opts.columns) {
            for (let col of opts.columns) {
                if (col.command) {
                    if (Array.isArray(col.command)) {
                        for (let cmd of col.command) {
                            this._setHandler(grid, cmd, commandHandler);
                        }
                    }
                    else {
                        this._setHandler(grid, col.command, commandHandler);

                    }
                }
            }
        }
        grid.setOptions(opts);
    }

    private _setHandler(grid: kendo.ui.Grid, command: any,
        commandHandler?: (command: string, data: any, target: HTMLElement) => void) {
        command.click = (e) => {
            e.preventDefault();
            let cmd = command.name;
            let data = grid.dataItem($(e.currentTarget).closest("tr"));
            if (commandHandler) {
                commandHandler(cmd, data, e.currentTarget);
            }
        }
    }

    private _setTitles() {
        for (let i in this._grid.columns) {
            let col = this._grid.columns[i];
            if (col.command) {
                if (Array.isArray(col.command)) {
                    for (let j in col.command) {
                        this._setTitle(col.command[j], i);
                    }
                }
                else {
                    this._setTitle(col.command, i);
                }
            }
        }
    }


    private _setTitle(c, i) {
        if (c.title) {
            let clsFilter = c.className.replace(/\s+/g, ".");
            this._grid.wrapper
                .find("tr")
                .find(`td:eq(${i})`)
                .find(`a.${clsFilter}`)
                .attr("title", c.title);
        }
    }

    static compareArrays(array1, array2) {
        if (array1.length !== array2.length)
            return false;
        for (var i = 0; i < array1.length; i++) {
            if (array1[i] !== array2[i])
                return false;
        }
        return true;
    };

    static getContainer(id) {
        return $('#' + id);
    };

    static getFullElementId(containerId, id) {
        return containerId + id;
    };

    static createTemplateScript(id, body) {
        return '<script id="' + id + '" type="text/x-kendo-template">' + body + '</script>';

    }

    static pgDateToString(date, kendoFormat) {
        return kendo.toString((kendo.parseDate(date, "yyyy-MM-ddTHH:mm:ss")), kendoFormat)
    }

    static getGridSelectedElements(grid) {
        let selectedElements = [];
        grid.select()
            .each(function (e) {
                let dataItem = grid.dataItem(this);
                selectedElements.push(dataItem);
            });
        return selectedElements;
    }

    static getGridSelectedElementIds(grid) {
        if (grid != undefined) {
            let selectedElements = [];
            grid.select()
                .each(function (e) {
                    let dataItem = grid.dataItem(this);
                    selectedElements.push(dataItem);
                });
            return selectedElements.map((i) => {
                return i.Id;
            });
        }
        return undefined;
    }

    static removeA(arr, item) {
        var index = arr.indexOf(item);
        if (index > -1) {
            arr.splice(index, 1);
        }
        return arr;
    }
}