//
//  AboutView.swift
//  QuranMeali
//
//  Mirrors apps/web/src/pages/AboutPage.tsx — project, sources, contact,
//  and the appearance toggle (light/dark/system) tucked at the bottom.
//

import SwiftUI

enum AppearancePreference: String, CaseIterable, Identifiable {
    case system, light, dark

    var id: String { rawValue }

    var label: String {
        switch self {
        case .system: return "Sistem"
        case .light:  return "Açık"
        case .dark:   return "Koyu"
        }
    }

    var colorScheme: ColorScheme? {
        switch self {
        case .system: return nil
        case .light:  return .light
        case .dark:   return .dark
        }
    }
}

struct AboutView: View {
    @AppStorage("qc-appearance") private var appearanceRaw: String = AppearancePreference.system.rawValue

    var body: some View {
        ScrollView {
            VStack(spacing: 0) {
                Header()
                    .padding(.top, Spacing.xxl)
                    .padding(.bottom, Spacing.lg)

                SectionRule()
                    .padding(.bottom, Spacing.xxxl)

                VStack(alignment: .leading, spacing: Spacing.xxxl) {
                    section(
                        title: "Proje",
                        body: "Kur'an-ı Kerim'i Arapça aslıyla okumak ve Türkçe mealinde kelime ya da kavram aramak için sade bir okuma uygulaması. Reklamsız, ücretsiz, açık kaynaklara dayalı."
                    )

                    VStack(alignment: .leading, spacing: Spacing.lg) {
                        Eyebrow(text: "Kaynaklar")
                        sourceItem(
                            title: "Türkçe meal",
                            body: "Elmalılı Muhammed Hamdi Yazır, *Hak Dini Kur'an Dili* (1935–1938). Müellif 27 Mayıs 1942'de vefat etmiştir; eser, 5846 sayılı Fikir ve Sanat Eserleri Kanunu Madde 27 uyarınca 1 Ocak 2013'ten itibaren kamu malı statüsündedir."
                        )
                        sourceItem(
                            title: "Arapça mushaf",
                            body: "Tanzil Project, *Uthmani Minimal* sürümü (v1.1, Şubat 2021). Metin lisansı verbatim dağıtıma izin verir; değiştirilmemiştir."
                        )
                        sourceItem(
                            title: "Anlamsal arama",
                            body: "Sorgu ve ayet metinleri *intfloat/multilingual-e5-small* embedding modeliyle vektörleştirilir. Tüm çıkarımlar yerel ortamda yapılır; kullanıcı verisi üçüncü taraf servislere gönderilmez."
                        )
                    }

                    section(
                        title: "Hata bildirimi",
                        body: "Mealde ya da arayüzde bir hata fark ederseniz lütfen bildirin — bbozkurtcagri@gmail.com."
                    )

                    section(
                        title: "Ücretsizdir",
                        body: "Kur'an metni ve meali üzerinden gelir elde edilmez. Hiçbir reklam, takip kodu ya da abonelik yoktur."
                    )

                    appearanceSection
                }
                .padding(.horizontal, Spacing.lg)
                .padding(.bottom, Spacing.huge)
            }
            .frame(maxWidth: Spacing.readingMaxWidth)
            .frame(maxWidth: .infinity)
            .padding(.horizontal, Spacing.md)
        }
        .background(Color.qcBackground.ignoresSafeArea())
    }

    private func section(title: String, body: String) -> some View {
        VStack(alignment: .leading, spacing: Spacing.md) {
            Eyebrow(text: title)
            Text(body)
                .font(.qcReading(17))
                .foregroundStyle(Color.qcTextMuted)
                .lineSpacing(8)
                .multilineTextAlignment(.leading)
                .frame(maxWidth: .infinity, alignment: .leading)
        }
    }

    private func sourceItem(title: String, body: String) -> some View {
        VStack(alignment: .leading, spacing: Spacing.xs) {
            Text(title)
                .font(.qcDisplay(22))
                .foregroundStyle(Color.qcText)
            Text(renderMarkdownItalics(body))
                .font(.qcReading(17))
                .foregroundStyle(Color.qcTextMuted)
                .lineSpacing(8)
                .multilineTextAlignment(.leading)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
    }

    /// Renders simple `*italic*` markers without pulling a full Markdown parser.
    private func renderMarkdownItalics(_ source: String) -> AttributedString {
        do {
            return try AttributedString(markdown: source)
        } catch {
            return AttributedString(source)
        }
    }

    private var appearanceSection: some View {
        VStack(alignment: .leading, spacing: Spacing.md) {
            Eyebrow(text: "Görünüm")

            HStack(spacing: 0) {
                ForEach(AppearancePreference.allCases) { pref in
                    let active = appearanceRaw == pref.rawValue
                    Button {
                        appearanceRaw = pref.rawValue
                    } label: {
                        Text(pref.label.uppercased())
                            .font(.qcMono(10))
                            .tracking(2.0)
                            .foregroundStyle(active ? Color.qcAccent : Color.qcTextMuted)
                            .padding(.horizontal, Spacing.md)
                            .padding(.vertical, 8)
                            .background(
                                Capsule().fill(active ? Color.qcAccentSoft : .clear)
                            )
                            .animation(.softSkill, value: active)
                    }
                    .buttonStyle(.plain)
                }
            }
            .padding(2)
            .background(Capsule().fill(Color.qcAccent.opacity(0.04)))
            .overlay(Capsule().stroke(Color.qcBorder, lineWidth: 1))
        }
    }
}

// MARK: - Header

private struct Header: View {
    var body: some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Hakkında")
            Text("Kur'an Meali")
                .font(.qcDisplay(56))
                .tracking(-1.0)
                .foregroundStyle(Color.qcText)
                .lineLimit(1)
                .minimumScaleFactor(0.5)
        }
        .frame(maxWidth: .infinity)
    }
}
