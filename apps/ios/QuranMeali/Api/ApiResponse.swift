//
//  ApiResponse.swift
//  QuranMeali
//
//  Envelope returned by every QuranCompanion endpoint.
//  { "data": T?, "success": bool, "message": string?, "errors": ApiError[] }
//

import Foundation

struct ApiResponse<T: Decodable>: Decodable {
    let data: T?
    let success: Bool
    let message: String?
    let errors: [ApiResponseError]?
}

struct ApiResponseError: Decodable {
    let code: String
    let message: String
    let field: String?
}
