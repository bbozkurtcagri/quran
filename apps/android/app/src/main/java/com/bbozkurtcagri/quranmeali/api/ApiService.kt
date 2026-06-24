package com.bbozkurtcagri.quranmeali.api

import com.bbozkurtcagri.quranmeali.model.PagedResult
import com.bbozkurtcagri.quranmeali.model.SurahDetail
import com.bbozkurtcagri.quranmeali.model.SurahListItem
import com.bbozkurtcagri.quranmeali.model.VerseSearchHit
import com.bbozkurtcagri.quranmeali.model.VerseSummary
import retrofit2.http.GET
import retrofit2.http.Path
import retrofit2.http.Query

// Retrofit suspend interface — her endpoint envelope sarmalı döner; unwrap
// edilmesi ApiClient.unwrap helper'ında yapılır.
interface ApiService {

    @GET("api/v1/surahs")
    suspend fun getSurahs(): ApiResponse<List<SurahListItem>>

    @GET("api/v1/surahs/{number}")
    suspend fun getSurahDetail(@Path("number") number: Int): ApiResponse<SurahDetail>

    @GET("api/v1/surahs/{number}/verses")
    suspend fun getSurahVerses(
        @Path("number") number: Int,
        @Query("translationSourceCode") translationSourceCode: String = "elmalili",
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 300
    ): ApiResponse<PagedResult<VerseSummary>>

    @GET("api/v1/search")
    suspend fun searchVerses(
        @Query("query") query: String,
        @Query("mode") mode: String = "semantic",       // "semantic" | "keyword"
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("translationSourceCode") translationSourceCode: String = "elmalili"
    ): ApiResponse<PagedResult<VerseSearchHit>>
}
