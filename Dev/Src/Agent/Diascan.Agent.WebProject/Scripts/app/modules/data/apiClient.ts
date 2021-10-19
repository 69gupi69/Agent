const urlGetManyDefault = "";
const urlGetOneDefault = "{id}";
const urlPostDefault = "";
const urlPutDefault = "{id}";
const urlDeleteDefault = "{id}";
const eventIdKeyDefault = "reasonEventId";

interface ApiClientConfig {
    baseUrl: string;
    urlGetMany?: string;
    urlGetOne?: string;
    urlPost?: string;
    urlPut?: string;
    urlDelete?: string;
    eventIdKey?: string;
}

function makeUrl(base: string, rel: string): string {
    if (base[base.length - 1] == "/") {  
        return `${base}${rel}`;
    }
    return `${base}/${rel}`;
}

function generateMessage(code: number, message?: string): string {
    switch (code) {
        case 404:
            return "Объект не найден!";
        case 405:
            return "Функционал не поддерживатеся API";
        case 409:
            return "Объект с таким идентификатором уже создан";
        case 500:
            return "Внутренняя проблема сервера";
        default:
            return `${"Неизвестная ошибка сервера"}:${message} (${code})`;
    }
}

class ApiClient<T> {
    private _config: ApiClientConfig;

    protected static generateMessage(code: number, message?: string): string {
        return generateMessage(code, message);
    }



    constructor(config: ApiClientConfig) {
        this._config = {
            baseUrl: config.baseUrl,
            urlGetMany: config.urlGetMany === null || typeof config.urlGetMany === "undefined" ? urlGetManyDefault : config.urlGetMany
        }
    }
     getMany(success?: (data: T[]) => any, error?: (message, jqXHR: JQueryXHR) => void): JQueryDeferred<T[]> {
        let result = $.Deferred<T[]>();
        $.ajax({
            url: makeUrl(this._config.baseUrl, this._config.urlGetMany),
            method: "GET",
            cache: false,
            dataType: "json",
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: (data) => {
                if (success) {
                    success(data);
                }
                result.resolve(data);
            },
            error: ((jqXHR) => {
                console.error(jqXHR.status);
                if (error) {
                    error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                }
                result.reject(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
            })
        });
        return result;
    }
    

   //static getMany(url: string, success?: (data: any[]) => void, error?: (message, jqXHR: JQueryXHR) => void) {
   //    $.ajax({
   //        url: url,
   //        method: "GET",
   //        cache: false,
   //        dataType: "json",
   //        crossDomain: true,
   //        xhrFields: {
   //            withCredentials: true
   //        },
   //        success: (data) => {
   //            success(data);
   //        },
   //        error: ((jqXHR) => {
   //            error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
   //        })
   //    });
   //}

    static get<TOut>(url: string, data?: any, map?: (data: any) => TOut): JQueryDeferred<TOut> {
        let result = $.Deferred<TOut>();
        $.ajax({
                url: url,
                method: "GET",
                cache: false,
                dataType: "json",
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                data: data,
                success: (data) => {
                    var s = data;
                },
                error: ((jqXHR) => {
                    var m = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                })
            })
            .done((response) => {
                let responseData = map ? map(response) : response;
                result.resolve(responseData);
            })
            .fail((jqXHR) => {
                let message = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                result.reject(message, jqXHR);
            });

        return result;
    }

    static post<TOut>(url: string, data?: any): JQueryDeferred<TOut> {
        let result = $.Deferred<TOut>();
        $.ajax({
                url: url,
                method: "POST",
                xhrFields: { withCredentials: true },
                crossDomain: true,
                contentType: "application/json",
                cache: false,
                data: JSON.stringify(data)
            })
            .done((response) => {
                result.resolve(response);
            })
            .fail((jqXHR) => {
                let message = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                result.reject(message, jqXHR);
            });
        return result;
    }

    static put(url: string, data?: any): JQueryDeferred<string> {
        let result = $.Deferred<string>();
        $.ajax({
                url: url,
                method: "PUT",
                xhrFields: { withCredentials: true },
                crossDomain: true,
                contentType: "application/json",
                cache: false,
                data: kendo.stringify(data)
            })
            .done((response) => {
                result.resolve(response)
            })
            .fail((jqXHR) => {
                let message: any;
                if (jqXHR.responseText != undefined) {
                    message = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                } else {
                    message = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                }
                result.reject(message, jqXHR);
            });
        return result;
    }

    static delete(url: string): JQueryDeferred<string> {
        let result = $.Deferred<string>();
        $.ajax({
                url: url,
                method: "DELETE",
                xhrFields: { withCredentials: true },
                crossDomain: true,
                contentType: "application/json",
            })
            .done((response) => {
                result.resolve(response)
            })
            .fail((jqXHR) => {
                let message: any;
                if (jqXHR.responseText != undefined) {
                    message = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                } else {
                    message = ApiClient.generateMessage(jqXHR.status, jqXHR.statusText);
                }
                result.reject(message, jqXHR);
            });
        return result;
    }
 }

interface IGetDataConfig<T> {
    id?: string | number,
    success?: (data: T | T[]) => void,
    error?: (message, jqXHR: JQueryXHR) => void,

}

function GetData<T>(url: string, config?: IGetDataConfig<T>) {
    let reqUrl = (config && config.id) ? makeUrl(url, config.id.toString()) : url;
    return $.ajax({
        url: reqUrl,
        method: "GET",
        cache: false,
        dataType: "json",
        crossDomain: true,
        xhrFields: {
            withCredentials: true
        },
        success: (data) => {
            if (config && config.success) {
                config.success(data);
            }
        },
        error: ((jqXHR) => {
            console.error(jqXHR.status);
            if (config && config.error) {
                config.error(generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
            }
        })
    })
}


function GetDataAsync<T>(url: string, param?: string | number | { [key: string]: any }): JQueryDeferred<T> {
    let deff = $.Deferred<T>();
    let reqUrl = url;
    let queryData = {};
    if (param) {
        if (typeof param === "string" || typeof param === "number") {
            reqUrl = makeUrl(reqUrl, param.toString());
        }
        else {

            for (let key in param) {
                if (url.indexOf(`{${key}}`) > -1) {
                    reqUrl = reqUrl.replace(`{${key}}`, param[key].toString());
                }
                else {
                    queryData[key] = param;
                }
            }
        }
    }
    $.ajax({
        url: reqUrl,
        method: "GET",
        cache: false,
        dataType: "json",
        crossDomain: true,
        xhrFields: {
            withCredentials: true
        },
        data: queryData
    }).done((data) => {
        deff.resolve(data);
    }).fail(jqXHR => {
        console.error(jqXHR.status);
        deff.reject(generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
    });
    return deff;
}



export { ApiClient, ApiClientConfig, GetData, GetDataAsync };
export default ApiClient;