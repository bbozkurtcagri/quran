package com.bbozkurtcagri.quranmeali.ui.theme

import androidx.compose.runtime.Immutable
import androidx.compose.runtime.staticCompositionLocalOf
import androidx.compose.ui.graphics.Color

// Warm parchment & emerald palette — birebir iOS/web tokenlarıyla eşleşir.
// Light + dark çiftleri ayrı sabitler olarak tutulur, runtime'da hangi şemanın
// aktif olduğuna göre tema seçer.

internal val LightBackground   = Color(0xFFFBFBFA)
internal val LightSurface      = Color(0xFFFFFFFF)
internal val LightText         = Color(0xFF111111)
internal val LightTextMuted    = Color(0xFF6B6660)
internal val LightAccent       = Color(0xFF346538)

internal val DarkBackground    = Color(0xFF1A1612)
internal val DarkSurface       = Color(0xFF221C16)
internal val DarkText          = Color(0xFFF2E9DA)
internal val DarkTextMuted     = Color(0xFF9E907C)
internal val DarkAccent        = Color(0xFF7FB88A)

/// Tema-bağımsız semantik tokenlar — composable'lar bu alanları okur,
/// renk değeri light/dark'a göre seçilir.
@Immutable
data class QcColors(
    val background: Color,
    val surface: Color,
    val text: Color,
    val textMuted: Color,
    val accent: Color,
    val border: Color,       // hairline; text üzerinden %7 opaklık ile türetilir
    val accentSoft: Color    // accent %12 opaklık — chip dolguları
)

internal val LightQcColors = QcColors(
    background = LightBackground,
    surface = LightSurface,
    text = LightText,
    textMuted = LightTextMuted,
    accent = LightAccent,
    border = LightText.copy(alpha = 0.07f),
    accentSoft = LightAccent.copy(alpha = 0.12f)
)

internal val DarkQcColors = QcColors(
    background = DarkBackground,
    surface = DarkSurface,
    text = DarkText,
    textMuted = DarkTextMuted,
    accent = DarkAccent,
    border = DarkText.copy(alpha = 0.07f),
    accentSoft = DarkAccent.copy(alpha = 0.12f)
)

// CompositionLocal — `MaterialTheme.qcColors` extension'ı bu üzerinden okur.
val LocalQcColors = staticCompositionLocalOf { LightQcColors }
