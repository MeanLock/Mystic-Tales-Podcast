import { useEffect, useRef } from "react"
import type { AxiosInstance } from "axios"
import { pollSagaResult, type SagaResult } from "@/core/api/rest-api/main/api-call/saga-polling.helper"

/**
 * Configuration options for the saga polling hook.
 * Provide default lifecycle callbacks & timing configs here.
 */
export interface UseSagaPollingOptions {
  onSuccess?: (data: any) => void
  onFailure?: (error: string) => void
  onTimeout?: () => void
  timeoutSeconds?: number // Thời gian chờ tối đa (giây)
  intervalSeconds?: number // Khoảng thời gian giữa các lần gọi (giây)
  abortRef?: { current: boolean } // Optional external abort ref (shared across multiple pollings)
}

/**
 * Hook to poll saga orchestration status. Supports multiple saga polling calls in a single component.
 * You can override callbacks & timing per call by passing a third parameter to startPolling.
 *
 * Example:
 * const { startPolling } = useSagaPolling({ timeoutSeconds: 30, intervalSeconds: 2 });
 * await startPolling(sagaId, axios, { onSuccess: customHandler });
 */
export function useSagaPolling(baseOptions?: UseSagaPollingOptions) {
  // Internal abortRef used if caller does not provide one.
  const internalAbortRef = useRef(false)
  const activeAbortRef = baseOptions?.abortRef ?? internalAbortRef

  // Auto-abort on unmount to prevent memory leaks.
  useEffect(() => {
    return () => {
      activeAbortRef.current = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  /**
   * Start polling a saga until success / failure / timeout.
   * Per-call overrides are shallow merged over base options.
   * Returns the raw SagaResult so callers can do additional logic.
   */
  const startPolling = async (
    sagaId: string,
    axiosInstance: AxiosInstance,
    overrides?: Partial<UseSagaPollingOptions>
  ): Promise<SagaResult> => {
    const merged: UseSagaPollingOptions = {
      ...baseOptions,
      ...overrides,
      // Preserve external abortRef if provided in overrides or baseOptions, else internal
      abortRef: overrides?.abortRef || baseOptions?.abortRef || activeAbortRef,
    }

    const result: SagaResult = await pollSagaResult({
      sagaId,
      axiosInstance,
      timeoutSeconds: merged.timeoutSeconds,
      intervalSeconds: merged.intervalSeconds,
      abortRef: merged.abortRef,
    })

    if (result.status === "SUCCESS") merged.onSuccess?.(result.data)
    else if (result.status === "FAILED") merged.onFailure?.(result.error || "")
    else merged.onTimeout?.()

    return result
  }

  return { startPolling, abortRef: activeAbortRef }
}
