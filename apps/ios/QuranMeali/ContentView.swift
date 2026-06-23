//
//  ContentView.swift
//  QuranMeali
//

import SwiftUI

struct ContentView: View {
    var body: some View {
        NavigationStack {
            SurahListView()
                .navigationDestination(for: SurahListItem.self) { surah in
                    // SurahDetailView arrives in the next step; for now we
                    // surface a placeholder so navigation works end-to-end.
                    PlaceholderDetail(surah: surah)
                }
        }
    }
}

private struct PlaceholderDetail: View {
    let surah: SurahListItem

    var body: some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Sure \(surah.number) · \(surah.revelationPlace.turkishLabel)")
            Text(surah.nameTurkish)
                .font(.qcDisplay(56))
                .foregroundStyle(Color.qcText)
            Text(surah.nameArabic)
                .font(.qcArabic(36))
                .foregroundStyle(Color.qcAccent)
            Text("Ayetler bir sonraki adımda gelecek.")
                .font(.qcMono(11))
                .tracking(2.4)
                .foregroundStyle(Color.qcTextMuted)
                .padding(.top, Spacing.lg)
        }
        .padding()
        .frame(maxWidth: .infinity, maxHeight: .infinity)
        .background(Color.qcBackground.ignoresSafeArea())
    }
}

#Preview {
    ContentView()
}
