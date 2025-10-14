import { useEffect, useRef } from "react"
import { AxiosInstance } from "axios"
import { pollSagaResult, SagaResult } from "@/core/api/rest-api/main/api-call/saga-polling.helper"

export interface UseSagaPollingOptions {
  onSuccess?: (data: any) => void
  onFailure?: (error: string) => void
  onTimeout?: () => void
}

export function useSagaPolling(options?: UseSagaPollingOptions) {
  const abortRef = useRef(false)

  useEffect(() => {
    return () => {
      abortRef.current = true
    }
  }, [])

  const startPolling = async (sagaId: string, axiosInstance: AxiosInstance) => {
    const result: SagaResult = await pollSagaResult({
      sagaId,
      axiosInstance,
      abortRef,
    })

    if (result.status === "SUCCESS") options?.onSuccess?.(result.data)
    else if (result.status === "FAILED") options?.onFailure?.(result.error || "")
    else options?.onTimeout?.()
  }

  return { startPolling }
}
