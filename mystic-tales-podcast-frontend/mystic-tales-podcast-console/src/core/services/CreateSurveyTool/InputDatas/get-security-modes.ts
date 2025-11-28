
import { queryOptions, useQuery } from "@tanstack/react-query";
import type { QueryConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

export const GET_SURVEY_SECURITY_MODE_QUERY_KEY = "survey-security-modes";

const get = async () => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `Survey/core/security-modes`
  })
  return response.data;
};

export const getOptions = () =>
  queryOptions({
    queryKey: [GET_SURVEY_SECURITY_MODE_QUERY_KEY],
    queryFn: () => get(),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getOptions>;
};

export const useGetSurveySecurityModes = ({ queryConfig }: UseGetType) => {
  return useQuery({
    ...getOptions(),
    ...queryConfig,
  });
};
