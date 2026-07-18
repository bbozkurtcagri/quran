//
//  ApiClient.swift
//  QuranMeali
//
//  URLSession-based client for the QuranCompanion backend. Mirrors the
//  envelope handling in apps/web/src/api/client.ts: unwraps `data`,
//  translates HTTP 499 into a cancellation so SwiftUI's task-cancellation
//  semantics line up cleanly.
//

import Foundation

// MARK: - Errors

enum ApiError: Error, LocalizedError {
    case notFound(message: String)
    case validation(message: String, field: String?)
    case conflict(message: String)
    case server(status: Int, message: String)
    case invalidResponse(detail: String)
    case transport(underlying: Error)

    var errorDescription: String? {
        switch self {
        case .notFound(let message): return message
        case .validation(let message, _): return message
        case .conflict(let message): return message
        case .server(_, let message): return message
        case .invalidResponse(let detail): return "Geçersiz yanıt: \(detail)"
        case .transport(let underlying): return underlying.localizedDescription
        }
    }
}

// MARK: - Search mode

enum SearchMode: String {
    case keyword
    case semantic
}

// MARK: - Client

final class ApiClient: @unchecked Sendable {

    nonisolated(unsafe) static let shared = ApiClient()

    /// DEBUG builds default to the local dev API (simulator ↔ host Mac via
    /// `localhost`; on a real device you'd override to a LAN IP like
    /// `http://192.168.1.42:8085`). RELEASE builds hit prod over HTTPS.
    /// Init `baseURL:` still lets you override for tests / one-off configs.
    private let baseURL: URL

    private let session: URLSession
    private let decoder: JSONDecoder

    nonisolated private static let defaultTranslationSource = "elmalili"

    #if DEBUG
    nonisolated private static let defaultBaseURL = URL(string: "http://localhost:8085")!
    #else
    nonisolated private static let defaultBaseURL = URL(string: "https://api.kuranmeali.app")!
    #endif

    init(baseURL: URL = ApiClient.defaultBaseURL,
         session: URLSession = .shared)
    {
        self.baseURL = baseURL
        self.session = session
        self.decoder = JSONDecoder()
    }

    // MARK: Public endpoints

    func getSurahs() async throws -> [SurahListItem] {
        try await get("/api/v1/surahs", as: [SurahListItem].self)
    }

    func getSurahDetail(number: Int) async throws -> SurahDetail {
        try await get("/api/v1/surahs/\(number)", as: SurahDetail.self)
    }

    func getSurahVerses(
        number: Int,
        translationSourceCode: String = defaultTranslationSource,
        page: Int = 1,
        pageSize: Int = 300
    ) async throws -> PagedResult<VerseSummary> {
        var components = URLComponents(string: "/api/v1/surahs/\(number)/verses")!
        components.queryItems = [
            URLQueryItem(name: "translationSourceCode", value: translationSourceCode),
            URLQueryItem(name: "page", value: String(page)),
            URLQueryItem(name: "pageSize", value: String(pageSize)),
        ]
        return try await get(components.string!, as: PagedResult<VerseSummary>.self)
    }

    func searchVerses(
        query: String,
        mode: SearchMode = .semantic,
        page: Int = 1,
        pageSize: Int = 20,
        translationSourceCode: String = defaultTranslationSource
    ) async throws -> PagedResult<VerseSearchHit> {
        var components = URLComponents(string: "/api/v1/search")!
        components.queryItems = [
            URLQueryItem(name: "query", value: query),
            URLQueryItem(name: "mode", value: mode.rawValue),
            URLQueryItem(name: "translationSourceCode", value: translationSourceCode),
            URLQueryItem(name: "page", value: String(page)),
            URLQueryItem(name: "pageSize", value: String(pageSize)),
        ]
        return try await get(components.string!, as: PagedResult<VerseSearchHit>.self)
    }

    // MARK: Generic GET

    private func get<T: Decodable>(_ path: String, as type: T.Type) async throws -> T {
        let url = baseURL.appendingPathComponent(path)
        // Use absolute path components when present (queries break appendingPathComponent).
        let finalURL: URL
        if path.contains("?") {
            finalURL = URL(string: baseURL.absoluteString + path)!
        } else {
            finalURL = url
        }

        let request = URLRequest(url: finalURL)

        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await session.data(for: request)
        } catch is CancellationError {
            throw CancellationError()
        } catch {
            throw ApiError.transport(underlying: error)
        }

        guard let http = response as? HTTPURLResponse else {
            throw ApiError.invalidResponse(detail: "Beklenen HTTP yanıtı alınamadı.")
        }

        // 499 — server saw our request was cancelled, treat the same as a local abort.
        if http.statusCode == 499 {
            throw CancellationError()
        }

        // Try to decode the envelope regardless of status; the backend uses
        // it for both success and failure shapes.
        let envelope: ApiResponse<T>
        do {
            envelope = try decoder.decode(ApiResponse<T>.self, from: data)
        } catch {
            throw ApiError.invalidResponse(detail: error.localizedDescription)
        }

        if envelope.success, let payload = envelope.data {
            return payload
        }

        let firstError = envelope.errors?.first
        let message = firstError?.message ?? envelope.message ?? "İstek başarısız (\(http.statusCode))"

        switch http.statusCode {
        case 404: throw ApiError.notFound(message: message)
        case 400: throw ApiError.validation(message: message, field: firstError?.field)
        case 409: throw ApiError.conflict(message: message)
        default: throw ApiError.server(status: http.statusCode, message: message)
        }
    }
}
