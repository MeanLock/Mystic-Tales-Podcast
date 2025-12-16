import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const USER_SERVICE_URL = "user-service/api";
const BOOKING_SERVICE_URL = "booking-management-service/api/misc";

export const getPublicSource = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${USER_SERVICE_URL}/misc/public-source/get-file-url/${fileKey}`,
    });

    return response;
}
export const getBuddyCommitment = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${USER_SERVICE_URL}/accounts/buddy-commitment-document/get-file-url/${fileKey}`,
    });

    return response;
}
export const getRequirements= async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BOOKING_SERVICE_URL}/public-source/get-file-url/${fileKey}`,
    });

    return response;
}
