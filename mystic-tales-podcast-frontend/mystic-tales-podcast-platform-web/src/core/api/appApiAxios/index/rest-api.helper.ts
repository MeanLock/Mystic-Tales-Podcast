import type { AxiosInstance, AxiosRequestConfig } from "axios";
import axios from "axios";
import { response_with_mess, type MessResponse } from "../response-generator";

interface RestApiHelperParams {
  instance: AxiosInstance;
  method: "get" | "post" | "put" | "delete" | "patch";
  url: string;
  data?: any;
  config?: AxiosRequestConfig;
}

export async function callAxiosRestApi(
  { instance, method, url, data, config }: RestApiHelperParams,
  title?: string
): Promise<MessResponse> {
  const mess_title = title ? title : null;
  const request_body = data ? data : null;

  try {
    const response = await instance.request({
      method,
      url,
      data: request_body,
      ...config,
    });
    return response_with_mess(
      true,
      false,
      mess_title,
      response.data.message,
      response.data
    );
  } catch (error: any) {
    console.error("API call error:", error);

    let isAppError = false;
    let errorMessage = "Có gì đó không ổn";

    if (axios.isAxiosError(error)) {
      if (error.response) {
        errorMessage =
          error.response.data ||
          error.response.data?.message ||
          `Lỗi API: ${error.response.status}`;
      } else if (error.request) {
        errorMessage = "Không có phản hồi từ server";
      } else {
        if (error.message === "No token found") {
          errorMessage = "Không tìm thấy token. Vui lòng đăng nhập lại!";
        } else if (error.message === "Token expired") {
          errorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
        } else {
          errorMessage = `Lỗi thiết lập request: ${error.message}`;
        }
      }
    } else {
      isAppError = true;
      errorMessage = error.message || "Lỗi không xác định";
    }
    return response_with_mess(
      false,
      isAppError,
      mess_title,
      errorMessage,
      null
    );
  }
}
