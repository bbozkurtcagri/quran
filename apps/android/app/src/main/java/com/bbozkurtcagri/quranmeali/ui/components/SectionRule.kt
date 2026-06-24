package com.bbozkurtcagri.quranmeali.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.unit.dp
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors

// Editorial bölüm ayracı — iki hairline + ortada küçük emerald nokta.
@Composable
fun SectionRule(modifier: Modifier = Modifier) {
    Row(
        modifier = modifier,
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        androidx.compose.foundation.layout.Box(
            modifier = Modifier
                .width(48.dp)
                .height(1.dp)
                .background(MaterialTheme.qcColors.border)
        )
        androidx.compose.foundation.layout.Box(
            modifier = Modifier
                .size(6.dp)
                .clip(RoundedCornerShape(50))
                .background(MaterialTheme.qcColors.accent)
        )
        androidx.compose.foundation.layout.Box(
            modifier = Modifier
                .width(48.dp)
                .height(1.dp)
                .background(MaterialTheme.qcColors.border)
        )
    }
}
