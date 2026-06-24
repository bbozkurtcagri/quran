package com.bbozkurtcagri.quranmeali.ui.theme

import androidx.compose.runtime.Immutable
import androidx.compose.runtime.staticCompositionLocalOf
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp
import com.bbozkurtcagri.quranmeali.R

// Bundled fontlar — res/font/ altındaki TTF'lere bağlanır.
// iOS tarafıyla aynı dosyalar (SIL OFL): Instrument Serif, Newsreader (variable),
// Geist + Geist Mono, Amiri.

private val InstrumentSerif = FontFamily(
    Font(R.font.instrument_serif_regular, FontWeight.Normal, FontStyle.Normal),
    Font(R.font.instrument_serif_italic,  FontWeight.Normal, FontStyle.Italic)
)

private val Newsreader = FontFamily(
    Font(R.font.newsreader_regular, FontWeight.Normal, FontStyle.Normal),
    Font(R.font.newsreader_italic,  FontWeight.Normal, FontStyle.Italic),
    // Variable font wght axis'ini Medium için aynı dosya üzerinden vekâlet ettiriyoruz.
    Font(R.font.newsreader_regular, FontWeight.Medium, FontStyle.Normal)
)

private val Geist = FontFamily(
    Font(R.font.geist_regular, FontWeight.Normal),
    Font(R.font.geist_medium,  FontWeight.Medium)
)

private val GeistMono = FontFamily(
    Font(R.font.geist_mono_regular, FontWeight.Normal)
)

private val Amiri = FontFamily(
    Font(R.font.amiri_regular, FontWeight.Normal)
)

@Immutable
data class QcTypography(
    val display: TextStyle,
    val displayItalic: TextStyle,
    val reading: TextStyle,
    val ui: TextStyle,
    val uiMedium: TextStyle,
    val mono: TextStyle,
    val arabic: TextStyle
)

internal val DefaultQcTypography = QcTypography(
    display = TextStyle(
        fontFamily = InstrumentSerif,
        fontWeight = FontWeight.Normal,
        fontSize = 56.sp,
        lineHeight = 60.sp,
        letterSpacing = (-1).sp
    ),
    displayItalic = TextStyle(
        fontFamily = InstrumentSerif,
        fontWeight = FontWeight.Normal,
        fontStyle = FontStyle.Italic,
        fontSize = 56.sp,
        lineHeight = 60.sp,
        letterSpacing = (-1).sp
    ),
    reading = TextStyle(
        fontFamily = Newsreader,
        fontWeight = FontWeight.Normal,
        fontSize = 17.sp,
        lineHeight = 28.sp
    ),
    ui = TextStyle(
        fontFamily = Geist,
        fontWeight = FontWeight.Normal,
        fontSize = 15.sp,
        lineHeight = 20.sp
    ),
    uiMedium = TextStyle(
        fontFamily = Geist,
        fontWeight = FontWeight.Medium,
        fontSize = 15.sp,
        lineHeight = 20.sp
    ),
    mono = TextStyle(
        fontFamily = GeistMono,
        fontWeight = FontWeight.Normal,
        fontSize = 11.sp,
        lineHeight = 16.sp,
        letterSpacing = 2.4.sp
    ),
    arabic = TextStyle(
        fontFamily = Amiri,
        fontWeight = FontWeight.Normal,
        fontSize = 28.sp,
        lineHeight = 48.sp
    )
)

val LocalQcTypography = staticCompositionLocalOf { DefaultQcTypography }

internal val Typography = androidx.compose.material3.Typography(
    bodyLarge = DefaultQcTypography.reading
)
