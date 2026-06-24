package com.bbozkurtcagri.quranmeali.ui.theme

import androidx.compose.ui.unit.dp

// 4-8-12-16-24-32-48-64-96 ladder — web/iOS tokenlarıyla aynı.
object Spacing {
    val xxs  = 4.dp
    val xs   = 8.dp
    val sm   = 12.dp
    val md   = 16.dp
    val lg   = 24.dp
    val xl   = 32.dp
    val xxl  = 48.dp
    val xxxl = 64.dp
    val huge = 96.dp

    // Okuma kolonu max genişliği — web max-w-3xl ≈ 672dp.
    val readingMaxWidth = 672.dp
    // Liste tarama kolonu — iPad/tablet'te taşmayı engellemek için cap.
    val browseMaxWidth  = 720.dp
}
