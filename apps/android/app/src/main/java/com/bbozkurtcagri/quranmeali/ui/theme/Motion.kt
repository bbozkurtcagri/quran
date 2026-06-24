package com.bbozkurtcagri.quranmeali.ui.theme

import androidx.compose.animation.core.CubicBezierEasing
import androidx.compose.animation.core.tween

// Tek kanonik easing — web'in cubic-bezier(0.16, 1, 0.3, 1) "soft-skill" eğrisi.
val SoftSkillEasing = CubicBezierEasing(0.16f, 1f, 0.3f, 1f)

object Motion {
    /// 200ms — hover/focus/küçük geçişler.
    fun softSkill(durationMillis: Int = 200) = tween<Float>(
        durationMillis = durationMillis,
        easing = SoftSkillEasing
    )

    /// 600ms — sayfa girişleri, splash element animasyonları.
    fun softSkillSlow(durationMillis: Int = 600) = tween<Float>(
        durationMillis = durationMillis,
        easing = SoftSkillEasing
    )
}
