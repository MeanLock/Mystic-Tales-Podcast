import { useMutation } from "@tanstack/react-query";
import type { MutationConfig } from "@/lib/mutation";
import type { SurveyType } from "@/core/types/tools";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
type PayLoadType = SurveyType & {
  SurveyId: number;
};

const update = async (payload: PayLoadType) => {
  try {
    const { SurveyId, ...body } = payload;
    const response = await callAxiosRestApi({
      instance: loginRequiredAxiosInstance,
      method: "put",
      url: `Survey/session/surveys/${SurveyId}/editing-auto-trigger`,
      data: {
        EditingSession: body,
      },
    });
    return {
      responseData: response.data,
      sentBody: body,
    };
  } catch (error) {
    throw error;
  }
};

type UpdateType = {
  mutationConfig?: MutationConfig<typeof update>;
};

export const useUpdateSurvey = ({ mutationConfig }: UpdateType) => {
  return useMutation({
    ...mutationConfig,
    mutationFn: update,
  });
};
