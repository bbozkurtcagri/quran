package com.bbozkurtcagri.quranmeali.ui.search

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.bbozkurtcagri.quranmeali.api.ApiClient
import com.bbozkurtcagri.quranmeali.api.ApiException
import com.bbozkurtcagri.quranmeali.model.VerseSearchHit
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

enum class SearchMode(val wire: String) { Semantic("semantic"), Keyword("keyword") }

data class SearchUiState(
    val query: String = "",
    val mode: SearchMode = SearchMode.Semantic,
    val results: List<VerseSearchHit> = emptyList(),
    val totalCount: Long = 0L,
    val isLoading: Boolean = false,
    val error: String? = null
)

class SearchViewModel : ViewModel() {
    private val _state = MutableStateFlow(SearchUiState())
    val state: StateFlow<SearchUiState> = _state.asStateFlow()

    private var debounceJob: Job? = null
    private var fetchJob: Job? = null

    fun onQueryChanged(value: String) {
        _state.update { it.copy(query = value) }
        debounceJob?.cancel()
        debounceJob = viewModelScope.launch {
            delay(300)
            fire(value, _state.value.mode)
        }
    }

    fun onModeChanged(mode: SearchMode) {
        _state.update { it.copy(mode = mode) }
        debounceJob?.cancel()
        // Mod değişiminde anında yeniden sorgula.
        fire(_state.value.query, mode)
    }

    private fun fire(query: String, mode: SearchMode) {
        val trimmed = query.trim()
        if (trimmed.length < 2) {
            fetchJob?.cancel()
            _state.update { it.copy(results = emptyList(), totalCount = 0, error = null, isLoading = false) }
            return
        }
        fetchJob?.cancel()
        _state.update { it.copy(isLoading = true, error = null) }
        fetchJob = viewModelScope.launch {
            try {
                val paged = ApiClient.unwrap {
                    ApiClient.service.searchVerses(
                        query = trimmed,
                        mode = mode.wire,
                        page = 1,
                        pageSize = 20
                    )
                }
                _state.update { it.copy(results = paged.items, totalCount = paged.totalCount, isLoading = false) }
            } catch (e: ApiException) {
                _state.update { it.copy(error = e.message, isLoading = false, results = emptyList(), totalCount = 0) }
            } catch (e: Exception) {
                _state.update { it.copy(error = e.message ?: "Bilinmeyen hata", isLoading = false, results = emptyList(), totalCount = 0) }
            }
        }
    }
}
