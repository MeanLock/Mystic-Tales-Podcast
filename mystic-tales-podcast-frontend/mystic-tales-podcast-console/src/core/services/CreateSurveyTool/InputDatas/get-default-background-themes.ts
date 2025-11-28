
import { queryOptions, useQuery } from "@tanstack/react-query";
import type { QueryConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

export const GET_SURVEY_DEFAULT_BACKGROUND_THEMES_QUERY_KEY =
  "survey-default-background-themes";

const get = async () => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `Survey/core/survey-default-background-themes`
  })
  return response.data.SurveyDefaultBackgroundThemes;
};

export const getOptions = () =>
  queryOptions({
    queryKey: [GET_SURVEY_DEFAULT_BACKGROUND_THEMES_QUERY_KEY],
    queryFn: () => get(),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getOptions>;
};

export const useGetSurveyDefaultBackgroundThemes = ({
  queryConfig,
}: UseGetType) => {
  return useQuery({
    ...getOptions(),
    ...queryConfig,
  });
};
