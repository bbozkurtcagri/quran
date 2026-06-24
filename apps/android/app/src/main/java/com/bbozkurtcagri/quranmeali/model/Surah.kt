package com.bbozkurtcagri.quranmeali.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = false)
data class SurahListItem(
    val number: Int,
    val nameArabic: String,
    val nameTurkish: String,
    val nameTransliteration: String,
    val verseCount: Int,
    val revelationPlace: RevelationPlace
)

@JsonClass(generateAdapter = false)
data class SurahDetail(
    val number: Int,
    val nameArabic: String,
    val nameTurkish: String,
    val nameTransliteration: String,
    val verseCount: Int,
    val revelationPlace: RevelationPlace
)

// Backend enum'u "Meccan" / "Medinan" / "Unknown" string'leri olarak gönderiyor —
// UI'da "Mekkî" / "Medenî" gösterilir; harita aşağıda.
enum class RevelationPlace(val turkishLabel: String) {
    Meccan("Mekkî"),
    Medinan("Medenî"),
    Unknown("—")
}
