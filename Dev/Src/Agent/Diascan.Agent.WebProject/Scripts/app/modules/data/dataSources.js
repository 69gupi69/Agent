define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function createDataSource(url, parse, sort, filter) {
        return new kendo.data.DataSource({
            transport: {
                read: {
                    url: url,
                    dataType: 'json',
                    crossDomain: true,
                    xhrFields: { withCredentials: true },
                    type: "GET",
                    cache: false,
                }
            },
            schema: {
                model: {
                    id: "Id"
                },
                parse: function (data) {
                    if (parse) {
                        return parse(data);
                    }
                    return data;
                }
            },
            sort: sort || {},
            filter: filter || {}
        });
    }
});
//# sourceMappingURL=dataSources.js.map