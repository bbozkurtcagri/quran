//
//  Verse.swift
//  QuranMeali
//

import Foundation

struct Translation: Decodable, Hashable {
    let sourceCode: String
    let sourceName: String
    let languageCode: String
    let author: String
    let text: String
}

struct VerseSummary: Decodable, Identifiable, Hashable {
    let surahNumber: Int
    let verseNumber: Int
    let globalVerseNumber: Int
    let juzNumber: Int
    let pageNumber: Int
    let arabicText: String
    let translations: [Translation]

    var id: Int { globalVerseNumber }

    /// First available translation (the default source surfaces first
    /// thanks to backend ordering).
    var primaryTranslation: Translation? { translations.first }
}

struct VerseSearchHit: Decodable, Identifiable, Hashable {
    let surahNumber: Int
    let surahNameTurkish: String
    let verseNumber: Int
    let globalVerseNumber: Int
    let arabicText: String
    let translationSourceCode: String
    let translationText: String

    var id: String { "\(globalVerseNumber)-\(translationSourceCode)" }
}

struct PagedResult<T: Decodable & Hashable>: Decodable, Hashable {
    let items: [T]
    let totalCount: Int64
    let page: Int
    let pageSize: Int
    let totalPages: Int
    let hasNextPage: Bool
    let hasPreviousPage: Bool
}
