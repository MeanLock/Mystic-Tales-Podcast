// // src/core/api/transformers/fileKeyToUrl.ts
// import { appApi } from "@/core/api/appApi";
// import type { RootState } from "@/redux/store";

// // Duyệt object & thay mọi "*FileKey" → "*Url" bằng cách gọi endpoint getFileUrlByKey (kickoff→Saga)
// export async function replaceFileKeysWithUrls(
//   obj: any,
//   store: { dispatch: any; getState: () => RootState }
// ) {
//   if (!obj || typeof obj !== "object") return obj;

//   const entries = Object.entries(obj);
//   const promises: Promise<void>[] = [];

//   for (const [k, v] of entries) {
//     if (v && typeof v === "object") {
//       promises.push(replaceFileKeysWithUrls(v, store) as any);
//       continue;
//     }

//     if (typeof v === "string" && /FileKey$/i.test(k) && v) {
//       const urlKey = k.replace(/FileKey$/i, "Url");
//       const p = store
//         .dispatch(
//           appApi.endpoints.getFileUrlByKey.initiate(
//             { fileKey: v },
//             { subscribe: false }
//           )
//         )
//         .unwrap()
//         .then((res) => {
//           obj[urlKey] = res?.url;
//         })
//         .catch(() => {
//           // im lặng hoặc log nếu cần
//         });
//       promises.push(p);
//     }
//   }

//   await Promise.all(promises);
//   return obj;
// }
