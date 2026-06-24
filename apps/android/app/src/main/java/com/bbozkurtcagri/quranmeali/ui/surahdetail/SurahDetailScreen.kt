package com.bbozkurtcagri.quranmeali.ui.surahdetail

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.runtime.snapshotFlow
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.bbozkurtcagri.quranmeali.model.LastRead
import com.bbozkurtcagri.quranmeali.model.LastReadStore
import com.bbozkurtcagri.quranmeali.model.SurahDetail
import com.bbozkurtcagri.quranmeali.ui.components.Eyebrow
import com.bbozkurtcagri.quranmeali.ui.components.SectionRule
import com.bbozkurtcagri.quranmeali.ui.components.VerseBlock
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography
import kotlinx.coroutines.flow.distinctUntilChanged

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SurahDetailScreen(
    surahNumber: Int,
    targetVerse: Int? = null,
    onBack: () -> Unit
) {
    val context = LocalContext.current
    val vm: SurahDetailViewModel = viewModel(
        key = "surah-$surahNumber",
        factory = SurahDetailViewModel.factory(surahNumber)
    )
    val state by vm.state.collectAsState()
    val listState = rememberLazyListState()

    var isProgrammaticScrolling by remember { mutableStateOf(targetVerse != null) }
    var hasScrolledToTarget by remember { mutableStateOf(false) }

    // Predictive back / sistem geri — NavHost otomatik handle etmiyorsa garanti edelim.
    BackHandler(enabled = true) { onBack() }

    LaunchedEffect(Unit) { vm.load() }

    LaunchedEffect(state.detail) {
        val d = state.detail ?: return@LaunchedEffect
        LastReadStore.save(
            context,
            LastRead(d.number, d.nameTurkish, targetVerse ?: 1, System.currentTimeMillis())
        )
    }

    LaunchedEffect(state.verses.size, targetVerse) {
        if (hasScrolledToTarget) return@LaunchedEffect
        if (state.verses.isEmpty()) return@LaunchedEffect
        val target = targetVerse ?: run {
            isProgrammaticScrolling = false
            return@LaunchedEffect
        }
        // LazyColumn item dizilimi: 0 = header, 1 = verse 1, ..., N = verse N.
        // Yani target verseNumber doğrudan item index'i olarak kullanılır.
        listState.animateScrollToItem(target)
        hasScrolledToTarget = true
        isProgrammaticScrolling = false
        state.detail?.let {
            LastReadStore.save(
                context,
                LastRead(it.number, it.nameTurkish, target, System.currentTimeMillis())
            )
        }
    }

    LaunchedEffect(state.detail) {
        snapshotFlow { listState.firstVisibleItemIndex }
            .distinctUntilChanged()
            .collect { idx ->
                if (isProgrammaticScrolling) return@collect
                if (idx == 0) return@collect  // header görünürken save'i tetikleme
                val d = state.detail ?: return@collect
                // Item dizilimi: 0 = header, N = verse N. idx doğrudan verseNumber.
                val verseNumber = idx.coerceAtMost(state.verses.size.coerceAtLeast(1))
                LastReadStore.save(
                    context,
                    LastRead(d.number, d.nameTurkish, verseNumber, System.currentTimeMillis())
                )
            }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {},
                navigationIcon = {
                    // iOS'taki "← SURELER" toolbar item — sabit, scroll'dan etkilenmez.
                    TextButton(onClick = onBack) {
                        Text(
                            text = "← SURELER",
                            style = MaterialTheme.qcTypography.mono.copy(letterSpacing = 2.4.sp),
                            color = MaterialTheme.qcColors.textMuted
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.qcColors.background,
                    scrolledContainerColor = MaterialTheme.qcColors.background
                )
            )
        },
        containerColor = MaterialTheme.qcColors.background
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .background(MaterialTheme.qcColors.background)
        ) {
            LazyColumn(
                state = listState,
                modifier = Modifier
                    .fillMaxSize()
                    .widthIn(max = Spacing.readingMaxWidth)
                    .align(Alignment.TopCenter),
                contentPadding = PaddingValues(
                    start = Spacing.md, end = Spacing.md,
                    bottom = Spacing.huge
                )
            ) {
                // Editorial header — LazyColumn'un *contentinin* ilk item'ı değil,
                // verse içeriğinin önüne tek başına eklenir; index böylece verseNumber'a 1:1 hizalanır.
                item("header") {
                    Header(detail = state.detail)
                }

                items(items = state.verses, key = { it.verseNumber }) { verse ->
                    VerseBlock(
                        verse = verse,
                        modifier = Modifier.padding(horizontal = Spacing.xs)
                    )
                }

                if (state.error != null) {
                    item { ErrorState(state.error!!, vm::retry) }
                } else if (state.isLoading && state.verses.isEmpty()) {
                    item { LoadingState() }
                }
            }
        }
    }
}

// MARK: - NOT: header artık index 0'da; verseler 1+. firstVisibleItemIndex == 0
//       header görünürken save tetiklenmesin diye scroll tracking'te idx == 0 atlanır.
//       Yukarıdaki snapshotFlow bloğu zaten bunu yapıyor (idx+1 -> verseNumber).
//       Header görünürken verseNumber=1 yazılır, kabul edilebilir.

@Composable
private fun Header(detail: SurahDetail?) {
    if (detail == null) return
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = Spacing.xl, bottom = Spacing.huge),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = "Sure ${detail.number} · ${detail.revelationPlace.turkishLabel} · ${detail.verseCount} ayet")
        Text(
            text = detail.nameTurkish,
            style = MaterialTheme.qcTypography.display.copy(fontSize = 60.sp, lineHeight = 64.sp),
            color = MaterialTheme.qcColors.text,
            textAlign = TextAlign.Center
        )
        Text(
            text = detail.nameArabic,
            style = MaterialTheme.qcTypography.arabic.copy(fontSize = 44.sp, lineHeight = 64.sp),
            color = MaterialTheme.qcColors.accent
        )
        Eyebrow(text = detail.nameTransliteration)
        Spacer(Modifier.height(Spacing.lg))
        SectionRule()
    }
}

@Composable
private fun LoadingState() {
    Box(
        modifier = Modifier.fillMaxWidth().padding(vertical = Spacing.xxxl),
        contentAlignment = Alignment.Center
    ) {
        Eyebrow(text = "Yükleniyor")
    }
}

@Composable
private fun ErrorState(message: String, onRetry: () -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = Spacing.xxxl, horizontal = Spacing.lg),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = "Hata")
        Text(
            text = message,
            style = MaterialTheme.qcTypography.reading,
            color = MaterialTheme.qcColors.textMuted,
            textAlign = TextAlign.Center
        )
        TextButton(onClick = onRetry) {
            Text(
                text = "TEKRAR DENE",
                style = MaterialTheme.qcTypography.mono.copy(letterSpacing = 2.4.sp),
                color = MaterialTheme.qcColors.accent
            )
        }
    }
}
