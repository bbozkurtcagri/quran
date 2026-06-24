package com.bbozkurtcagri.quranmeali.ui.surahdetail

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.bbozkurtcagri.quranmeali.api.ApiClient
import com.bbozkurtcagri.quranmeali.api.ApiException
import com.bbozkurtcagri.quranmeali.model.SurahDetail
import com.bbozkurtcagri.quranmeali.model.VerseSummary
import kotlinx.coroutines.async
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class SurahDetailUiState(
    val detail: SurahDetail? = null,
    val verses: List<VerseSummary> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

class SurahDetailViewModel(
    private val surahNumber: Int
) : ViewModel() {

    private val _state = MutableStateFlow(SurahDetailUiState())
    val state: StateFlow<SurahDetailUiState> = _state.asStateFlow()

    fun load() {
        if (_state.value.isLoading || _state.value.detail != null) return
        _state.update { it.copy(isLoading = true, error = null) }
        viewModelScope.launch {
            try {
                // Detail + verseler paralel — toplam latency tek istek seviyesine iner.
                coroutineScope {
                    val detailJob = async { ApiClient.unwrap { ApiClient.service.getSurahDetail(surahNumber) } }
                    val versesJob = async { ApiClient.unwrap { ApiClient.service.getSurahVerses(surahNumber) } }
                    val detail = detailJob.await()
                    val paged = versesJob.await()
                    _state.update {
                        it.copy(
                            detail = detail,
                            verses = paged.items,
                            isLoading = false
                        )
                    }
                }
            } catch (e: ApiException) {
                _state.update { it.copy(error = e.message, isLoading = false) }
            } catch (e: Exception) {
                _state.update { it.copy(error = e.message ?: "Bilinmeyen hata", isLoading = false) }
            }
        }
    }

    fun retry() {
        _state.update { SurahDetailUiState() }
        load()
    }

    companion object {
        fun factory(surahNumber: Int): ViewModelProvider.Factory = viewModelFactory {
            initializer { SurahDetailViewModel(surahNumber) }
        }
    }
}
