import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import type { QueryConfig } from "@/lib/query";
import { queryOptions, useQuery } from "@tanstack/react-query";

export const GET_SURVEY_TOPICS_QUERY_KEY = "survey-topics";

const get = async () => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `Survey/core/topics`,
  });
  return response.data;
};

export const getOptions = () =>
  queryOptions({
    queryKey: [GET_SURVEY_TOPICS_QUERY_KEY],
    queryFn: () => get(),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getOptions>;
};

export const useGetSurveyTopics = ({ queryConfig }: UseGetType) => {
  return useQuery({
    ...getOptions(),
    ...queryConfig,
  });
};
