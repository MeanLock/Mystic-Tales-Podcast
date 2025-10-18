import {
  MutationCache,
  QueryCache,
  QueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { AxiosError, HttpStatusCode, isAxiosError } from "axios";
import { toast } from "react-toastify";

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (isAxiosError(error)) {
        handleGlobalAxiosError(error);
      }
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      if (isAxiosError(error)) {
        handleGlobalAxiosError(error);
      }
    },
  }),
  defaultOptions: {
    queries: {
      retry: 0,
    },
  },
});

const handleGlobalAxiosError = (error: AxiosError) => {
  if (error.status === HttpStatusCode.Unauthorized) {
    toast.error("Bạn cần đăng nhập để có thể thực hiện hành động này!");
    window.location.href = "/login";
  } else if (error.status === HttpStatusCode.Forbidden) {
    toast.error("Bạn không đủ quyền hạn để thực hiện hành động này!");
  }
};

export default queryClient;

export type ApiFnReturnType<FnType extends (...args: any) => Promise<any>> =
  Awaited<ReturnType<FnType>>;

export type QueryConfig<T extends (...args: any[]) => any> = Omit<
  ReturnType<T>,
  "queryKey" | "queryFn"
>;

export type MutationConfig<
  MutationFnType extends (...args: any) => Promise<any>
> = UseMutationOptions<
  ApiFnReturnType<MutationFnType>, // return type
  Error, // error type
  Parameters<MutationFnType>[0] // variables type
>;
