import { BaseQueryFn } from "@reduxjs/toolkit/query";
import { withAuthMode } from "./baseApi";

// Types for saga results
type SagaStatus = "PENDING" | "SUCCESS" | "FAILURE" | "TIMEOUT";

interface SagaResult<T> {
  status: SagaStatus;
  data: T | null;
  error: string | null;
}

interface SagaPollingOptions<T> {
  sagaId: string;
  baseQuery: BaseQueryFn;
  api: any; // API context from RTK Query
  extraOptions: any; // Extra options from RTK Query
  timeoutSeconds?: number; // Maximum time to wait before giving up
  intervalSeconds?: number; // Time between checks
  extraHeaders?: Record<string, string>; // Any extra headers to include
}

/**
 * Poll a saga orchestrator until it returns a SUCCESS or FAILURE status
 * @param options Polling options including saga ID, base query function, etc.
 * @returns Promise resolving to the saga result
 */

export async function pollSagaResult<T>(
  options: SagaPollingOptions<T>
): Promise<SagaResult<T>> {
  const {
    sagaId,
    baseQuery,
    api,
    extraOptions,
    timeoutSeconds = 60, // Default 1 minute timeout
    intervalSeconds = 2, // Default 2 second interval
    extraHeaders = {},
  } = options;

  // Calculate timeout timestamp
  const timeoutAt = Date.now() + timeoutSeconds * 1000;

  // Default result if something goes wrong
  const defaultResult: SagaResult<T> = {
    status: "FAILURE",
    data: null,
    error: "Unknown error occurred",
  };

  // Poll until timeout or success/failure
  while (Date.now() < timeoutAt) {
    try {
      // Create query args with public auth mode (no token required for saga status check)
      const queryArgs = withAuthMode(
        {
          url: `/api/saga-orchestrator-service/api/orchestration/result-data/${sagaId}`,
          method: "GET",
          headers: extraHeaders,
        },
        { authMode: "public" }
      );

      // Execute the query
      const result = await baseQuery(queryArgs, api, extraOptions);

      // Check for network errors
      if ("error" in result) {
        // console.error("Error polling saga status:", result.error);
        // Continue polling on network errors - the server might be temporarily unavailable
        await new Promise((resolve) =>
          setTimeout(resolve, intervalSeconds * 1000)
        );
        continue;
      }

      // Get the saga status from response (support several shapes)
      const raw = result.data as any;
      const flowStatus: SagaStatus | undefined = raw?.FlowStatus ?? raw?.Status;
      const dataField = raw?.Data ?? raw?.ResultData ?? raw?.Result ?? null;
      const errorField = raw?.Error ?? raw?.ErrorMessage ?? null;

      // Normalize status string (tolerate small typos from orchestrator)
      const statusNorm =
        typeof flowStatus === "string" ? flowStatus.toUpperCase().trim() : "";

      // If flowStatus is success (or contains SUCCESS), try to parse Data/ResultData if it's a JSON string
      if (statusNorm.includes("SUCCESS")) {
        let parsed: any = null;
        if (dataField != null) {
          if (typeof dataField === "string") {
            try {
              parsed = JSON.parse(dataField);
            } catch (e) {
              // If parsing fails, return the raw string
              parsed = dataField;
            }
          } else {
            parsed = dataField;
          }
        }

        return {
          status: "SUCCESS",
          data: parsed || null,
          error: null,
        };
      }

      // Check if saga has failed (or contains FAIL)
      if (typeof statusNorm === "string" && statusNorm.includes("FAIL")) {
        return {
          status: "FAILURE",
          data: null,
          error: errorField || "Saga failed without specific error",
        };
      }

      // If still pending, wait and try again
      await new Promise((resolve) =>
        setTimeout(resolve, intervalSeconds * 1000)
      );
    } catch (error) {
      //   console.error("Exception during saga polling:", error);
      // Continue polling on exceptions - the server might be temporarily unavailable
      await new Promise((resolve) =>
        setTimeout(resolve, intervalSeconds * 1000)
      );
    }
  }

  // If we reach here, we've timed out
  return {
    ...defaultResult,
    error: `Saga polling timed out after ${timeoutSeconds} seconds`,
  };
}
