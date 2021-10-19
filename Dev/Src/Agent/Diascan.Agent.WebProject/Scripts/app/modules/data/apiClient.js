define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.GetDataAsync = exports.GetData = exports.ApiClient = void 0;
    var urlGetManyDefault = "";
    var urlGetOneDefault = "{id}";
    var urlPostDefault = "";
    var urlPutDefault = "{id}";
    var urlDeleteDefault = "{id}";
    var eventIdKeyDefault = "reasonEventId";
    function makeUrl(base, rel) {
        if (base[base.length - 1] == "/") {
            return "" + base + rel;
        }
        return base + "/" + rel;
    }
    function generateMessage(code, message) {
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
                return "Неизвестная ошибка сервера" + ":" + message + " (" + code + ")";
        }
    }
    var ApiClient = /** @class */ (function () {
        function ApiClient(config) {
            this._config = {
                baseUrl: config.baseUrl,
                urlGetMany: config.urlGetMany === null || typeof config.urlGetMany === "undefined" ? urlGetManyDefault : config.urlGetMany,
                urlGetOne: config.urlGetOne === null || typeof config.urlGetOne === "undefined" ? urlGetOneDefault : config.urlGetOne,
                urlPost: config.urlPost === null || typeof config.urlPost === "undefined" ? urlPostDefault : config.urlPost,
                urlPut: config.urlPut === null || typeof config.urlPut === "undefined" ? urlPutDefault : config.urlPut,
                urlDelete: config.urlDelete === null || typeof config.urlDelete === "undefined" ? urlDeleteDefault : config.urlDelete,
                eventIdKey: config.eventIdKey === null || typeof config.eventIdKey === "undefined" ? eventIdKeyDefault : config.urlDelete
            };
        }
        ApiClient.generateMessage = function (code, message) {
            return generateMessage(code, message);
        };
        ApiClient.prototype.getMany = function (success, error) {
            var result = $.Deferred();
            $.ajax({
                url: makeUrl(this._config.baseUrl, this._config.urlGetMany),
                method: "GET",
                cache: false,
                dataType: "json",
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function (data) {
                    if (success) {
                        success(data);
                    }
                    result.resolve(data);
                },
                error: (function (jqXHR) {
                    console.error(jqXHR.status);
                    if (error) {
                        error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                    }
                    result.reject(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                })
            });
            return result;
        };
        ApiClient.prototype.get = function (id, success, error) {
            var result = $.Deferred();
            $.ajax({
                url: makeUrl(this._config.baseUrl, this._config.urlGetOne).replace("{id}", id.toString()),
                method: "GET",
                cache: false,
                dataType: "json",
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function (data) {
                    if (success)
                        success(data);
                    result.resolve(data);
                },
                error: (function (jqXHR) {
                    console.error(jqXHR.status);
                    if (error) {
                        error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                    }
                    result.reject(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                })
            });
            return result;
        };
        ApiClient.prototype.putWithEvent = function (id, data, eventDescription, /*eventType: TechnologicalEventType,*/ success, error) {
            var xhr = null;
            //EventSender.send(eventDescription, eventType).done(evnt => {
            //    xhr = this.put(id, data, success, error, evnt);
            //});
            return xhr;
        };
        ApiClient.prototype.put = function (id, data, success, error, reasonEventId) {
            return $.ajax({
                url: reasonEventId
                    ? makeUrl(this._config.baseUrl, this._config.urlPut)
                        .replace("{id}", id.toString()) + ("?" + this._config.eventIdKey + "=" + reasonEventId)
                    : makeUrl(this._config.baseUrl, this._config.urlPut)
                        .replace("{id}", id.toString()),
                method: "PUT",
                contentType: "application/json",
                cache: false,
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function () {
                    success();
                },
                error: (function (jqXHR) {
                    console.error(jqXHR.status);
                    if (error) {
                        error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                    }
                }),
                data: kendo.stringify(data)
            });
        };
        ApiClient.prototype.postWithEvent = function (data, eventDescription, /*eventType: TechnologicalEventType,*/ success, error) {
            var xhr = null;
            //EventSender.send(eventDescription, eventType).done(evnt => {
            //    xhr = this.post(data, success, error, evnt);
            //});
            return xhr;
        };
        ApiClient.prototype.post = function (data, success, error, reasonEventId) {
            return $.ajax({
                url: reasonEventId
                    ? this._config.baseUrl + ("?" + this._config.eventIdKey + "=" + reasonEventId)
                    : this._config.baseUrl,
                method: "POST",
                contentType: "application/json",
                cache: false,
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function (data) {
                    success(data);
                },
                error: (function (jqXHR) {
                    console.error(jqXHR.status);
                    if (error) {
                        error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                    }
                }),
                data: JSON.stringify(data)
            });
        };
        ApiClient.prototype.deleteWithEvent = function (id, eventDescription, /* eventType: TechnologicalEventType,*/ success, error) {
            var xhr = null;
            //EventSender.send(eventDescription, eventType).done(evnt => {
            //    xhr = this.delete(id, success, error, evnt);
            //});
            return xhr;
        };
        ApiClient.prototype.delete = function (id, success, error, reasonEventId) {
            return $.ajax({
                url: reasonEventId
                    ? makeUrl(this._config.baseUrl, this._config.urlDelete)
                        .replace("{id}", id.toString()) + ("?" + this._config.eventIdKey + "=" + reasonEventId)
                    : makeUrl(this._config.baseUrl, this._config.urlDelete)
                        .replace("{id}", id.toString()),
                method: "DELETE",
                contentType: "application/json",
                cache: false,
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function () {
                    success();
                },
                error: (function (jqXHR) {
                    console.error(jqXHR.status);
                    if (error) {
                        error(ApiClient.generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                    }
                })
            });
        };
        return ApiClient;
    }());
    exports.ApiClient = ApiClient;
    function GetData(url, config) {
        var reqUrl = (config && config.id) ? makeUrl(url, config.id.toString()) : url;
        return $.ajax({
            url: reqUrl,
            method: "GET",
            cache: false,
            dataType: "json",
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            success: function (data) {
                if (config && config.success) {
                    config.success(data);
                }
            },
            error: (function (jqXHR) {
                console.error(jqXHR.status);
                if (config && config.error) {
                    config.error(generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
                }
            })
        });
    }
    exports.GetData = GetData;
    function GetDataAsync(url, param) {
        var deff = $.Deferred();
        var reqUrl = url;
        var queryData = {};
        if (param) {
            if (typeof param === "string" || typeof param === "number") {
                reqUrl = makeUrl(reqUrl, param.toString());
            }
            else {
                for (var key in param) {
                    if (url.indexOf("{" + key + "}") > -1) {
                        reqUrl = reqUrl.replace("{" + key + "}", param[key].toString());
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
        }).done(function (data) {
            deff.resolve(data);
        }).fail(function (jqXHR) {
            console.error(jqXHR.status);
            deff.reject(generateMessage(jqXHR.status, jqXHR.statusText), jqXHR);
        });
        return deff;
    }
    exports.GetDataAsync = GetDataAsync;
    exports.default = ApiClient;
});
//# sourceMappingURL=apiClient.js.map