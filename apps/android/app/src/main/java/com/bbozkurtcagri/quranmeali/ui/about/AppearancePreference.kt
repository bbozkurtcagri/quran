package com.bbozkurtcagri.quranmeali.ui.about

import android.content.Context

// iOS'taki AppearancePreference + AppStorage('qc-appearance') eşdeğeri.
// Boolean? döner: null = sistem, true = koyu, false = açık.

enum class AppearancePreference(val storedValue: String, val label: String) {
    System("system", "Sistem"),
    Light("light", "Açık"),
    Dark("dark", "Koyu");

    fun isDarkOrNull(systemDark: Boolean): Boolean = when (this) {
        System -> systemDark
        Light  -> false
        Dark   -> true
    }

    companion object {
        private const val PREFS = "qc-appearance"
        private const val KEY = "value"

        fun load(context: Context): AppearancePreference {
            val raw = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE).getString(KEY, null)
            return entries.firstOrNull { it.storedValue == raw } ?: System
        }

        fun save(context: Context, pref: AppearancePreference) {
            context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .edit()
                .putString(KEY, pref.storedValue)
                .apply()
        }
    }
}
