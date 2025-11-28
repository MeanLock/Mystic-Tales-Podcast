// src/core/api/appApi/tokens.ts
/** Bạn tự thay thế getter này để lấy token từ store/safe storage của bạn */
let _getToken: () => string | undefined = () => undefined;

export function configureTokenGetter(getter: () => string | undefined) {
  _getToken = getter;
}

export function getAccessToken() {
  return _getToken();
}
