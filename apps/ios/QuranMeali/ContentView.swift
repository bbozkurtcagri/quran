//
//  ContentView.swift
//  QuranMeali
//

import SwiftUI

struct ContentView: View {
    @AppStorage("qc-appearance") private var appearanceRaw: String = AppearancePreference.system.rawValue

    var body: some View {
        TabView {
            BrowseTab()
                .tabItem {
                    Label("Sureler", systemImage: "book.closed")
                }

            SearchTab()
                .tabItem {
                    Label("Arama", systemImage: "magnifyingglass")
                }

            AboutTab()
                .tabItem {
                    Label("Hakkında", systemImage: "info.circle")
                }
        }
        .tint(Color.qcAccent)
        .preferredColorScheme(
            AppearancePreference(rawValue: appearanceRaw)?.colorScheme
        )
    }
}

// MARK: - Tabs

private struct BrowseTab: View {
    var body: some View {
        NavigationStack {
            SurahListView()
                .navigationDestination(for: Int.self) { surahNumber in
                    SurahDetailView(number: surahNumber)
                }
                .navigationDestination(for: SurahListItem.self) { surah in
                    SurahDetailView(number: surah.number)
                }
        }
    }
}

private struct SearchTab: View {
    var body: some View {
        NavigationStack {
            SearchView()
                .navigationDestination(for: SearchDestination.self) { dest in
                    SurahDetailView(
                        number: dest.surahNumber,
                        targetVerse: dest.verseNumber
                    )
                }
        }
    }
}

private struct AboutTab: View {
    var body: some View {
        NavigationStack {
            AboutView()
        }
    }
}

#Preview {
    ContentView()
}
