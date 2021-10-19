import ApiClient from "./apiClient";
import {
    pipelinesBaseUrl,
    routesBaseUrl,
    allUserIdsGetUrl
    } from "./urls";

export const pipelinesClient = new ApiClient({ baseUrl: pipelinesBaseUrl, urlPut: "" });
export const routesClient = new ApiClient({ baseUrl: routesBaseUrl, urlPut: "" });
export const allUserIdsClient = new ApiClient({ baseUrl: allUserIdsGetUrl, urlPut: "" });
