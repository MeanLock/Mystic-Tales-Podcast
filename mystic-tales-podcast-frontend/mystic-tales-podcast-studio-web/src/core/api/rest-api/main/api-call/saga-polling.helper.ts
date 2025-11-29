import { AxiosInstance } from "axios"

export type SagaResult = {
  status: "SUCCESS" | "FAILED" | "TIMEOUT"
  data?: any
  error?: string
}

const parseSagaResultData = (raw: any) => {
  if (!raw) return null
  if (typeof raw === "object") return raw
  if (typeof raw === "string") {
    try {
      return JSON.parse(raw.replace(/\r\n/g, ""))
    } catch {
      return null
    }
  }
  return null
}

/**
 * Poll saga orchestrator until success, failure, or timeout
 */
export async function pollSagaResult({
  sagaId,
  axiosInstance,
  timeoutSeconds = 5, // Mặc định gọi trong 5 giây
  intervalSeconds = 0.5, // Mặc định 0.5 giây mỗi lần gọi
  abortRef,
}: {
  sagaId: string
  axiosInstance: AxiosInstance
  timeoutSeconds?: number
  intervalSeconds?: number
  abortRef?: { current: boolean }
}): Promise<SagaResult> {

  const startTime = Date.now()
  const timeoutMs = timeoutSeconds * 1000
  const intervalMs = intervalSeconds * 1000
while (Date.now() - startTime < timeoutMs) {

    if (abortRef?.current) return { status: "TIMEOUT", error: "Aborted" }

    try {
      const res = await axiosInstance.get(
        `/saga-orchestrator-service/api/orchestration/result-data/${sagaId}`
      )

      const flowStatus = res.data?.FlowStatus
      const parsed = parseSagaResultData(res.data?.ResultData)

      if (flowStatus === "SUCCESS") return { status: "SUCCESS", data: parsed }
      if (flowStatus === "FAILED")
        return { status: "FAILED", error: parsed?.ErrorMessage || "Saga failed" }
    } catch {
      // ignore errors, retry later
    }

    await new Promise((resolve) => setTimeout(resolve, intervalMs))
  }

  return { status: "TIMEOUT", error: "Saga polling timeout" }
}
