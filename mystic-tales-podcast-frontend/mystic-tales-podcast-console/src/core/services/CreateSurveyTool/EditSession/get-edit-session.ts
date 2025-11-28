import { queryOptions, useQuery } from "@tanstack/react-query";
import type { QueryConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

export const GET_QUERY_KEY = "get-edit-session";

// API fetcher
const get = async (id: number) => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `Survey/session/surveys/${id}/editing-session`,
  });
  return response.data;
};

// Query Options
export const getEditSessionOptions = (id: number) =>
  queryOptions({
    queryKey: [GET_QUERY_KEY, id],
    queryFn: () => get(id),
  });

// Hook
type UseGetType = {
  //   queryConfig?: QueryConfig<ReturnType<typeof getEditSessionOptions>>;
  queryConfig?: QueryConfig<Awaited<ReturnType<typeof get>>>;
  id: number;
};

export const useGetEditSession = ({ queryConfig, id }: UseGetType) => {
  return useQuery({
    ...getEditSessionOptions(id),
    ...queryConfig,
    enabled: !!id && (queryConfig?.enabled ?? true), // giữ enabled trong queryConfig nếu có
  });
};
