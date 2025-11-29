import { loginRequiredAxiosInstance } from "../api/rest-api/config/instances/v2";
import { getPublicSource } from "../services/file/file.service";
import NOT_FOUND_IMG from "../../assets/notfound.png"

export async function fetchImage(mainImageFileKey: string | null) {
    if (!mainImageFileKey) return "";

    try {
        const response = await getPublicSource(loginRequiredAxiosInstance, mainImageFileKey);
        console.log('Fetch Image Response:', response);
        if (response.success) {
            const imageUrl = response.data.FileUrl;
            return imageUrl;
        } else {
            console.error('API Error:', response.message);
        }
    } catch (error) {
        console.error('Lỗi khi fetch channel list:', error);
    }
}
export const handleImgError = (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
    if (e.currentTarget.src !== window.location.origin + NOT_FOUND_IMG) {
        e.currentTarget.src = NOT_FOUND_IMG;
    }
};

export async function urlToFile(url: string, filename: string, mimeType = "image/jpeg"): Promise<File> {
    const res = await fetch(url);
    const blob = await res.blob();
    return new File([blob], filename, { type: mimeType });
}