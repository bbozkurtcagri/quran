package com.bbozkurtcagri.quranmeali.model

import android.content.Context
import com.squareup.moshi.JsonClass
import com.squareup.moshi.Moshi
import com.squareup.moshi.kotlin.reflect.KotlinJsonAdapterFactory

// SharedPreferences-backed reading position — iOS'taki UserDefaults LastReadStore eşdeğeri.
// State tek cihaza özgü; backend yok.
@JsonClass(generateAdapter = false)
data class LastRead(
    val surahNumber: Int,
    val surahNameTurkish: String,
    val verseNumber: Int,
    val savedAt: Long
)

object LastReadStore {
    private const val PREFS = "qc-last-read"
    private const val KEY = "value"

    private val adapter = Moshi.Builder()
        .add(KotlinJsonAdapterFactory())
        .build()
        .adapter(LastRead::class.java)

    fun load(context: Context): LastRead? {
        val raw = context
            .getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .getString(KEY, null)
            ?: return null
        return runCatching { adapter.fromJson(raw) }.getOrNull()
    }

    fun save(context: Context, entry: LastRead) {
        val raw = adapter.toJson(entry)
        context
            .getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .edit()
            .putString(KEY, raw)
            .apply()
    }

    fun clear(context: Context) {
        context
            .getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .edit()
            .remove(KEY)
            .apply()
    }
}
