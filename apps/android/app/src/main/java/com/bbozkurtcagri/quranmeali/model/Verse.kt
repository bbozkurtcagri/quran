package com.bbozkurtcagri.quranmeali.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = false)
data class Translation(
    val sourceCode: String,
    val sourceName: String,
    val languageCode: String,
    val author: String,
    val text: String
)

@JsonClass(generateAdapter = false)
data class VerseSummary(
    val surahNumber: Int,
    val verseNumber: Int,
    val globalVerseNumber: Int,
    val juzNumber: Int,
    val pageNumber: Int,
    val arabicText: String,
    val translations: List<Translation>
) {
    // Backend ilk sırada default kaynağı koyuyor; UI'da bu birinci stiline güvenir.
    val primaryTranslation: Translation? get() = translations.firstOrNull()
}

@JsonClass(generateAdapter = false)
data class VerseSearchHit(
    val surahNumber: Int,
    val surahNameTurkish: String,
    val verseNumber: Int,
    val globalVerseNumber: Int,
    val arabicText: String,
    val translationSourceCode: String,
    val translationText: String
)

@JsonClass(generateAdapter = false)
data class PagedResult<T>(
    val items: List<T>,
    val totalCount: Long,
    val page: Int,
    val pageSize: Int,
    val totalPages: Int,
    val hasNextPage: Boolean,
    val hasPreviousPage: Boolean
)
