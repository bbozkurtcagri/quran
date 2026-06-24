package com.bbozkurtcagri.quranmeali.ui.surahlist

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.bbozkurtcagri.quranmeali.api.ApiClient
import com.bbozkurtcagri.quranmeali.api.ApiException
import com.bbozkurtcagri.quranmeali.model.SurahListItem
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.text.Normalizer
import java.util.Locale

data class SurahListUiState(
    val surahs: List<SurahListItem> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null,
    val filterText: String = ""
) {
    val filtered: List<SurahListItem>
        get() {
            val needle = SurahListViewModel.normalize(filterText)
            if (needle.isEmpty()) return surahs
            return surahs.filter { s ->
                val haystacks = listOf(
                    SurahListViewModel.normalize(s.nameTurkish),
                    SurahListViewModel.normalize(s.nameTransliteration),
                    s.nameArabic,
                    s.number.toString(),
                    "%02d".format(s.number)
                )
                haystacks.any { it.contains(needle) }
            }
        }
}

class SurahListViewModel : ViewModel() {
    private val _state = MutableStateFlow(SurahListUiState())
    val state: StateFlow<SurahListUiState> = _state.asStateFlow()

    fun load() {
        if (_state.value.isLoading || _state.value.surahs.isNotEmpty()) return
        _state.update { it.copy(isLoading = true, error = null) }
        viewModelScope.launch {
            try {
                val list = ApiClient.unwrap { ApiClient.service.getSurahs() }
                _state.update { it.copy(surahs = list, isLoading = false) }
            } catch (e: ApiException) {
                _state.update { it.copy(error = e.message, isLoading = false) }
            } catch (e: Exception) {
                _state.update { it.copy(error = e.message ?: "Bilinmeyen hata", isLoading = false) }
            }
        }
    }

    fun retry() {
        _state.update { it.copy(surahs = emptyList(), error = null) }
        load()
    }

    fun setFilter(text: String) {
        _state.update { it.copy(filterText = text) }
    }

    companion object {
        // Türkçe-bilinçli normalize — web tarafıyla aynı kural.
        // "İ"→"i", "ı"→"i", diakritik temizliği, harf+rakam dışını boşluğa çevir.
        fun normalize(input: String): String {
            val locale = Locale("tr", "TR")
            val lower = input.lowercase(locale)
            val folded = Normalizer.normalize(lower, Normalizer.Form.NFD)
                .replace(Regex("\\p{InCombiningDiacriticalMarks}+"), "")
            val sb = StringBuilder()
            for (ch in folded) {
                when {
                    ch == 'ı' || ch == 'İ' -> sb.append('i')
                    ch.isLetterOrDigit() -> sb.append(ch)
                    ch.isWhitespace() -> sb.append(' ')
                }
            }
            return sb.toString().trim()
        }
    }
}
