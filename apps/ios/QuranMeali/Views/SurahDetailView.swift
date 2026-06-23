//
//  SurahDetailView.swift
//  QuranMeali
//
//  Reading view: editorial header + verse blocks.
//  Mirrors apps/web/src/pages/SurahDetailPage.tsx.
//

import SwiftUI

@MainActor
@Observable
final class SurahDetailModel {
    private(set) var detail: SurahDetail?
    private(set) var verses: [VerseSummary] = []
    private(set) var isLoading: Bool = false
    private(set) var error: String?

    private let number: Int

    init(number: Int) {
        self.number = number
    }

    func load() async {
        guard !isLoading else { return }
        guard detail == nil else { return }
        isLoading = true
        error = nil
        do {
            async let detailTask = ApiClient.shared.getSurahDetail(number: number)
            async let versesTask = ApiClient.shared.getSurahVerses(number: number)

            let (detail, paged) = try await (detailTask, versesTask)
            self.detail = detail
            self.verses = paged.items

            // Persist the last-read marker — top of the surah for now.
            LastReadStore.save(LastRead(
                surahNumber: detail.number,
                surahNameTurkish: detail.nameTurkish,
                verseNumber: 1,
                savedAt: Date()
            ))
        } catch is CancellationError {
            // ignored — view disappeared
        } catch let apiError as ApiError {
            self.error = apiError.errorDescription ?? "Bilinmeyen hata"
        } catch let other {
            self.error = other.localizedDescription
        }
        isLoading = false
    }
}

struct SurahDetailView: View {
    @State private var model: SurahDetailModel
    @Environment(\.dismiss) private var dismiss

    /// Optional verse to scroll to once the surah has loaded. Used when
    /// arriving from a search hit.
    private let targetVerse: Int?

    init(number: Int, targetVerse: Int? = nil) {
        _model = State(initialValue: SurahDetailModel(number: number))
        self.targetVerse = targetVerse
    }

    var body: some View {
        ScrollViewReader { proxy in
            ScrollView {
                VStack(spacing: 0) {
                    content
                }
                .frame(maxWidth: Spacing.readingMaxWidth)
                .frame(maxWidth: .infinity)
                .padding(.horizontal, Spacing.md)
                .padding(.bottom, Spacing.huge)
            }
            .background(Color.qcBackground.ignoresSafeArea())
            .navigationBarBackButtonHidden(true)
            .toolbar {
                ToolbarItem(placement: .topBarLeading) {
                    Button {
                        dismiss()
                    } label: {
                        Text("← SURELER")
                            .font(.qcMono(11))
                            .tracking(2.4)
                            .foregroundStyle(Color.qcTextMuted)
                    }
                }
            }
            .task { await model.load() }
            .onChange(of: model.verses.count) { _, count in
                guard let target = targetVerse, count > 0 else { return }
                withAnimation(.softSkillSlow) {
                    proxy.scrollTo("v\(target)", anchor: .top)
                }
            }
        }
    }

    @ViewBuilder
    private var content: some View {
        if let detail = model.detail {
            Header(detail: detail)
                .padding(.top, Spacing.xl)
                .padding(.bottom, Spacing.huge)

            VStack(spacing: 0) {
                ForEach(model.verses) { verse in
                    VerseBlock(verse: verse)
                }
            }
            .padding(.horizontal, Spacing.xs)
        } else if let err = model.error {
            errorState(err)
        } else {
            loadingState
        }
    }

    private var loadingState: some View {
        Text("YÜKLENİYOR")
            .font(.qcMono(11))
            .tracking(2.4)
            .foregroundStyle(Color.qcTextMuted)
            .padding(.vertical, Spacing.huge)
    }

    private func errorState(_ message: String) -> some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Hata")
            Text(message)
                .font(.qcReading(16))
                .foregroundStyle(Color.qcTextMuted)
                .multilineTextAlignment(.center)
            Button {
                Task { await model.load() }
            } label: {
                Text("Tekrar dene".uppercased())
                    .font(.qcMono(11))
                    .tracking(2.4)
                    .foregroundStyle(Color.qcAccent)
            }
            .padding(.top, Spacing.sm)
        }
        .frame(maxWidth: .infinity)
        .padding(.horizontal, Spacing.lg)
        .padding(.vertical, Spacing.huge)
    }
}

// MARK: - Header

private struct Header: View {
    let detail: SurahDetail

    var body: some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Sure \(detail.number) · \(detail.revelationPlace.turkishLabel) · \(detail.verseCount) ayet")

            Text(detail.nameTurkish)
                .font(.qcDisplay(60))
                .tracking(-1.0)
                .foregroundStyle(Color.qcText)
                .lineLimit(1)
                .minimumScaleFactor(0.5)

            Text(detail.nameArabic)
                .font(.qcArabic(44))
                .foregroundStyle(Color.qcAccent)
                .environment(\.layoutDirection, .rightToLeft)
                .padding(.top, Spacing.xs)

            Text(detail.nameTransliteration.uppercased())
                .font(.qcMono(11))
                .tracking(2.4)
                .foregroundStyle(Color.qcTextMuted)
                .padding(.top, Spacing.xxs)

            SectionRule()
                .padding(.top, Spacing.xl)
        }
        .frame(maxWidth: .infinity)
    }
}
