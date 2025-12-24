// debounce đơn giản cho async function
export function debouncePromise<T extends (...args: any[]) => Promise<any>>(
  func: T,
  wait: number
): (...args: Parameters<T>) => Promise<ReturnType<T>> {
  let timer: ReturnType<typeof setTimeout> | null = null;
  let lastCallId = 0;

  return (...args: Parameters<T>): Promise<ReturnType<T>> => {
    lastCallId += 1;
    const callId = lastCallId;

    if (timer) {
      clearTimeout(timer);
    }

    return new Promise<ReturnType<T>>((resolve, reject) => {
      timer = setTimeout(async () => {
        try {
          const result = await func(...args);
          // Only resolve if this is the latest call
          if (callId === lastCallId) {
            resolve(result);
          } else {
            reject(new Error("Cancelled by newer call"));
          }
        } catch (err) {
          if (callId === lastCallId) {
            reject(err);
          } else {
            reject(new Error("Cancelled by newer call"));
          }
        }
      }, wait);
    });
  };
}
