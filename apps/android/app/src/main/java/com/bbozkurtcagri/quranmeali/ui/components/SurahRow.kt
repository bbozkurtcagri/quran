package com.bbozkurtcagri.quranmeali.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.bbozkurtcagri.quranmeali.model.SurahListItem
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography

// Liste satırı: [02 numarası] [Türkçe ad + eyebrow] [Arapça ad]
@Composable
fun SurahRow(
    surah: SurahListItem,
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier
        .fillMaxWidth()
        .clickable(onClick = onClick)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = Spacing.md, vertical = Spacing.lg),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(Spacing.lg)
        ) {
            Text(
                text = "%02d".format(surah.number),
                style = MaterialTheme.qcTypography.mono.copy(fontSize = 12.sp),
                color = MaterialTheme.qcColors.textMuted,
                modifier = Modifier.width(28.dp)
            )

            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = surah.nameTurkish,
                    style = MaterialTheme.qcTypography.display.copy(fontSize = 24.sp, lineHeight = 28.sp),
                    color = MaterialTheme.qcColors.text,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Eyebrow(
                    text = "${surah.revelationPlace.turkishLabel} · ${surah.verseCount} ayet · ${surah.nameTransliteration}",
                    modifier = Modifier.padding(top = Spacing.xxs)
                )
            }

            Text(
                text = surah.nameArabic,
                style = MaterialTheme.qcTypography.arabic.copy(fontSize = 22.sp, lineHeight = 28.sp),
                color = MaterialTheme.qcColors.text
            )
        }

        // hairline divider — son satırda da görünür, web/iOS tasarımıyla uyumlu
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(1.dp)
                .background(MaterialTheme.qcColors.border)
        )
    }
}
