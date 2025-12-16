
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

export const submitAudio = async (instance: AxiosInstance, id: String, audioFileArray: File[]) => {
    const form = new FormData();

    if (audioFileArray && audioFileArray.length > 0) {
        audioFileArray.forEach((file) => {
            form.append("AudioFiles", file);
        });
    }


    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${id}/submit`,
        data: form,
    }, "");

    return response;
}
export const acceptProducingRequest = async (instance: AxiosInstance, id: String, isAccept: boolean, rejectReason: string) => {
    const payload = {
        RejectReason: rejectReason
    }
    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${id}/accept/${isAccept}`,
        data: payload,
    }, "");
    return response;
}
// export const cancelQuotation = async (instance: AxiosInstance, bookingId: Number) => {

//     const response = await callAxiosRestApi({
//         instance: instance,
//         method: "put",
//         url: `${BASE_URL}/${bookingId}/reject`,
//     }, "");

//     return response;
// }