import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import type { QueryConfig } from "@/lib/query";
import { queryOptions, useQuery } from "@tanstack/react-query";

export const GET_SURVEY_SPECIFIC_TOPICS_QUERY_KEY = "survey-specific-topics";

const get = async () => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `Survey/core/specific-topics`,
  });
  return response.data;
};

export const getOptions = () =>
  queryOptions({
    queryKey: [GET_SURVEY_SPECIFIC_TOPICS_QUERY_KEY],
    queryFn: () => get(),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getOptions>;
};

export const useGetSurveySpecificTopics = ({ queryConfig }: UseGetType) => {
  return useQuery({
    ...getOptions(),
    ...queryConfig,
  });
};
