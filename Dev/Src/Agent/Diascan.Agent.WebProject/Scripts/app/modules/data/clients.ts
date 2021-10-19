import ApiClient from "./apiClient";
import {
    pipelinesBaseUrl,
    routesBaseUrl,
    allUserIdsGetUrl,
    deleteCarrierDataUrl,
    insertCarrierDataUrl,
    updateCarrierDataUrl,
    importToExcelUrl
    } from "./urls";

export const pipelinesClient = new ApiClient({ baseUrl: pipelinesBaseUrl, urlPut: "" });
export const routesClient = new ApiClient({ baseUrl: routesBaseUrl, urlPut: "" });
export const allUserIdsClient = new ApiClient({ baseUrl: allUserIdsGetUrl, urlPut: "" });

export function createCarrierIdentifier(id: string, change: string, type: string, sensorCount: string, carrierDiametr: string, numberSensorsBlock: string, speedMin: string, speedMax: string, defectoscope: string) {
    let url = `${insertCarrierDataUrl}`;
    let data = {
        Id: id,
        Change: change,
        Type: type,
        Sensorcount: sensorCount,
        CarrierDiametr: carrierDiametr,
        NumberSensorsBlock: numberSensorsBlock,
        SpeedMin: speedMin,
        SpeedMax: speedMax,
        Defectoscope: defectoscope
    };
    return ApiClient.post<string>(url,data);
}

export function editCarrierIdentifier(id:string, change: string, type: string, sensorCount: string, carrierDiametr: string, numberSensorsBlock: string, speedMin: string, speedMax: string, defectoscope: string) {
    let url = `${updateCarrierDataUrl}`;
    let data = {
        Id: id,
        Change: change,
        Type: type,
        Sensorcount: sensorCount,
        CarrierDiametr: carrierDiametr,
        NumberSensorsBlock: numberSensorsBlock,
        SpeedMin: speedMin,
        SpeedMax: speedMax,
        Defectoscope: defectoscope
    };
    return ApiClient.put(url, data);
}

export function deleteCarrier(id: string) {
    return ApiClient.delete(`${deleteCarrierDataUrl(id)}`);
}

export function importExcel() {
    return ApiClient.get(`${importToExcelUrl}`)
}