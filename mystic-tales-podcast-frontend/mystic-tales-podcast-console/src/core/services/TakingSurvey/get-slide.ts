import { queryOptions, useQuery } from "@tanstack/react-query";
import type { QueryConfig } from "@/lib/query";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

export const GET_QUERY_KEY = "slide";

const get = async (id: number, taking_subject: string) => {
  const response = await callAxiosRestApi({
    instance: loginRequiredAxiosInstance,
    method: "get",
    url: `/Survey/session/surveys/${id}/taking-session?id=${id}&taking_subject=${taking_subject}`
  })
  return response.data.TakingSession;
};

export const getTodDoOptions = (id: number, taking_subject: string) =>
  queryOptions({
    queryKey: [GET_QUERY_KEY],
    queryFn: () => get(id, taking_subject),
  });

type UseGetType = {
  queryConfig?: QueryConfig<typeof getTodDoOptions>;
  id: number;
  taking_subject: string;
};

export const useGetSlide = ({
  queryConfig,
  id,
  taking_subject,
}: UseGetType) => {
  return useQuery({
    ...getTodDoOptions(id, taking_subject),
    ...queryConfig,
    enabled: Boolean(id),
  });
};
