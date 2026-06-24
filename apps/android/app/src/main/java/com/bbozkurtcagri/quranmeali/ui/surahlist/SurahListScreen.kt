package com.bbozkurtcagri.quranmeali.ui.surahlist

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
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TextField
import androidx.compose.material3.TextFieldDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardCapitalization
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.bbozkurtcagri.quranmeali.model.LastRead
import com.bbozkurtcagri.quranmeali.model.LastReadStore
import com.bbozkurtcagri.quranmeali.model.SurahListItem
import com.bbozkurtcagri.quranmeali.ui.components.ContinueChip
import com.bbozkurtcagri.quranmeali.ui.components.Eyebrow
import com.bbozkurtcagri.quranmeali.ui.components.SectionRule
import com.bbozkurtcagri.quranmeali.ui.components.SurahRow
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography

@Composable
fun SurahListScreen(
    onSurahClick: (SurahListItem) -> Unit,
    onResume: (LastRead) -> Unit,
    /// Tab'a her dönüşte chip'in tazelenmesi için kullanılan tetikleyici.
    refreshKey: Any = Unit,
    viewModel: SurahListViewModel = viewModel()
) {
    val state by viewModel.state.collectAsState()
    val context = LocalContext.current

    // Tab'a her gelişte SharedPreferences'tan en güncel last-read'i çek.
    var lastRead by remember { mutableStateOf<LastRead?>(null) }
    LaunchedEffect(refreshKey) {
        lastRead = LastReadStore.load(context)
    }

    LaunchedEffect(Unit) { viewModel.load() }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.qcColors.background)
    ) {
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .widthIn(max = Spacing.browseMaxWidth)
                .align(Alignment.TopCenter),
            contentPadding = PaddingValues(
                start = Spacing.md,
                end = Spacing.md,
                top = Spacing.xxl,
                bottom = Spacing.xxl
            )
        ) {
            item { Hero() }
            item { Spacer(Modifier.height(Spacing.lg)) }
            item {
                Box(Modifier.fillMaxWidth(), contentAlignment = Alignment.Center) {
                    SectionRule()
                }
            }
            item { Spacer(Modifier.height(Spacing.xxl)) }

            lastRead?.let { lr ->
                item {
                    ContinueChip(
                        lastRead = lr,
                        onResume = { onResume(lr) },
                        onClear = {
                            LastReadStore.clear(context)
                            lastRead = null
                        },
                        modifier = Modifier.padding(horizontal = Spacing.lg)
                    )
                }
                item { Spacer(Modifier.height(Spacing.xl)) }
            }

            item {
                FilterField(
                    value = state.filterText,
                    onChange = viewModel::setFilter,
                    modifier = Modifier.padding(horizontal = Spacing.lg)
                )
            }
            item { Spacer(Modifier.height(Spacing.lg)) }

            when {
                state.error != null -> item { ErrorState(state.error!!, viewModel::retry) }
                state.isLoading && state.surahs.isEmpty() -> item { LoadingState() }
                state.filtered.isEmpty() && state.filterText.isNotEmpty() ->
                    item { EmptyFilterState(state.filterText) { viewModel.setFilter("") } }
                else -> items(state.filtered, key = { it.number }) { surah ->
                    SurahRow(
                        surah = surah,
                        onClick = { onSurahClick(surah) },
                        modifier = Modifier.padding(horizontal = Spacing.lg)
                    )
                }
            }
        }
    }
}

@Composable
private fun Hero() {
    Column(
        modifier = Modifier.fillMaxWidth(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = "Kur'an-ı Kerim · 114 sure · 6236 ayet")
        Text(
            text = "Sureler",
            style = MaterialTheme.qcTypography.display.copy(fontSize = 72.sp, lineHeight = 76.sp),
            color = MaterialTheme.qcColors.text,
            textAlign = TextAlign.Center
        )
    }
}

@Composable
private fun FilterField(value: String, onChange: (String) -> Unit, modifier: Modifier = Modifier) {
    Box(modifier = modifier.fillMaxWidth(), contentAlignment = Alignment.Center) {
        TextField(
            value = value,
            onValueChange = onChange,
            placeholder = {
                Text(
                    "Sure ara — adı veya numarası",
                    style = MaterialTheme.qcTypography.reading,
                    color = MaterialTheme.qcColors.textMuted.copy(alpha = 0.6f)
                )
            },
            textStyle = MaterialTheme.qcTypography.reading.copy(
                color = MaterialTheme.qcColors.text
            ),
            singleLine = true,
            colors = TextFieldDefaults.colors(
                focusedContainerColor = Color.Transparent,
                unfocusedContainerColor = Color.Transparent,
                disabledContainerColor = Color.Transparent,
                focusedIndicatorColor = MaterialTheme.qcColors.accent,
                unfocusedIndicatorColor = MaterialTheme.qcColors.border,
                cursorColor = MaterialTheme.qcColors.accent
            ),
            keyboardOptions = KeyboardOptions(
                capitalization = KeyboardCapitalization.None,
                autoCorrectEnabled = false,
                imeAction = ImeAction.Search
            ),
            modifier = Modifier.widthIn(max = 420.dp).fillMaxWidth()
        )
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
private fun EmptyFilterState(filterText: String, onClear: () -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = Spacing.xxxl, horizontal = Spacing.lg),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(Spacing.sm)
    ) {
        Text(
            text = "\"$filterText\" için sure bulunamadı.",
            style = MaterialTheme.qcTypography.reading,
            color = MaterialTheme.qcColors.textMuted,
            textAlign = TextAlign.Center
        )
        TextButton(onClick = onClear) {
            Text(
                text = "ARAMAYI TEMİZLE",
                style = MaterialTheme.qcTypography.mono.copy(letterSpacing = 2.4.sp),
                color = MaterialTheme.qcColors.accent
            )
        }
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
