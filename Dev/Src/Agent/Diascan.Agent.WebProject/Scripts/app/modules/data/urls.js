define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.allUserIdsGetUrl = exports.SelectiveMailingExcelFileUrl = exports.mailingExcelFileUrl = exports.diagnosticsTreeUrl = exports.diagnosticsExportApiUrl = exports.routesByPipelineUrl = exports.getAgentUsers = exports.getJsonsHeadAndCountFromDataBaseUrl = exports.routesBaseUrl = exports.pipelinesBaseUrl = exports.contractorsApiUrl = exports.createTableDatasetUrl = exports.deleteJsontUrl = void 0;
    var apiUrl = window["BaseApiUrl"];
    var webApiMainUrl = window["WebApiMainUrl"];
    var _diagnostApiUrl = window["BaseApiUrl"];
    var diagnostApiUrl = _diagnostApiUrl[_diagnostApiUrl.length - 1] === "/" ? _diagnostApiUrl : _diagnostApiUrl + "/";
    exports.deleteJsontUrl = apiUrl + "Agent/DeleteJsonFromDataBase";
    exports.createTableDatasetUrl = apiUrl + "Agent/ExcelFromDataBase";
    exports.contractorsApiUrl = diagnostApiUrl + "Agent/contractors";
    exports.pipelinesBaseUrl = diagnostApiUrl + "Agent/pipelines";
    exports.routesBaseUrl = diagnostApiUrl + "Agent/routes";
    exports.getJsonsHeadAndCountFromDataBaseUrl = diagnostApiUrl + "Agent/JsonsHeadAndCountFromDataBase";
    exports.getAgentUsers = diagnostApiUrl + "Agent/GetAgentUsers";
    exports.routesByPipelineUrl = exports.routesBaseUrl + "/byPipelineId";
    exports.diagnosticsExportApiUrl = apiUrl + "views/diagnost/diagnostics/export";
    exports.diagnosticsTreeUrl = webApiMainUrl + "tree/diagnostics";
    exports.mailingExcelFileUrl = diagnostApiUrl + "Agent/MailingExcelFile";
    exports.SelectiveMailingExcelFileUrl = diagnostApiUrl + "Agent/SelectiveMailing";
    exports.allUserIdsGetUrl = diagnostApiUrl + "Agent/GetAllUserIds";
});
//# sourceMappingURL=urls.js.map