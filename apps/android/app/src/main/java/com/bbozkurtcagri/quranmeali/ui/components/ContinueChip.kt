package com.bbozkurtcagri.quranmeali.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.bbozkurtcagri.quranmeali.model.LastRead
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography

// Son okunan ayet chip'i. Ana gövde tıklanırsa devam eder; sağ üstte ✕ ile
// chip silinir. iOS'taki ZStack overlay pattern'ının Compose karşılığı.
@Composable
fun ContinueChip(
    lastRead: LastRead,
    onResume: () -> Unit,
    onClear: () -> Unit,
    modifier: Modifier = Modifier
) {
    Box(modifier = modifier.fillMaxWidth()) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .clip(RoundedCornerShape(4.dp))
                .background(MaterialTheme.qcColors.accentSoft)
                .clickable(onClick = onResume)
                .padding(horizontal = Spacing.lg, vertical = Spacing.lg),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column(modifier = Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(Spacing.xxs)) {
                Text(
                    text = "DEVAM ET",
                    style = MaterialTheme.qcTypography.mono.copy(fontSize = 10.sp, letterSpacing = 2.2.sp),
                    color = MaterialTheme.qcColors.accent
                )
                Text(
                    text = lastRead.surahNameTurkish,
                    style = MaterialTheme.qcTypography.display.copy(fontSize = 22.sp, lineHeight = 26.sp),
                    color = MaterialTheme.qcColors.text
                )
                Text(
                    text = "Sure ${lastRead.surahNumber} · Ayet ${lastRead.verseNumber}",
                    style = MaterialTheme.qcTypography.mono.copy(fontSize = 11.sp, letterSpacing = 1.6.sp),
                    color = MaterialTheme.qcColors.textMuted
                )
            }
            Text(
                text = "AÇ ↗",
                style = MaterialTheme.qcTypography.mono.copy(fontSize = 10.sp, letterSpacing = 2.2.sp),
                color = MaterialTheme.qcColors.accent,
                modifier = Modifier
                    .clip(RoundedCornerShape(50))
                    .border(1.dp, MaterialTheme.qcColors.accent.copy(alpha = 0.45f), RoundedCornerShape(50))
                    .padding(horizontal = Spacing.md, vertical = Spacing.xs)
            )
        }
        // ✕ — Row tıklamasının üstüne ZIndex ile bindirilir; click ana onResume'i tetiklemez.
        Box(
            modifier = Modifier
                .align(Alignment.TopEnd)
                .clickable(onClick = onClear)
                .padding(Spacing.sm)
        ) {
            Text(
                text = "×",
                fontSize = 18.sp,
                color = MaterialTheme.qcColors.textMuted
            )
        }
    }
}
