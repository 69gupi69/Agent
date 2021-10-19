
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
            urlGetMany: config.urlGetMany === null || typeof config.urlGetMany === "undefined" ? urlGetManyDefault : config.urlGetMany,
            urlGetOne: config.urlGetOne === null || typeof config.urlGetOne === "undefined" ? urlGetOneDefault : config.urlGetOne,
            urlPost: config.urlPost === null || typeof config.urlPost === "undefined" ? urlPostDefault : config.urlPost,
            urlPut: config.urlPut === null || typeof config.urlPut === "undefined" ? urlPutDefault : config.urlPut,
            urlDelete: config.urlDelete === null || typeof config.urlDelete === "undefined" ? urlDeleteDefault : config.urlDelete,
            eventIdKey: config.eventIdKey === null || typeof config.eventIdKey === "undefined" ? eventIdKeyDefault : config.urlDelete
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

    get(id: string | number, success?: (data: T) => void, error?: (message, jqXHR: JQueryXHR) => void): JQueryDeferred<T> {
        let result = $.Deferred<T>();
        $.ajax({
            url: makeUrl(this._config.baseUrl, this._config.urlGetOne).replace("{id}", id.toString()),
            method: "GET",
            cache: false,
            dataType: "json",
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: (data) => {
                if (success)
                    success(data);
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

    putWithEvent(id: string | number, data: T, eventDescription: string, /*eventType: TechnologicalEventType,*/
        success: () => void, error?: (message, jqXHR: JQueryXHR) => void) {
        let xhr = null;
        //EventSender.send(eventDescription, eventType).done(evnt => {
        //    xhr = this.put(id, data, success, error, evnt);
        //});
        return xhr;
    }

    put(id: string | number, data: T, success: () => void, error?: (message, jqXHR: JQueryXHR) => void, reasonEventId?: string) {
        return $.ajax({
            url: reasonEventId
                ? makeUrl(this._config.baseUrl, this._config.urlPut)
                    .replace("{id}", id.toString()) + `?${this._config.eventIdKey}=${reasonEventId}`
                : makeUrl(this._config.baseUrl, this._config.urlPut)
                    .replace("{id}", id.toString()),
            method: "PUT",
            contentType: "application/json",
            cache: false,
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: () => {
                success();
            },
            error: ((jqXHR) => {
                console.error(jqXHR.status);
                if (error) {
                    error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                }
            }),
            data: kendo.stringify(data)
        });
    }

    postWithEvent(data: T, eventDescription: string, /*eventType: TechnologicalEventType,*/
        success: (data: any) => void, error?: (message, jqXHR: JQueryXHR) => void) {
        let xhr = null;
        //EventSender.send(eventDescription, eventType).done(evnt => {
        //    xhr = this.post(data, success, error, evnt);
        //});
        return xhr;
    }

    post(data: T, success: (data: any) => void, error?: (message, jqXHR: JQueryXHR) => void, reasonEventId?: string) {
        return $.ajax({
            url: reasonEventId
                ? this._config.baseUrl + `?${this._config.eventIdKey}=${reasonEventId}`
                : this._config.baseUrl,
            method: "POST",
            contentType: "application/json",
            cache: false,
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: (data) => {
                success(data);
            },
            error: ((jqXHR) => {
                console.error(jqXHR.status);
                if (error) {
                    error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                }
            }),
            data: JSON.stringify(data)
        });
    }

    deleteWithEvent(id: string | number, eventDescription: string,/* eventType: TechnologicalEventType,*/
        success: () => void, error?: (message, jqXHR: JQueryXHR) => void) {
        let xhr = null;
        //EventSender.send(eventDescription, eventType).done(evnt => {
        //    xhr = this.delete(id, success, error, evnt);
        //});
        return xhr;
    }

    delete(id: string | number, success: () => void, error?: (message, jqXHR: JQueryXHR) => void, reasonEventId?: string) {
        return $.ajax({
            url: reasonEventId
                ? makeUrl(this._config.baseUrl, this._config.urlDelete)
                    .replace("{id}", id.toString()) + `?${this._config.eventIdKey}=${reasonEventId}`
                : makeUrl(this._config.baseUrl, this._config.urlDelete)
                    .replace("{id}", id.toString()),
            method: "DELETE",
            contentType: "application/json",
            cache: false,
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: () => {
                success();
            },
            error: ((jqXHR) => {
                console.error(jqXHR.status);
                if (error) {
                    error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                }
            })
        });
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