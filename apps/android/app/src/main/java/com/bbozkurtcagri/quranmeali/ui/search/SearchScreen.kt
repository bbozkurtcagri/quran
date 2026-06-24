package com.bbozkurtcagri.quranmeali.ui.search

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.material3.TextFieldDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardCapitalization
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.text.withStyle
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.bbozkurtcagri.quranmeali.model.VerseSearchHit
import com.bbozkurtcagri.quranmeali.ui.components.Eyebrow
import com.bbozkurtcagri.quranmeali.ui.components.SectionRule
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography
import java.util.Locale

@Composable
fun SearchScreen(
    onHitClick: (VerseSearchHit) -> Unit,
    viewModel: SearchViewModel = viewModel()
) {
    val state by viewModel.state.collectAsState()

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.qcColors.background)
    ) {
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .widthIn(max = Spacing.readingMaxWidth)
                .align(Alignment.TopCenter),
            contentPadding = PaddingValues(
                start = Spacing.md,
                end = Spacing.md,
                top = Spacing.xxl,
                bottom = Spacing.huge
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

            item {
                SearchField(
                    value = state.query,
                    mode = state.mode,
                    onChange = viewModel::onQueryChanged,
                    modifier = Modifier.padding(horizontal = Spacing.lg)
                )
            }
            item { Spacer(Modifier.height(Spacing.md)) }
            item {
                ModeBar(
                    mode = state.mode,
                    onChange = viewModel::onModeChanged,
                    modifier = Modifier.padding(horizontal = Spacing.lg)
                )
            }
            item { Spacer(Modifier.height(Spacing.xl)) }

            when {
                state.error != null -> item { ErrorMessage(state.error!!) }
                state.isLoading -> item { LoadingState() }
                state.results.isNotEmpty() -> {
                    item {
                        TotalsBar(
                            label = totalLabel(state.mode, state.totalCount, state.results.size),
                            modifier = Modifier.padding(horizontal = Spacing.lg)
                        )
                    }
                    items(state.results, key = { "${it.globalVerseNumber}-${it.translationSourceCode}" }) { hit ->
                        SearchResultRow(
                            hit = hit,
                            highlight = if (state.mode == SearchMode.Keyword) state.query else "",
                            onClick = { onHitClick(hit) },
                            modifier = Modifier.padding(horizontal = Spacing.lg)
                        )
                    }
                }
                state.query.trim().length >= 2 -> item { EmptyResults() }
                else -> item { Prompt() }
            }
        }
    }
}

private fun totalLabel(mode: SearchMode, totalCount: Long, displayed: Int): String {
    if (mode == SearchMode.Semantic) return "İlk $displayed sonuç"
    if (totalCount == 0L) return "Sonuç yok"
    if (totalCount > displayed) return "$totalCount sonuç · ilk $displayed"
    return "$totalCount sonuç"
}

@Composable
private fun Hero() {
    Column(
        modifier = Modifier.fillMaxWidth(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = "Elmalılı meali içinde arama")
        Text(
            text = "Arama",
            style = MaterialTheme.qcTypography.display.copy(fontSize = 72.sp, lineHeight = 76.sp),
            color = MaterialTheme.qcColors.text,
            textAlign = TextAlign.Center
        )
    }
}

@Composable
private fun SearchField(value: String, mode: SearchMode, onChange: (String) -> Unit, modifier: Modifier = Modifier) {
    val placeholder = if (mode == SearchMode.Semantic)
        "Tahammül, takva, helal–haram, israf…"
    else
        "örn. sabır · namaz · adalet"

    TextField(
        value = value,
        onValueChange = onChange,
        placeholder = {
            Text(
                placeholder,
                style = MaterialTheme.qcTypography.reading,
                color = MaterialTheme.qcColors.textMuted.copy(alpha = 0.6f)
            )
        },
        textStyle = MaterialTheme.qcTypography.reading.copy(
            fontSize = 22.sp,
            lineHeight = 28.sp,
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
        modifier = modifier.fillMaxWidth()
    )
}

@Composable
private fun ModeBar(mode: SearchMode, onChange: (SearchMode) -> Unit, modifier: Modifier = Modifier) {
    Row(
        modifier = modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Row(
            modifier = Modifier
                .clip(RoundedCornerShape(50))
                .background(MaterialTheme.qcColors.accent.copy(alpha = 0.04f))
                .border(1.dp, MaterialTheme.qcColors.border, RoundedCornerShape(50))
                .padding(2.dp)
        ) {
            ModeChip("Anlamsal", mode == SearchMode.Semantic) { onChange(SearchMode.Semantic) }
            ModeChip("Sözcük",   mode == SearchMode.Keyword)  { onChange(SearchMode.Keyword) }
        }
        Eyebrow(text = if (mode == SearchMode.Semantic) "Kavramsal" else "Birebir eşleşme")
    }
}

@Composable
private fun ModeChip(label: String, active: Boolean, onClick: () -> Unit) {
    val bg = if (active) MaterialTheme.qcColors.accentSoft else Color.Transparent
    val fg = if (active) MaterialTheme.qcColors.accent else MaterialTheme.qcColors.textMuted
    Box(
        modifier = Modifier
            .clip(RoundedCornerShape(50))
            .background(bg)
            .clickable(onClick = onClick)
            .padding(horizontal = Spacing.md, vertical = 6.dp)
    ) {
        Text(
            text = label.uppercase(Locale("tr", "TR")),
            style = MaterialTheme.qcTypography.mono.copy(fontSize = 10.sp, letterSpacing = 2.0.sp),
            color = fg
        )
    }
}

@Composable
private fun TotalsBar(label: String, modifier: Modifier = Modifier) {
    Column(modifier = modifier.fillMaxWidth()) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = Spacing.md),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Eyebrow(text = label)
            Eyebrow(text = "Elmalılı")
        }
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(1.dp)
                .background(MaterialTheme.qcColors.border)
        )
    }
}

@Composable
private fun SearchResultRow(
    hit: VerseSearchHit,
    highlight: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
            .padding(vertical = Spacing.lg)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.Top
        ) {
            Eyebrow(text = "${hit.surahNameTurkish} · ${hit.surahNumber}:${hit.verseNumber}")
            Text(
                text = "OKU ↗",
                style = MaterialTheme.qcTypography.mono.copy(fontSize = 10.sp, letterSpacing = 2.2.sp),
                color = MaterialTheme.qcColors.accent
            )
        }
        Spacer(Modifier.height(Spacing.sm))
        Text(
            text = renderHighlighted(hit.translationText, highlight, MaterialTheme.qcColors.accent),
            style = MaterialTheme.qcTypography.reading.copy(fontSize = 17.sp, lineHeight = 26.sp),
            color = MaterialTheme.qcColors.text
        )
        Spacer(Modifier.height(Spacing.lg))
        Box(modifier = Modifier.fillMaxWidth().height(1.dp).background(MaterialTheme.qcColors.border))
    }
}

// Türkçe locale ile case-insensitive substring eşleşmesi; bulunan kısım accent
// rengine boyanır ve altı çizilir.
private fun renderHighlighted(source: String, query: String, accent: Color): AnnotatedString {
    val trimmed = query.trim()
    if (trimmed.length < 2) return AnnotatedString(source)
    val locale = Locale("tr", "TR")
    val lower = source.lowercase(locale)
    val target = trimmed.lowercase(locale)

    return buildAnnotatedString {
        var i = 0
        while (i < source.length) {
            val idx = lower.indexOf(target, i)
            if (idx == -1) {
                append(source.substring(i))
                break
            }
            if (idx > i) append(source.substring(i, idx))
            withStyle(SpanStyle(
                color = accent,
                fontWeight = FontWeight.Medium,
                textDecoration = TextDecoration.Underline
            )) {
                append(source.substring(idx, idx + target.length))
            }
            i = idx + target.length
        }
    }
}

@Composable
private fun LoadingState() {
    Box(modifier = Modifier.fillMaxWidth().padding(vertical = Spacing.huge), contentAlignment = Alignment.Center) {
        Eyebrow(text = "Aranıyor")
    }
}

@Composable
private fun ErrorMessage(message: String) {
    Box(modifier = Modifier.fillMaxWidth().padding(vertical = Spacing.huge), contentAlignment = Alignment.Center) {
        Text(
            text = message,
            style = MaterialTheme.qcTypography.reading,
            color = MaterialTheme.qcColors.textMuted,
            textAlign = TextAlign.Center
        )
    }
}

@Composable
private fun EmptyResults() {
    Box(modifier = Modifier.fillMaxWidth().padding(vertical = Spacing.huge), contentAlignment = Alignment.Center) {
        Text(
            text = "Sonuç bulunamadı.",
            style = MaterialTheme.qcTypography.reading,
            color = MaterialTheme.qcColors.textMuted
        )
    }
}

@Composable
private fun Prompt() {
    Box(
        modifier = Modifier.fillMaxWidth().padding(vertical = Spacing.huge, horizontal = Spacing.lg),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = "Aramaya başlamak için bir kelime veya kavram yazın.",
            style = MaterialTheme.qcTypography.reading,
            color = MaterialTheme.qcColors.textMuted,
            textAlign = TextAlign.Center
        )
    }
}
