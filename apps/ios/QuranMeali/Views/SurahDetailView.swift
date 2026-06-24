//
//  SurahDetailView.swift
//  QuranMeali
//
//  Reading view: editorial header + verse blocks.
//
//  Last-read takibi PreferenceKey üzerinden — her VerseBlock kendi y-offset'ini
//  ScrollView coordinate space'ine yazıyor, parent en üstte hangi ayet olduğunu
//  hesaplıyor. LazyVStack ile deterministik scroll kuramadığımız için (variable
//  height meal prose tahminleri bozuyor) düz VStack kullanıyoruz.
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
    let initialVerse: Int

    init(number: Int, initialVerse: Int = 1) {
        self.number = number
        self.initialVerse = initialVerse
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
            persistLastRead(verseNumber: initialVerse, detail: detail)
        } catch is CancellationError {
            // ignored
        } catch let apiError as ApiError {
            self.error = apiError.errorDescription ?? "Bilinmeyen hata"
        } catch let other {
            self.error = other.localizedDescription
        }
        isLoading = false
    }

    func updateLastRead(verseNumber: Int) {
        guard let detail else { return }
        persistLastRead(verseNumber: verseNumber, detail: detail)
    }

    private func persistLastRead(verseNumber: Int, detail: SurahDetail) {
        LastReadStore.save(LastRead(
            surahNumber: detail.number,
            surahNameTurkish: detail.nameTurkish,
            verseNumber: verseNumber,
            savedAt: Date()
        ))
    }
}

/// Her VerseBlock kendi ScrollView-içi minY'sini bu key altında publish eder;
/// parent en yüksek pozisyondaki ayeti seçer.
private struct VerseOffsetsKey: PreferenceKey {
    static var defaultValue: [Int: CGFloat] = [:]
    static func reduce(value: inout [Int: CGFloat], nextValue: () -> [Int: CGFloat]) {
        value.merge(nextValue(), uniquingKeysWith: { _, new in new })
    }
}

struct SurahDetailView: View {
    @State private var model: SurahDetailModel
    @Environment(\.dismiss) private var dismiss

    private let targetVerse: Int?

    // En üstte (navigation bar'ın hemen altında) görünen ayetin numarası.
    @State private var topmostVerseNumber: Int = 1
    // Programatik scroll süresince true; ara pozisyonların save'i tetiklemesini engeller.
    @State private var isProgrammaticScrolling: Bool = false

    private static let scrollSpace = "qcVerseScroll"

    init(number: Int, targetVerse: Int? = nil) {
        _model = State(initialValue: SurahDetailModel(
            number: number,
            initialVerse: targetVerse ?? 1
        ))
        self.targetVerse = targetVerse
    }

    var body: some View {
        ScrollViewReader { proxy in
            ScrollView {
                VStack(spacing: 0) {
                    if let detail = model.detail {
                        Header(detail: detail)
                            .padding(.top, Spacing.xl)
                            .padding(.bottom, Spacing.huge)

                        // VStack non-lazy — tüm 286 ayet bile baştan render edilir;
                        // bu sayede scrollTo deterministik çalışır ve PreferenceKey
                        // ile her ayetin gerçek pozisyonu okunabilir.
                        VStack(spacing: 0) {
                            ForEach(model.verses) { verse in
                                VerseBlock(verse: verse)
                                    .id(verse.verseNumber)
                                    .background(versePositionProbe(for: verse.verseNumber))
                            }
                        }
                        .padding(.horizontal, Spacing.xs)
                    } else if let err = model.error {
                        errorState(err)
                    } else {
                        loadingState
                    }
                }
                .frame(maxWidth: Spacing.readingMaxWidth)
                .frame(maxWidth: .infinity)
                .padding(.horizontal, Spacing.md)
                .padding(.bottom, Spacing.huge)
            }
            .coordinateSpace(name: Self.scrollSpace)
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
            // Verses geldikten sonra hedef ayete kaydır — VStack non-lazy
            // olduğu için tek scrollTo yeterli ve doğru pozisyona oturuyor.
            .onChange(of: model.verses.count) { _, count in
                guard let target = targetVerse, count > 0 else { return }
                isProgrammaticScrolling = true
                DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
                    withAnimation(.softSkillSlow) {
                        proxy.scrollTo(target, anchor: .top)
                    }
                    // Animasyon süresi + buffer; sonra save'i serbest bırak.
                    DispatchQueue.main.asyncAfter(deadline: .now() + 0.8) {
                        isProgrammaticScrolling = false
                        topmostVerseNumber = target
                        model.updateLastRead(verseNumber: target)
                    }
                }
            }
            // Y-offset'lerden en üstte olan ayeti hesapla.
            //   minY ≤ ~80 (navbar + safe area) ve en büyük (en alta yakın) = currently topmost
            .onPreferenceChange(VerseOffsetsKey.self) { offsets in
                guard !isProgrammaticScrolling, !offsets.isEmpty else { return }
                let candidate = offsets
                    .filter { $0.value <= 80 }
                    .max(by: { $0.value < $1.value })?.key
                if let n = candidate, n != topmostVerseNumber {
                    topmostVerseNumber = n
                    model.updateLastRead(verseNumber: n)
                }
            }
        }
    }

    // Verse'in ScrollView içindeki minY'sini PreferenceKey'e yazan invisible probe.
    private func versePositionProbe(for verseNumber: Int) -> some View {
        GeometryReader { geo in
            Color.clear
                .preference(
                    key: VerseOffsetsKey.self,
                    value: [verseNumber: geo.frame(in: .named(Self.scrollSpace)).minY]
                )
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
