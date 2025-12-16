
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "booking-management-service/api/producing-requests";

export const getProducingRequestDetail = async (instance: AxiosInstance, id: String) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${id}`,
    }, "");

    return response;
}
export const getProducingRequestAudio = async (instance: AxiosInstance, fileKey: String) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/audio/get-file-url/${fileKey}`,
    }, "");

    return response;
}

