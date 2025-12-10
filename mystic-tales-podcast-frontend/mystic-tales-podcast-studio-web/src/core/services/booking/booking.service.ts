
import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "booking-management-service/api/bookings";
const DASHBOARD_URL = "booking-management-service/api/report/bookings";

export const getBookingList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/podcaster`,
    }, "");

    return response;
}
export const getSummaryBooking = async (instance: AxiosInstance, reportPeriod: string) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${DASHBOARD_URL}/statistics/summary/me?report_period=${reportPeriod}`,
    });

    return response;
}
export const getBookingToneList = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/podcast-booking-tone`,
    }, "");

    return response;
}
export const getBookingTone = async (instance: AxiosInstance ) => {
    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/podcast-booking-tone/me`,
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

export const addDealing = async (instance: AxiosInstance, bookingId: Number, 
    payload: {
        BookingDealingInfo:{
            BookingRequirementInfoList: {
                Id: string;
                WordCount: number;
            }[];
            DeadlineDayCount: Number;
        }
    } ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${bookingId}/dealing`,
        data: payload,
    }, "");

    return response;
}
export const updateBookingTone = async (instance: AxiosInstance, isBuddy: boolean,
    payload: {
        PodcasterBookingToneApplyInfo: String[];
    } ) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/podcast-booking-tone/me/${isBuddy}`,
        data: payload,
    }, "");

    return response;
}
export const cancelProducing = async (instance: AxiosInstance, bookingId: Number, bookingManualCancelledReason: string) => {
    
    const payload = {
        BookingCancelInfo: {
            BookingManualCancelledReason: bookingManualCancelledReason
        }
    }
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${bookingId}/cancel-request`,
        data: payload,
    }, "");

    return response;
}

export const cancelQuotation = async (instance: AxiosInstance, bookingId: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${bookingId}/reject`,
    }, "");

    return response;
}