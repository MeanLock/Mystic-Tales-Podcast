import { jwtDecode } from "jwt-decode";

/**
 * Giải mã JWT để lấy payload
 * @param token Chuỗi JWT
 * @returns Payload của JWT hoặc null nếu lỗi
 */
const decodeToken = (token: string): any | null => {
  try {
    console.log("Decoded: ", jwtDecode(token));
    return jwtDecode(token);
  } catch (error) {
    console.error("Invalid token", error);
    return null;
  }
};

/**
 * Kiểm tra xem JWT có hợp lệ hay không (có hết hạn không)
 * @returns true nếu hợp lệ, false nếu không hợp lệ
 */
const isTokenValid = (token: string): boolean => {
  if (!token) return false;

  const decoded: any = decodeToken(token);
  return !!decoded && typeof decoded.exp === "number";
};

const isTokenNotExpired = (token: string): boolean => {
  const decoded: any = decodeToken(token);
  if (!decoded || typeof decoded.exp !== "number") return false;

  const currentTime = Math.floor(Date.now() / 1000);
  return decoded.exp > currentTime;
};

const JwtUtil = {
  decodeToken,
  isTokenValid,
  isTokenNotExpired,
};

export { JwtUtil };
