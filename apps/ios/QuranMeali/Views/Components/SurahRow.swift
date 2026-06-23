//
//  SurahRow.swift
//  QuranMeali
//

import SwiftUI

struct SurahRow: View {
    let surah: SurahListItem

    var body: some View {
        HStack(alignment: .center, spacing: Spacing.lg) {
            // Order index (mono, dimmed).
            Text(String(format: "%02d", surah.number))
                .font(.qcMono(12))
                .tracking(0.5)
                .foregroundStyle(Color.qcTextMuted)
                .frame(width: 28, alignment: .leading)

            // Turkish name + eyebrow line.
            VStack(alignment: .leading, spacing: Spacing.xs) {
                Text(surah.nameTurkish)
                    .font(.qcDisplay(24))
                    .tracking(-0.3)
                    .foregroundStyle(Color.qcText)
                    .lineLimit(1)

                Eyebrow(text: "\(surah.revelationPlace.turkishLabel) · \(surah.verseCount) ayet · \(surah.nameTransliteration)")
            }
            .frame(maxWidth: .infinity, alignment: .leading)

            // Arabic name (RTL).
            Text(surah.nameArabic)
                .font(.qcArabic(22))
                .foregroundStyle(Color.qcText)
                .environment(\.layoutDirection, .rightToLeft)
        }
        .padding(.vertical, Spacing.lg)
        .padding(.horizontal, Spacing.md)
        .frame(maxWidth: .infinity, alignment: .leading)
        .contentShape(Rectangle())
        .overlay(alignment: .bottom) {
            Rectangle()
                .fill(Color.qcBorder)
                .frame(height: 1)
        }
    }
}

#Preview {
    VStack(spacing: 0) {
        SurahRow(surah: .init(number: 1, nameArabic: "الفاتحة", nameTurkish: "Fâtiha",
                              nameTransliteration: "Al-Fatihah",
                              verseCount: 7, revelationPlace: .meccan))
        SurahRow(surah: .init(number: 2, nameArabic: "البقرة", nameTurkish: "Bakara",
                              nameTransliteration: "Al-Baqarah",
                              verseCount: 286, revelationPlace: .medinan))
        SurahRow(surah: .init(number: 112, nameArabic: "الإخلاص", nameTurkish: "İhlas",
                              nameTransliteration: "Al-Ikhlas",
                              verseCount: 4, revelationPlace: .meccan))
    }
    .padding(.horizontal, Spacing.md)
    .background(Color.qcBackground)
}
