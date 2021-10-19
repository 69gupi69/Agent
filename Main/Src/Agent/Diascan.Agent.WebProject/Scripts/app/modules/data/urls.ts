const apiUrl = window["BaseApiUrl"];
const webApiMainUrl = window["WebApiMainUrl"];
const _diagnostApiUrl = <string>window["BaseApiUrl"];
const diagnostApiUrl = _diagnostApiUrl[_diagnostApiUrl.length - 1] === "/" ? _diagnostApiUrl : `${_diagnostApiUrl}/`;

interface IActionUrls {
    notifyNewState: string,
    notifyReturnState: string,
    createTableDataset: string
}

export const deleteJsontUrl                      = `${apiUrl}Agent/DeleteJsonFromDataBase`;
export const createTableDatasetUrl               = `${apiUrl}Agent/ExcelFromDataBase`;

export const contractorsApiUrl                   = `${diagnostApiUrl}Agent/contractors`;
export const pipelinesBaseUrl                    = `${diagnostApiUrl}Agent/pipelines`;
export const routesBaseUrl                       = `${diagnostApiUrl}Agent/routes`;

export const getJsonsHeadAndCountFromDataBaseUrl = `${diagnostApiUrl}Agent/JsonsHeadAndCountFromDataBase`;
export const getAgentUsers                       = `${diagnostApiUrl}Agent/GetAgentUsers`; 
export const routesByPipelineUrl                 = routesBaseUrl + "/byPipelineId";
export const diagnosticsExportApiUrl             = `${apiUrl}views/diagnost/diagnostics/export`;
export const diagnosticsTreeUrl                  = `${webApiMainUrl}tree/diagnostics`;
export const mailingExcelFileUrl                 = `${diagnostApiUrl}Agent/MailingExcelFile`;
export const SelectiveMailingExcelFileUrl        = `${diagnostApiUrl}Agent/SelectiveMailing`;
export const allUserIdsGetUrl                    = `${diagnostApiUrl}Agent/GetAllUserIds`;