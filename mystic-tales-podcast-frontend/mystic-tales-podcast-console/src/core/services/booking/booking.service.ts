
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "booking-management-service/api/bookings";
const DASHBOARD_URL = "booking-management-service/api/report/bookings";

export const getBookingList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}`,
    }, "");

    return response;
}
export const getBookingHoldingList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/holding`,
    }, "");

    return response;
}
export const getSummaryBooking = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/system?report_period=${reportPeriod}`,
    });

    return response;
}
export const getTotalBooking = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/system/total?report_period=${reportPeriod}`,
    });

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
