//
//  Surah.swift
//  QuranMeali
//
//  Shapes mirror the backend DTOs at
//  QuranCompanion.Application.Features.Surahs.Dtos.*
//

import Foundation

enum RevelationPlace: String, Decodable {
    case meccan = "Meccan"
    case medinan = "Medinan"
    case unknown = "Unknown"

    var turkishLabel: String {
        switch self {
        case .meccan: return "Mekkî"
        case .medinan: return "Medenî"
        case .unknown: return "—"
        }
    }
}

struct SurahListItem: Decodable, Identifiable, Hashable {
    let number: Int
    let nameArabic: String
    let nameTurkish: String
    let nameTransliteration: String
    let verseCount: Int
    let revelationPlace: RevelationPlace

    var id: Int { number }
}

struct SurahDetail: Decodable, Identifiable, Hashable {
    let number: Int
    let nameArabic: String
    let nameTurkish: String
    let nameTransliteration: String
    let verseCount: Int
    let revelationPlace: RevelationPlace
    let displayOrder: Int

    var id: Int { number }
}
