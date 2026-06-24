package com.bbozkurtcagri.quranmeali.api

import com.squareup.moshi.JsonClass

// Backend her cevabı bu zarfa sarıyor; başarıda `data`, hatada `errors` dolar.
@JsonClass(generateAdapter = false)
data class ApiResponse<T>(
    val data: T?,
    val success: Boolean,
    val message: String?,
    val errors: List<ApiErrorField>?
)

@JsonClass(generateAdapter = false)
data class ApiErrorField(
    val field: String?,
    val message: String
)

// UI tarafındaki when bloklarının domain'i — HTTP status'a göre mapler.
sealed class ApiException(message: String) : RuntimeException(message) {
    class NotFound(message: String) : ApiException(message)
    class Validation(message: String, val field: String?) : ApiException(message)
    class Conflict(message: String) : ApiException(message)
    class Server(val status: Int, message: String) : ApiException(message)
    class Transport(cause: Throwable) : ApiException(cause.message ?: "Bağlantı hatası") {
        init { initCause(cause) }
    }
}
