//
//  SearchResultRow.swift
//  QuranMeali
//

import SwiftUI

struct SearchResultRow: View {
    let hit: VerseSearchHit
    /// In keyword mode we underline matches; in semantic mode `highlight`
    /// is empty and the text renders untouched.
    let highlight: String

    var body: some View {
        VStack(alignment: .leading, spacing: Spacing.sm) {
            HStack(alignment: .firstTextBaseline) {
                Text("\(hit.surahNameTurkish) · \(hit.surahNumber):\(hit.verseNumber)")
                    .font(.qcMono(11))
                    .tracking(2.2)
                    .textCase(.uppercase)
                    .foregroundStyle(Color.qcTextMuted)

                Spacer(minLength: 0)

                Text("OKU ↗")
                    .font(.qcMono(10))
                    .tracking(2.2)
                    .foregroundStyle(Color.qcAccent)
            }

            highlightedText
                .font(.qcReading(17))
                .foregroundStyle(Color.qcText)
                .lineSpacing(6)
                .multilineTextAlignment(.leading)
                .frame(maxWidth: .infinity, alignment: .leading)
        }
        .padding(.vertical, Spacing.lg)
        .frame(maxWidth: .infinity, alignment: .leading)
        .contentShape(Rectangle())
        .overlay(alignment: .bottom) {
            Rectangle().fill(Color.qcBorder).frame(height: 1)
        }
    }

    private var highlightedText: Text {
        let trimmed = highlight.trimmingCharacters(in: .whitespaces)
        if trimmed.count < 2 {
            return Text(hit.translationText)
        }
        return buildHighlighted(hit.translationText, query: trimmed)
    }

    private func buildHighlighted(_ source: String, query: String) -> Text {
        // Turkish-locale case-insensitive split, preserving original casing.
        let locale = Locale(identifier: "tr_TR")
        let lower = source.lowercased(with: locale)
        let target = query.lowercased(with: locale)

        var result = Text("")
        var i = lower.startIndex
        let end = lower.endIndex

        while i < end {
            if let range = lower.range(of: target, range: i..<end) {
                if range.lowerBound > i {
                    let pre = String(source[i..<range.lowerBound])
                    result = result + Text(pre)
                }
                let hitText = String(source[range])
                result = result
                    + Text(hitText)
                    .foregroundColor(Color.qcAccent)
                    .underline(true, color: Color.qcAccent.opacity(0.5))
                i = range.upperBound
            } else {
                let rest = String(source[i..<end])
                result = result + Text(rest)
                break
            }
        }
        return result
    }
}
