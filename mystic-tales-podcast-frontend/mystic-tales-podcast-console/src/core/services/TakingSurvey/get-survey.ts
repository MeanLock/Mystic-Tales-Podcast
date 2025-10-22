import { queryOptions, useQuery } from "@tanstack/react-query";
import type { QueryConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

export const GET_QUERY_KEY = "edit";

const get = async (id: number) => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `/survey?id=${id}`,
  });

  return response.data;
};

export const getTodDoOptions = (id: number) =>
  queryOptions({
    queryKey: [GET_QUERY_KEY],
    queryFn: () => get(id),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getTodDoOptions>;
  id: number;
};

export const useGetSurvey = ({ queryConfig, id }: UseGetType) => {
  return useQuery({
    ...getTodDoOptions(id),
    ...queryConfig,
    enabled: false,
  });
};
