import type {
  ApiResponse,
  PagedResult,
  SurahDetail,
  SurahListItem,
  VerseSearchHit,
  VerseSummary,
} from "./types";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5185";

/** Default Turkish meal source for Phase 1. */
export const DEFAULT_TRANSLATION_CODE = "elmalili";

export class ApiClientError extends Error {
  readonly status: number;
  readonly code?: string;

  constructor(status: number, message: string, code?: string) {
    super(message);
    this.name = "ApiClientError";
    this.status = status;
    this.code = code;
  }
}

function abortError(): DOMException {
  return new DOMException("The request was aborted.", "AbortError");
}

async function apiGet<T>(path: string, signal?: AbortSignal): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, { signal });

  // The signal may have fired between headers arriving and the body being
  // consumed; treat that the same as a local abort.
  if (signal?.aborted) {
    throw abortError();
  }

  // 499 = Client Closed Request — our server writes this when it sees an
  // OperationCanceledException. Hits when a fetch is aborted late (after the
  // request reached the server). Surface as an abort so callers don't render
  // an error UI for cancelled work.
  if (res.status === 499) {
    throw abortError();
  }

  let body: ApiResponse<T> | null = null;
  try {
    body = (await res.json()) as ApiResponse<T>;
  } catch (err) {
    if (signal?.aborted || (err instanceof DOMException && err.name === "AbortError")) {
      throw abortError();
    }
    // Surface the parser's message so we can tell a network truncation from a
    // genuine syntax error in the response.
    const detail = err instanceof Error ? err.message : String(err);
    console.error("apiGet body parse failed", { path, status: res.status, detail, err });
    throw new ApiClientError(res.status, `Invalid response body (${res.status}): ${detail}`);
  }

  if (!res.ok || !body?.success) {
    const firstError = body?.errors?.[0];
    throw new ApiClientError(
      res.status,
      firstError?.message ?? body?.message ?? `Request failed (${res.status})`,
      firstError?.code,
    );
  }

  return body.data as T;
}

export function getSurahs(signal?: AbortSignal): Promise<SurahListItem[]> {
  return apiGet<SurahListItem[]>("/api/v1/surahs", signal);
}

export function getSurahDetail(
  surahNumber: number,
  signal?: AbortSignal,
): Promise<SurahDetail> {
  return apiGet<SurahDetail>(`/api/v1/surahs/${surahNumber}`, signal);
}

export function getSurahVerses(
  surahNumber: number,
  translationSourceCode: string = DEFAULT_TRANSLATION_CODE,
  page: number = 1,
  pageSize: number = 300,
  signal?: AbortSignal,
): Promise<PagedResult<VerseSummary>> {
  const params = new URLSearchParams({
    translationSourceCode,
    page: String(page),
    pageSize: String(pageSize),
  });
  return apiGet<PagedResult<VerseSummary>>(
    `/api/v1/surahs/${surahNumber}/verses?${params.toString()}`,
    signal,
  );
}

export type SearchMode = "keyword" | "semantic";

export function searchVerses(
  query: string,
  page: number = 1,
  pageSize: number = 20,
  translationSourceCode: string = DEFAULT_TRANSLATION_CODE,
  mode: SearchMode = "keyword",
  signal?: AbortSignal,
): Promise<PagedResult<VerseSearchHit>> {
  const params = new URLSearchParams({
    query,
    translationSourceCode,
    page: String(page),
    pageSize: String(pageSize),
    mode,
  });
  return apiGet<PagedResult<VerseSearchHit>>(
    `/api/v1/search?${params.toString()}`,
    signal,
  );
}
