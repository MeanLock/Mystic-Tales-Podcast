import { useMutation } from "@tanstack/react-query";

import type { MutationConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
type PayLoadType = {
  // taken_subject: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
} & any;

const update = async (payload: PayLoadType) => {
  // const { taken_subject, req } = payload;
  const dataClone = { ...payload };
  delete dataClone.taken_subject;
  delete dataClone.surveyId;
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "post",
    url: `Survey/response/filter/surveys/${payload.surveyId}?taken_subject=${payload.taken_subject}`,
    data: dataClone,
  });
  //console.log("dataClone: ", dataClone);
  return response.data;
};

type UpdateType = {
  mutationConfig?: MutationConfig<typeof update>;
};

export const useUpdateSurveyPro = ({ mutationConfig }: UpdateType) => {
  return useMutation({
    ...mutationConfig,
    mutationFn: update,
  });
};
