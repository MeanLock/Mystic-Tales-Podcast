import { AxiosInstance } from "axios";
import { callAxiosRestApi } from "../../api/rest-api/main/api-call";

const BASE_URL = "moderation-service/api/dmca-accusations";

export const getDMCAList = async (instance: AxiosInstance) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}`,
    });

    return response;
}
export const getDMCADetail = async (instance: AxiosInstance, id: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${id}`,
    });

    return response;
}
export const getDMCANoticeFile = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/dmca-notice/get-file-url/${fileKey}`,
    });

    return response;
}
export const getCounterNoticeFile = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/counter-notice/get-file-url/${fileKey}`,
    });

    return response;
}
export const getLawsuitProofFile = async (instance: AxiosInstance, fileKey: string) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/lawsuit-document/get-file-url/${fileKey}`,
    });

    return response;
}
export const getDMCAReport = async (instance: AxiosInstance, DMCAAccusationId: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "get",
        url: `${BASE_URL}/${DMCAAccusationId}/dmca-conclusion-report`,
    });

    return response;
}
export const assignStaff = async (instance: AxiosInstance, accountId: Number, DMCAAccusationId: Number) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${DMCAAccusationId}/assign/staffs/${accountId}`,
    });

    return response;
};
export const updateStatus = async (instance: AxiosInstance, status: String, DMCAAccusationId: Number, DMCAAccusationTakenDownReasonEnum?: string,
    payload?: {
        AttachmentFiles?: File[];
    }
) => {

    if (DMCAAccusationTakenDownReasonEnum) {
        const formData = new FormData();

        if (payload && payload.AttachmentFiles && payload.AttachmentFiles.length > 0) {
            payload.AttachmentFiles.forEach((file) => {
                formData.append("AttachmentFiles", file);
            });
        }
        const response = await callAxiosRestApi({
            instance: instance,
            method: "put",
            url: `${BASE_URL}/${DMCAAccusationId}?DMCAAccusationAction=${status}&DMCAAccusationTakenDownReasonEnum=${DMCAAccusationTakenDownReasonEnum}`,
            data: formData
        });

        return response;
        
    } else {
        const response = await callAxiosRestApi({
            instance: instance,
            method: "put",
            url: `${BASE_URL}/${DMCAAccusationId}?DMCAAccusationAction=${status}`,
        });

        return response;
    }

};
export const createShowDMCAAccusation = async (
    instance: AxiosInstance,
    id: string,
    payload: {
        DMCANoticeCreateInfo: {
            AccuserEmail: string;
            AccuserPhone: string;
            AccuserFullName: string;
        };
        DMCANoticeAttachFiles?: File[];
    }
) => {
    const formData = new FormData();
    formData.append("DMCANoticeCreateInfo", JSON.stringify(payload.DMCANoticeCreateInfo));

    if (payload.DMCANoticeAttachFiles && payload.DMCANoticeAttachFiles.length > 0) {
        payload.DMCANoticeAttachFiles.forEach((file) => {
            formData.append("DMCANoticeAttachFiles", file);
        });
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/shows/${id}`,
        data: formData,
    });

    return response;
}
export const createEpisodeDMCAAccusation = async (
    instance: AxiosInstance,
    id: string,
    payload: {
        DMCANoticeCreateInfo: {
            AccuserEmail: string;
            AccuserPhone: string;
            AccuserFullName: string;
        };
        DMCANoticeAttachFiles?: File[];
    }
) => {
    const formData = new FormData();
    formData.append("DMCANoticeCreateInfo", JSON.stringify(payload.DMCANoticeCreateInfo));

    if (payload.DMCANoticeAttachFiles && payload.DMCANoticeAttachFiles.length > 0) {
        payload.DMCANoticeAttachFiles.forEach((file) => {
            formData.append("DMCANoticeAttachFiles", file);
        });
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/episodes/${id}`,
        data: formData,
    });

    return response;
}
export const createCounterNotice = async (
    instance: AxiosInstance,
    id: Number,
    payload: {
        CounterNoticeAttachFiles: File[];
    }
) => {
    const formData = new FormData();

    if (payload.CounterNoticeAttachFiles && payload.CounterNoticeAttachFiles.length > 0) {
        payload.CounterNoticeAttachFiles.forEach((file) => {
            formData.append("CounterNoticeAttachFiles", file);
        });
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${id}/counter-notice`,
        data: formData,
    });

    return response;
}
export const createLawsuit = async (
    instance: AxiosInstance,
    id: Number,
    payload: {
        LawsuitProofAttachFiles: File[];
    }
) => {
    const formData = new FormData();

    if (payload.LawsuitProofAttachFiles && payload.LawsuitProofAttachFiles.length > 0) {
        payload.LawsuitProofAttachFiles.forEach((file) => {
            formData.append("LawsuitProofAttachFiles", file);
        });
    }

    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${id}/lawsuit`,
        data: formData,
    });

    return response;
}
export const createReport = async (
    instance: AxiosInstance,
    DMCAAccusationId: Number,
    payload: {
        DMCAAccusationConclusationReportInfo: {
            DmcaAccusationConclusionReportTypeId: Number;
            Description: string | undefined;
            InvalidReason: string | undefined;
        };
    }
) => {
    if (payload.DMCAAccusationConclusationReportInfo.Description === '' || payload.DMCAAccusationConclusationReportInfo.Description === undefined || payload.DMCAAccusationConclusationReportInfo.InvalidReason === '' || payload.DMCAAccusationConclusationReportInfo.InvalidReason === undefined) {
        const newPayload = {
            ...payload,
            DMCAAccusationConclusationReportInfo: {
                ...payload.DMCAAccusationConclusationReportInfo,
                Description: null,
                InvalidReason: null
            }
        };
        payload = newPayload;
    }
    const response = await callAxiosRestApi({
        instance: instance,
        method: "post",
        url: `${BASE_URL}/${DMCAAccusationId}/create-report`,
        data: payload,
    });

    return response;
}
export const validateReport = async (instance: AxiosInstance, reportId: String, isValid: boolean) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${reportId}/${isValid}`,
    });

    return response;
};
export const cancelReport = async (instance: AxiosInstance, reportId: String) => {

    const response = await callAxiosRestApi({
        instance: instance,
        method: "put",
        url: `${BASE_URL}/${reportId}/cancel`,
    });

    return response;
};