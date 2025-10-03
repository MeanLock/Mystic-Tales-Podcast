import { queryOptions, useQuery } from "@tanstack/react-query";
import type { QueryConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

export const GET_SURVEY_QUESTION_TYPE_QUERY_KEY = "survey-question-types";

const get = async () => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `Survey/core/survey-question-types`
  });
  return response.data;
};

export const getOptions = () =>
  queryOptions({
    queryKey: [GET_SURVEY_QUESTION_TYPE_QUERY_KEY],
    queryFn: () => get(),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getOptions>;
};

export const useGetSurveyQuestionTypes = ({ queryConfig }: UseGetType) => {
  return useQuery({
    ...getOptions(),
    ...queryConfig,
  });
};
