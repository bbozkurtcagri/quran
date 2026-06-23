//
//  LastRead.swift
//  QuranMeali
//
//  UserDefaults-backed reading position, mirroring apps/web/src/hooks/useLastRead.ts.
//

import Foundation

struct LastRead: Codable, Equatable {
    var surahNumber: Int
    var surahNameTurkish: String
    var verseNumber: Int
    var savedAt: Date
}

enum LastReadStore {
    private static let key = "qc-last-read"

    static func load() -> LastRead? {
        guard let data = UserDefaults.standard.data(forKey: key) else { return nil }
        return try? JSONDecoder().decode(LastRead.self, from: data)
    }

    static func save(_ entry: LastRead) {
        guard let data = try? JSONEncoder().encode(entry) else { return }
        UserDefaults.standard.set(data, forKey: key)
    }

    static func clear() {
        UserDefaults.standard.removeObject(forKey: key)
    }
}
