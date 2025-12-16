// src/core/api/appApi/token.ts

/** Getter đồng bộ, sẽ được cấu hình từ store.ts */
let _getToken: () => string | undefined = () => undefined;

/** Gọi hàm này trong store.ts sau khi tạo store */
export function configureTokenGetter(getter: () => string | undefined) {
  _getToken = getter;
}

/** Hàm dùng chung để lấy accessToken trong prepareAuthHeaders() */
export function getAccessToken() {
  return _getToken();
}
