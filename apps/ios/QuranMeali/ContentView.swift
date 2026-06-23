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
                    SurahDetailView(number: surah.number)
                }
        }
    }
}

#Preview {
    ContentView()
}
