//
//  ContinueChip.swift
//  QuranMeali
//
//  Inline call-to-action shown on the surah list when a last-read marker
//  exists. The chip body is purely presentational so the parent can wrap
//  it in a NavigationLink without nested-Button conflicts; the dismiss
//  affordance is laid out separately by the parent.
//

import SwiftUI

/// NavigationStack value tipi — chip'e tıklayınca surah numarası ve son ayet
/// birlikte taşınır ki detail view doğru ayete scroll edebilsin.
struct ResumeDestination: Hashable {
    let surahNumber: Int
    let verseNumber: Int
}

struct ContinueChipBody: View {
    let lastRead: LastRead

    var body: some View {
        HStack(alignment: .center, spacing: Spacing.md) {
            VStack(alignment: .leading, spacing: Spacing.xxs) {
                Text("DEVAM ET")
                    .font(.qcMono(10))
                    .tracking(2.2)
                    .foregroundStyle(Color.qcAccent)
                Text(lastRead.surahNameTurkish)
                    .font(.qcDisplay(22))
                    .foregroundStyle(Color.qcText)
                Text("Sure \(lastRead.surahNumber) · Ayet \(lastRead.verseNumber)")
                    .font(.qcMono(11))
                    .tracking(1.6)
                    .foregroundStyle(Color.qcTextMuted)
            }

            Spacer(minLength: 0)

            Text("AÇ ↗")
                .font(.qcMono(10))
                .tracking(2.2)
                .foregroundStyle(Color.qcAccent)
                .padding(.horizontal, Spacing.md)
                .padding(.vertical, Spacing.xs)
                .overlay(
                    Capsule().stroke(Color.qcAccent.opacity(0.45), lineWidth: 1)
                )
        }
        .padding(.vertical, Spacing.lg)
        .padding(.horizontal, Spacing.lg)
        .background(
            RoundedRectangle(cornerRadius: 4, style: .continuous)
                .fill(Color.qcAccentSoft)
        )
        .contentShape(Rectangle())
    }
}
