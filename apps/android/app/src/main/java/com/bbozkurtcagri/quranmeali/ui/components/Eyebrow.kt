package com.bbozkurtcagri.quranmeali.ui.components

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography
import java.util.Locale

// Üst başlık etiketi — büyük harf, mono, geniş tracking.
// Türkçe "i → İ" ve "ı → I" eşlemesi için locale TR sabitli.
private val TR = Locale("tr", "TR")

@Composable
fun Eyebrow(text: String, modifier: Modifier = Modifier) {
    Text(
        text = text.uppercase(TR),
        style = MaterialTheme.qcTypography.mono,
        color = MaterialTheme.qcColors.textMuted,
        modifier = modifier
    )
}
