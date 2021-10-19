define(["require", "exports", "./apiClient", "./urls"], function (require, exports, apiClient_1, urls_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.allUserIdsClient = exports.routesClient = exports.pipelinesClient = void 0;
    exports.pipelinesClient = new apiClient_1.default({ baseUrl: urls_1.pipelinesBaseUrl, urlPut: "" });
    exports.routesClient = new apiClient_1.default({ baseUrl: urls_1.routesBaseUrl, urlPut: "" });
    exports.allUserIdsClient = new apiClient_1.default({ baseUrl: urls_1.allUserIdsGetUrl, urlPut: "" });
});
//# sourceMappingURL=clients.js.map