package com.bbozkurtcagri.quranmeali

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.core.tween
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
import com.bbozkurtcagri.quranmeali.ui.about.AppearancePreference
import com.bbozkurtcagri.quranmeali.ui.navigation.AppNavigation
import com.bbozkurtcagri.quranmeali.ui.splash.SplashContent
import com.bbozkurtcagri.quranmeali.ui.theme.QuranMealiTheme
import com.bbozkurtcagri.quranmeali.ui.theme.SoftSkillEasing
import kotlinx.coroutines.delay

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        // Android 12+ static SplashScreen — sistem warm dark zemini çizer, Compose hazırlanır hazırlanmaz devralır.
        installSplashScreen()
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent { App() }
    }
}

@Composable
private fun App() {
    val context = LocalContext.current
    var appearance by remember { mutableStateOf(AppearancePreference.load(context)) }
    LaunchedEffect(appearance) { AppearancePreference.save(context, appearance) }

    val systemDark = isSystemInDarkTheme()

    // Animated splash overlay — ana içerikle aynı backdrop'ta açılıp 1.7s sonra crossfade ile çekilir.
    var showSplash by remember { mutableStateOf(true) }
    LaunchedEffect(Unit) {
        delay(1700)
        showSplash = false
    }

    QuranMealiTheme(darkTheme = appearance.isDarkOrNull(systemDark)) {
        Box(modifier = Modifier.fillMaxSize()) {
            AppNavigation(
                appearance = appearance,
                onAppearanceChange = { appearance = it }
            )
            AnimatedVisibility(
                visible = showSplash,
                enter = fadeIn(),
                exit = fadeOut(animationSpec = tween(600, easing = SoftSkillEasing))
            ) {
                SplashContent()
            }
        }
    }
}
