//
//  ContentView.swift
//  QuranMeali
//

import SwiftUI

struct ContentView: View {
    @AppStorage("qc-appearance") private var appearanceRaw: String = AppearancePreference.system.rawValue

    // Animated splash uygulama açılırken bir kez gösterilir; statik launch screen
    // ile aynı renkte olduğundan göze çarpan bir sıçrama yok.
    @State private var showSplash: Bool = true

    var body: some View {
        ZStack {
            mainTabs

            if showSplash {
                SplashView()
                    .transition(.opacity)
                    .zIndex(1)
            }
        }
        .preferredColorScheme(
            AppearancePreference(rawValue: appearanceRaw)?.colorScheme
        )
        .task {
            // Splash en az 1.7s kalır; çoğu cihazda API çağrısı bu sürede tamamlanır.
            try? await Task.sleep(for: .milliseconds(1700))
            withAnimation(.softSkillSlow) {
                showSplash = false
            }
        }
    }

    private var mainTabs: some View {
        TabView {
            BrowseTab()
                .tabItem { Label("Sureler", systemImage: "book.closed") }

            SearchTab()
                .tabItem { Label("Arama", systemImage: "magnifyingglass") }

            AboutTab()
                .tabItem { Label("Hakkında", systemImage: "info.circle") }
        }
        .tint(Color.qcAccent)
    }
}

// MARK: - Tabs

private struct BrowseTab: View {
    var body: some View {
        NavigationStack {
            SurahListView()
                .navigationDestination(for: SurahListItem.self) { surah in
                    // Listeden tıklama — her zaman başa scroll.
                    SurahDetailView(number: surah.number)
                }
                .navigationDestination(for: ResumeDestination.self) { dest in
                    // Devam et — last-read ayetinden devam.
                    SurahDetailView(number: dest.surahNumber, targetVerse: dest.verseNumber)
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
