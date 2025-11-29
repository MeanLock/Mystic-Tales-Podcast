
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "booking-management-service/api/bookings";

export const getBookingList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}`,
    }, "");

    return response;
}

export const getBookingDetail = async (instance: AxiosInstance, bookingId: Number ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${bookingId}`,
    }, "");

    return response;
}

export const cancelRequest = async (instance: AxiosInstance, bookingId: Number, isAccepted: boolean,
    payload?: {
        BookingCancelValidationInfo:{
            CustomerBookingCancelDepositRefundRate?: number;
            PodcastBuddyBookingCancelDepositRefundRate?: number ;
        }
    }  ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${bookingId}/cancel-request/${isAccepted}`,
        data: payload ? payload : {},
    }, "");

    return response;
}
