//
//  QuranMealiApp.swift
//  QuranMeali
//

import SwiftUI

@main
struct QuranMealiApp: App {
    init() {
        // Once on launch we print the registered font families so we can verify
        // the bundled TTFs resolved to the PostScript names DesignTokens expects.
        #if DEBUG
        FontDebug.dumpAvailableFonts()
        #endif
    }

    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}
