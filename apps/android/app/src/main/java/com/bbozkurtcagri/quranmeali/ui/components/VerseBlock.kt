package com.bbozkurtcagri.quranmeali.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.LayoutDirection
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.ui.platform.LocalLayoutDirection
import com.bbozkurtcagri.quranmeali.model.VerseSummary
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography

// Editorial verse bloku: ayet no + cüz/sayfa caption + Arapça + Türkçe meal.
@Composable
fun VerseBlock(verse: VerseSummary, modifier: Modifier = Modifier) {
    Column(
        modifier = modifier
            .fillMaxWidth()
            .padding(vertical = Spacing.xl)
    ) {
        // Üst satır: büyük yeşil ayet numarası + sağda küçük mono caption.
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.Top,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Text(
                text = verse.verseNumber.toString(),
                style = MaterialTheme.qcTypography.display.copy(fontSize = 30.sp, lineHeight = 32.sp),
                color = MaterialTheme.qcColors.accent
            )
            Text(
                text = "CÜZ ${verse.juzNumber} · SAYFA ${verse.pageNumber}",
                style = MaterialTheme.qcTypography.mono.copy(fontSize = 10.sp, letterSpacing = 2.2.sp),
                color = MaterialTheme.qcColors.textMuted,
                modifier = Modifier.padding(top = Spacing.sm)
            )
        }

        // Arapça mushaf — RTL layout local'iyle satırlar doğru kırılır.
        CompositionLocalProvider(LocalLayoutDirection provides LayoutDirection.Rtl) {
            Text(
                text = verse.arabicText,
                style = MaterialTheme.qcTypography.arabic.copy(fontSize = 28.sp, lineHeight = 48.sp),
                color = MaterialTheme.qcColors.text,
                textAlign = TextAlign.Start,           // RTL'de "start" sağa hizalar
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = Spacing.lg)
            )
        }

        // Türkçe meal — Newsreader (şimdilik sistem serif).
        verse.primaryTranslation?.text?.let { meal ->
            Text(
                text = meal,
                style = MaterialTheme.qcTypography.reading.copy(fontSize = 18.sp, lineHeight = 28.sp),
                color = MaterialTheme.qcColors.textMuted,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = Spacing.xl)
            )
        }

        // Hairline alt sınır
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(1.dp)
                .padding(top = Spacing.xl)
                .background(MaterialTheme.qcColors.border)
        )
    }
}
