package com.bbozkurtcagri.quranmeali.ui.splash

import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.LinearEasing
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.draw.scale
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.PathEffect
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.bbozkurtcagri.quranmeali.ui.theme.SoftSkillEasing
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

// iOS SplashView'in Compose karşılığı — şemse + döner halka + section rule +
// eyebrow + Instrument Serif wordmark, soft-skill easing ile fade-up.
@Composable
fun SplashContent() {
    // Element-bazlı durumlar — Animatable ile zincirli `animateTo` çağrıları yapılır.
    val rosetteOpacity = remember { Animatable(0f) }
    val rosetteScale   = remember { Animatable(0.6f) }
    val rosetteRot     = remember { Animatable(-22f) }
    val ringOpacity    = remember { Animatable(0f) }
    val ruleScale      = remember { Animatable(0.01f) }
    val eyebrowOpacity = remember { Animatable(0f) }
    val wordmarkOpacity = remember { Animatable(0f) }
    val wordmarkOffsetY = remember { Animatable(18f) }

    // Dış halka sürekli yavaş döner — infiniteTransition iOS'taki ringRotation eşdeğeri.
    val infinite = rememberInfiniteTransition(label = "orbital-ring")
    val ringRotation by infinite.animateFloat(
        initialValue = 0f,
        targetValue = 360f,
        animationSpec = infiniteRepeatable(
            animation = tween(durationMillis = 14_000, easing = LinearEasing),
            repeatMode = RepeatMode.Restart
        ),
        label = "orbital-rotation"
    )

    val scope = rememberCoroutineScope()
    LaunchedEffect(Unit) {
        // 0.10s — rosette in
        scope.launch {
            delay(100)
            launch { rosetteOpacity.animateTo(1f, tween(600, easing = SoftSkillEasing)) }
            launch { rosetteScale.animateTo(1f,   tween(600, easing = SoftSkillEasing)) }
            launch { rosetteRot.animateTo(0f,     tween(600, easing = SoftSkillEasing)) }
        }
        // 0.30s — dış halka belirir
        scope.launch {
            delay(300)
            ringOpacity.animateTo(1f, tween(600, easing = SoftSkillEasing))
        }
        // 0.55s — section rule soldan açılır
        scope.launch {
            delay(550)
            ruleScale.animateTo(1f, tween(600, easing = SoftSkillEasing))
        }
        // 0.70s — eyebrow fade
        scope.launch {
            delay(700)
            eyebrowOpacity.animateTo(1f, tween(600, easing = SoftSkillEasing))
        }
        // 0.90s — wordmark fade + slide
        scope.launch {
            delay(900)
            launch { wordmarkOpacity.animateTo(1f, tween(600, easing = SoftSkillEasing)) }
            launch { wordmarkOffsetY.animateTo(0f,  tween(600, easing = SoftSkillEasing)) }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.qcColors.background)
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(horizontal = Spacing.lg),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            // Rosette + halka — overlap
            Box(
                modifier = Modifier.size(220.dp),
                contentAlignment = Alignment.Center
            ) {
                OrbitalRing(
                    modifier = Modifier
                        .size(220.dp)
                        .alpha(ringOpacity.value)
                        .rotate(ringRotation)
                )
                Rosette(
                    modifier = Modifier
                        .size(140.dp)
                        .alpha(rosetteOpacity.value)
                        .scale(rosetteScale.value)
                        .rotate(rosetteRot.value)
                )
            }

            Spacer(Modifier.height(Spacing.xxxl))

            // Section rule — soldan-sağa açılan hairline + accent nokta
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(1.dp)
                    .scale(scaleX = ruleScale.value, scaleY = 1f),
                contentAlignment = Alignment.Center
            ) {
                Box(
                    modifier = Modifier
                        .width(96.dp)
                        .height(1.dp)
                        .background(MaterialTheme.qcColors.border)
                )
            }

            Spacer(Modifier.height(Spacing.lg))

            // Eyebrow
            Text(
                text = "KUR'AN-I KERİM · TÜRKÇE MEAL",
                style = MaterialTheme.qcTypography.mono.copy(letterSpacing = 3.0.sp),
                color = MaterialTheme.qcColors.textMuted,
                modifier = Modifier.alpha(eyebrowOpacity.value)
            )

            Spacer(Modifier.height(Spacing.lg))

            // Wordmark
            Text(
                text = "Kur'an Meali",
                style = MaterialTheme.qcTypography.display.copy(fontSize = 56.sp, lineHeight = 60.sp),
                color = MaterialTheme.qcColors.text,
                textAlign = TextAlign.Center,
                modifier = Modifier
                    .alpha(wordmarkOpacity.value)
                    .padding(top = wordmarkOffsetY.value.dp)
            )
        }
    }
}

// MARK: - Rosette (şemse)
@Composable
private fun Rosette(modifier: Modifier = Modifier) {
    val accent = MaterialTheme.qcColors.accent
    val bg = MaterialTheme.qcColors.background
    Canvas(modifier = modifier) {
        val w = size.width
        val cx = w / 2f
        val cy = size.height / 2f
        val petalR = w * 0.28f
        val off = w * 0.20f
        // Dört petal — overlapping circles
        listOf(0f to off, 0f to -off, off to 0f, -off to 0f).forEach { (dx, dy) ->
            drawCircle(color = accent, radius = petalR, center = Offset(cx + dx, cy + dy))
        }
        // Negatif göbek — background rengiyle delik açılır
        drawCircle(color = bg, radius = w * 0.13f, center = Offset(cx, cy))
        // Merkez disk
        drawCircle(color = accent, radius = w * 0.04f, center = Offset(cx, cy))
    }
}

// MARK: - Orbital dashed ring
@Composable
private fun OrbitalRing(modifier: Modifier = Modifier) {
    val accent = MaterialTheme.qcColors.accent.copy(alpha = 0.35f)
    Canvas(modifier = modifier) {
        drawCircle(
            color = accent,
            radius = size.minDimension / 2f - 1f,
            style = Stroke(
                width = 1f,
                pathEffect = PathEffect.dashPathEffect(floatArrayOf(2f, 6f))
            )
        )
    }
}
