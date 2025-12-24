import { publicAxiosInstance } from "@/core/api/appApiAxios/config/instances";
import { callAxiosRestApi } from "@/core/api/appApiAxios/index";

const ACCOUNT_PUBLIC_SOURCE_PREFIX_API =
  "/api/user-service/api/misc/public-source/get-file-url/";
const BOOKING_PUBLIC_SOURCE_PREFIX_API =
  "/api/booking-management-service/api/misc/public-source/get-file-url/";
const PODCAST_PUBLIC_SOURCE_PREFIX_API =
  "/api/podcast-service/api/misc/public-source/get-file-url/";
const CATEGORY_PUBLIC_SOURCE_PREFIX_API =
  "/api/podcast-service/api/categories/podcast-categories/get-file-url/";
const WITHDRAWAL_RECEIPT_IMAGE_FILE_PREFIX_API =
  "/api/transaction-service/api/account-balance-transactions/transfer-receipt-image/get-file-url/";

interface GetFileUrlParams {
  fileKey: string;
  type:
    | "AccountPublicSource"
    | "BookingPublicSource"
    | "PodcastPublicSource"
    | "CategoryPublicSource"
    | "WithdrawalReceipt";
}

export const getPublisSourceFileUrl = async ({
  fileKey,
  type,
}: GetFileUrlParams): Promise<string | null> => {
  let url = "";
  switch (type) {
    case "AccountPublicSource":
      url = `${ACCOUNT_PUBLIC_SOURCE_PREFIX_API}${fileKey}`;
      break;
    case "BookingPublicSource":
      url = `${BOOKING_PUBLIC_SOURCE_PREFIX_API}${fileKey}`;
      break;
    case "PodcastPublicSource":
      url = `${PODCAST_PUBLIC_SOURCE_PREFIX_API}${fileKey}`;
      break;
    case "CategoryPublicSource":
      url = `${CATEGORY_PUBLIC_SOURCE_PREFIX_API}${fileKey}`;
      break;
    case "WithdrawalReceipt":
      url = `${WITHDRAWAL_RECEIPT_IMAGE_FILE_PREFIX_API}${fileKey}`;
      break;
    default:
      url = `${ACCOUNT_PUBLIC_SOURCE_PREFIX_API}${fileKey}`;
  }
  if (url === "") return null;
  const response = await callAxiosRestApi({
    instance: publicAxiosInstance,
    method: "get",
    url: url,
  });
  if (response.success && response.data && response.data.FileUrl) {
    return response.data.FileUrl;
  } else {
    return null;
  }
};
