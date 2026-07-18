package com.bbozkurtcagri.quranmeali.api

import com.bbozkurtcagri.quranmeali.BuildConfig
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.moshi.MoshiConverterFactory
import retrofit2.HttpException
import java.io.IOException
import java.util.concurrent.TimeUnit

// Tek tip singleton — Compose tarafı doğrudan ApiClient.service.getSurahs() çağırır.
// Debug: emulator'dan host makineye 10.0.2.2 ile ulaşılır (iOS'taki localhost:8085 eşdeğeri).
// Release: prod API üzerinden HTTPS. BuildConfig.DEBUG derleme zamanında Gradle build type'a göre set edilir.
object ApiClient {
    private val BASE_URL: String = if (BuildConfig.DEBUG) {
        "http://10.0.2.2:8085/"
    } else {
        "https://api.kuranmeali.app/"
    }

    private val moshi: Moshi = Moshi.Builder()
        .add(KotlinJsonAdapterFactory())
        .build()

    private val okHttp: OkHttpClient = OkHttpClient.Builder()
        .connectTimeout(10, TimeUnit.SECONDS)
        .readTimeout(30, TimeUnit.SECONDS)
        .addInterceptor(HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BASIC
        })
        .build()

    val service: ApiService = Retrofit.Builder()
        .baseUrl(BASE_URL)
        .client(okHttp)
        .addConverterFactory(MoshiConverterFactory.create(moshi))
        .build()
        .create(ApiService::class.java)

    /// Envelope'u açar; success ise data'yı, değilse uygun ApiException'ı fırlatır.
    /// HTTP 4xx/5xx için HttpException → status-bazlı mapping.
    suspend fun <T> unwrap(call: suspend () -> ApiResponse<T>): T {
        val response = try {
            call()
        } catch (e: HttpException) {
            throw mapHttp(e)
        } catch (e: IOException) {
            throw ApiException.Transport(e)
        }
        if (response.success && response.data != null) {
            return response.data
        }
        val firstError = response.errors?.firstOrNull()
        val message = firstError?.message ?: response.message ?: "İstek başarısız oldu."
        throw ApiException.Server(status = 0, message = message)
    }

    private fun mapHttp(e: HttpException): ApiException {
        val raw = runCatching { e.response()?.errorBody()?.string() }.getOrNull()
        val parsed = raw?.let { runCatching {
            moshi.adapter(ApiResponse::class.java).fromJson(it)
        }.getOrNull() }

        val firstError = parsed?.errors?.firstOrNull()
        val msg = firstError?.message ?: parsed?.message ?: e.message() ?: "Sunucu hatası"
        return when (e.code()) {
            404 -> ApiException.NotFound(msg)
            400 -> ApiException.Validation(msg, firstError?.field)
            409 -> ApiException.Conflict(msg)
            else -> ApiException.Server(e.code(), msg)
        }
    }
}
