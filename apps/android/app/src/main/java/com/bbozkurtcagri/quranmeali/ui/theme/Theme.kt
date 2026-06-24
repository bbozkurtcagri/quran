package com.bbozkurtcagri.quranmeali.ui.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.ReadOnlyComposable

// Dynamic color (Material You) kapalı — markamızın kendi paleti var.

private val LightMaterial = lightColorScheme(
    primary = LightAccent,
    onPrimary = LightSurface,
    background = LightBackground,
    onBackground = LightText,
    surface = LightSurface,
    onSurface = LightText
)

private val DarkMaterial = darkColorScheme(
    primary = DarkAccent,
    onPrimary = DarkBackground,
    background = DarkBackground,
    onBackground = DarkText,
    surface = DarkSurface,
    onSurface = DarkText
)

@Composable
fun QuranMealiTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    val qcColors = if (darkTheme) DarkQcColors else LightQcColors
    val material = if (darkTheme) DarkMaterial else LightMaterial

    CompositionLocalProvider(
        LocalQcColors provides qcColors,
        LocalQcTypography provides DefaultQcTypography
    ) {
        MaterialTheme(
            colorScheme = material,
            typography = Typography,
            content = content
        )
    }
}

// `MaterialTheme.qcColors` / `MaterialTheme.qcTypography` — call-site noise'i
// azaltmak için extension property'ler. iOS'taki Color.qc* eşdeğeri.
val MaterialTheme.qcColors: QcColors
    @Composable @ReadOnlyComposable
    get() = LocalQcColors.current

val MaterialTheme.qcTypography: QcTypography
    @Composable @ReadOnlyComposable
    get() = LocalQcTypography.current
