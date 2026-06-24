package com.bbozkurtcagri.quranmeali.ui.about

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.bbozkurtcagri.quranmeali.ui.components.Eyebrow
import com.bbozkurtcagri.quranmeali.ui.components.SectionRule
import com.bbozkurtcagri.quranmeali.ui.theme.Spacing
import com.bbozkurtcagri.quranmeali.ui.theme.qcColors
import com.bbozkurtcagri.quranmeali.ui.theme.qcTypography
import java.util.Locale

@Composable
fun AboutScreen(
    appearance: AppearancePreference,
    onAppearanceChange: (AppearancePreference) -> Unit
) {
    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.qcColors.background)
    ) {
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .widthIn(max = Spacing.readingMaxWidth)
                .align(Alignment.TopCenter),
            contentPadding = PaddingValues(
                start = Spacing.md, end = Spacing.md,
                top = Spacing.xxl, bottom = Spacing.huge
            )
        ) {
            item { Hero() }
            item { Spacer(Modifier.height(Spacing.lg)) }
            item {
                Box(Modifier.fillMaxWidth(), contentAlignment = Alignment.Center) {
                    SectionRule()
                }
            }
            item { Spacer(Modifier.height(Spacing.xxxl)) }

            item {
                Section(title = "Proje", body =
                    "Kur'an-ı Kerim'i Arapça aslıyla okumak ve Türkçe mealinde kelime ya da kavram " +
                    "aramak için sade bir okuma uygulaması. Reklamsız, ücretsiz, açık kaynaklara dayalı."
                )
            }
            item { Spacer(Modifier.height(Spacing.xxxl)) }

            item {
                Column(verticalArrangement = Arrangement.spacedBy(Spacing.lg)) {
                    Eyebrow(text = "Kaynaklar")
                    Source(
                        title = "Türkçe meal",
                        body = "Elmalılı Muhammed Hamdi Yazır, Hak Dini Kur'an Dili (1935-1938). " +
                                "Müellif 27 Mayıs 1942'de vefat etmiştir; eser, 5846 sayılı Fikir ve Sanat Eserleri " +
                                "Kanunu Madde 27 uyarınca 1 Ocak 2013'ten itibaren kamu malı statüsündedir."
                    )
                    Source(
                        title = "Arapça mushaf",
                        body = "Tanzil Project, Uthmani Minimal sürümü (v1.1, Şubat 2021). " +
                                "Metin lisansı verbatim dağıtıma izin verir; değiştirilmemiştir."
                    )
                    Source(
                        title = "Anlamsal arama",
                        body = "Sorgu ve ayet metinleri intfloat/multilingual-e5-small embedding modeliyle " +
                                "vektörleştirilir. Tüm çıkarımlar yerel ortamda yapılır; kullanıcı verisi " +
                                "üçüncü taraf servislere gönderilmez."
                    )
                }
            }
            item { Spacer(Modifier.height(Spacing.xxxl)) }

            item {
                Section(title = "Hata bildirimi", body =
                    "Mealde ya da arayüzde bir hata fark ederseniz lütfen bildirin — bbozkurtcagri@gmail.com."
                )
            }
            item { Spacer(Modifier.height(Spacing.xxxl)) }

            item {
                Section(title = "Ücretsizdir", body =
                    "Kur'an metni ve meali üzerinden gelir elde edilmez. Hiçbir reklam, takip kodu ya da abonelik yoktur."
                )
            }
            item { Spacer(Modifier.height(Spacing.xxxl)) }

            item {
                AppearanceSection(appearance, onAppearanceChange)
            }
        }
    }
}

@Composable
private fun Hero() {
    Column(
        modifier = Modifier.fillMaxWidth(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = "Hakkında")
        Text(
            text = "Kur'an Meali",
            style = MaterialTheme.qcTypography.display.copy(fontSize = 56.sp, lineHeight = 60.sp),
            color = MaterialTheme.qcColors.text,
            textAlign = TextAlign.Center
        )
    }
}

@Composable
private fun Section(title: String, body: String) {
    Column(
        modifier = Modifier.padding(horizontal = Spacing.lg),
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = title)
        Text(
            text = body,
            style = MaterialTheme.qcTypography.reading.copy(fontSize = 17.sp, lineHeight = 28.sp),
            color = MaterialTheme.qcColors.textMuted
        )
    }
}

@Composable
private fun Source(title: String, body: String) {
    Column(
        modifier = Modifier.padding(horizontal = Spacing.lg),
        verticalArrangement = Arrangement.spacedBy(Spacing.xs)
    ) {
        Text(
            text = title,
            style = MaterialTheme.qcTypography.display.copy(fontSize = 22.sp, lineHeight = 26.sp),
            color = MaterialTheme.qcColors.text
        )
        Text(
            text = body,
            style = MaterialTheme.qcTypography.reading.copy(fontSize = 17.sp, lineHeight = 28.sp),
            color = MaterialTheme.qcColors.textMuted
        )
    }
}

@Composable
private fun AppearanceSection(
    current: AppearancePreference,
    onChange: (AppearancePreference) -> Unit
) {
    Column(
        modifier = Modifier.padding(horizontal = Spacing.lg),
        verticalArrangement = Arrangement.spacedBy(Spacing.md)
    ) {
        Eyebrow(text = "Görünüm")
        Row(
            modifier = Modifier
                .clip(RoundedCornerShape(50))
                .background(MaterialTheme.qcColors.accent.copy(alpha = 0.04f))
                .border(1.dp, MaterialTheme.qcColors.border, RoundedCornerShape(50))
                .padding(2.dp)
        ) {
            AppearancePreference.entries.forEach { pref ->
                val active = pref == current
                val bg = if (active) MaterialTheme.qcColors.accentSoft else Color.Transparent
                val fg = if (active) MaterialTheme.qcColors.accent else MaterialTheme.qcColors.textMuted
                Box(
                    modifier = Modifier
                        .clip(RoundedCornerShape(50))
                        .background(bg)
                        .clickable { onChange(pref) }
                        .padding(horizontal = Spacing.md, vertical = 8.dp)
                ) {
                    Text(
                        text = pref.label.uppercase(Locale("tr", "TR")),
                        style = MaterialTheme.qcTypography.mono.copy(fontSize = 10.sp, letterSpacing = 2.0.sp),
                        color = fg
                    )
                }
            }
        }
    }
}
