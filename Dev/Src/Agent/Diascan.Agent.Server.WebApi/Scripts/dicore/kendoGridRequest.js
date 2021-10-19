var kendoGridRequest = (function () {
    function getVisibleColumns(kendoGrid) {
        var visibleColumns = "";
        var columns = kendoGrid.columns;
        var first = true;
        jQuery.each(columns, function (index) {
            if (!this.hidden) {
                if (!first)
                    visibleColumns += ",";
                visibleColumns += this.field;
                first = false;
            }
        });
        return visibleColumns;
    }

    return {
        getVisibleColumns: getVisibleColumns
    }
})();