//
//  VerseBlock.swift
//  QuranMeali
//
//  Editorial verse block: ayet numarası, Arapça, Türkçe meal.
//  Mirrors apps/web/src/components/Verse.tsx.
//

import SwiftUI

struct VerseBlock: View {
    let verse: VerseSummary

    var body: some View {
        VStack(alignment: .leading, spacing: 0) {
            // Number + cüz/sayfa caption
            HStack(alignment: .firstTextBaseline, spacing: Spacing.md) {
                Text("\(verse.verseNumber)")
                    .font(.qcDisplay(30))
                    .foregroundStyle(Color.qcAccent)
                    .monospacedDigit()

                Spacer(minLength: 0)

                Text("CÜZ \(verse.juzNumber) · SAYFA \(verse.pageNumber)")
                    .font(.qcMono(10))
                    .tracking(2.2)
                    .foregroundStyle(Color.qcTextMuted)
            }
            .padding(.bottom, Spacing.lg)

            // Arabic — RTL, large.
            Text(verse.arabicText)
                .font(.qcArabic(28))
                .foregroundStyle(Color.qcText)
                .lineSpacing(18)
                .multilineTextAlignment(.trailing)
                .frame(maxWidth: .infinity, alignment: .trailing)
                .environment(\.layoutDirection, .rightToLeft)

            // Turkish meal — generous line height for reading.
            if let meal = verse.primaryTranslation?.text {
                Text(meal)
                    .font(.qcReading(18))
                    .foregroundStyle(Color.qcTextMuted)
                    .lineSpacing(8)
                    .multilineTextAlignment(.leading)
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(.top, Spacing.xl)
            }
        }
        .padding(.vertical, Spacing.xl)
        .frame(maxWidth: .infinity, alignment: .leading)
        .id("v\(verse.verseNumber)")
        .overlay(alignment: .bottom) {
            Rectangle()
                .fill(Color.qcBorder)
                .frame(height: 1)
        }
    }
}

#Preview {
    ScrollView {
        VStack(spacing: 0) {
            VerseBlock(verse: .init(
                surahNumber: 1, verseNumber: 1, globalVerseNumber: 1,
                juzNumber: 1, pageNumber: 1,
                arabicText: "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",
                translations: [Translation(
                    sourceCode: "elmalili", sourceName: "Elmalılı",
                    languageCode: "tr", author: "Elmalılı Hamdi Yazır",
                    text: "Rahmân ve Rahîm olan Allah'ın adıyla."
                )]
            ))
            VerseBlock(verse: .init(
                surahNumber: 1, verseNumber: 2, globalVerseNumber: 2,
                juzNumber: 1, pageNumber: 1,
                arabicText: "ٱلْحَمْدُ لِلَّهِ رَبِّ ٱلْعَٰلَمِينَ",
                translations: [Translation(
                    sourceCode: "elmalili", sourceName: "Elmalılı",
                    languageCode: "tr", author: "Elmalılı Hamdi Yazır",
                    text: "Hamd, âlemlerin Rabbi olan Allah'a mahsustur."
                )]
            ))
        }
        .padding(.horizontal, Spacing.lg)
    }
    .background(Color.qcBackground)
}
