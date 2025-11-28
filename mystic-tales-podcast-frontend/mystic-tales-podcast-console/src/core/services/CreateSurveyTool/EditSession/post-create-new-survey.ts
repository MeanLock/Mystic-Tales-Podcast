import { useMutation, type UseMutationOptions } from "@tanstack/react-query";
import { mutationOptions, type MutationConfig } from "@/lib/mutation";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

const createSurvey = async () => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "post",
    url: `Survey/core/filter/surveys`,
  });
  return response.data;
};

type CreateSurveyResponse = {
  NewSurveyId: number,
  Message: string
};

type UseCreateSurveyProps = {
  mutationConfig?: UseMutationOptions<
    CreateSurveyResponse, // data
    Error, // error
    void // variables (vì không truyền gì)
  >;
};

export const useCreateSurvey = ({
  mutationConfig,
}: UseCreateSurveyProps = {}) => {
  return useMutation<CreateSurveyResponse, Error, void>({
    mutationFn: createSurvey,
    ...mutationConfig,
  });
};
