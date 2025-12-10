
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "podcast-service/api/review-sessions/episode-publish";


export const getReviewSession = async (instance: AxiosInstance, episodeId: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/episodes/${episodeId}/current`,
    }, "");

    return response;
}