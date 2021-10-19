import { 
    allUserIdsGetUrl
} from "./urls";


function createDataSource(url: string, parse?: (any) => any, sort?: kendo.data.DataSourceFilter,
    filter?: kendo.data.DataSourceFilter): kendo.data.DataSource {
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
            parse: (data) => {
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