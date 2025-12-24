export interface MessResponse {
    success: boolean;
    isAppError: boolean;
    message: {
        color: string | null;
        title: string | null;
        content: string | null;
    };
    data: any | null;
}


export function response_with_mess (
    isSuccess: boolean,
    isAppError: boolean,
    title: string | null,
    mess: string | null,
    data: any | null
): MessResponse {

    const response = {

        success: isSuccess,
        isAppError: isAppError,
        message: {
            color: isSuccess ? "success" : "danger",
            title: title,
            content: mess,
        },
        data: data
    }
    console.log("RESPONSE HERE", response)
    return response;
}