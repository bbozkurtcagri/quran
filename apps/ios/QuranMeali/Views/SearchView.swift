//
//  SearchView.swift
//  QuranMeali
//
//  Elmalılı meali içinde arama. Mirrors apps/web/src/pages/SearchPage.tsx —
//  keyword + semantic toggle, debounced 300ms, results stream into the same
//  SurahDetailView with a verse anchor.
//

import SwiftUI

@MainActor
@Observable
final class SearchModel {
    private(set) var results: [VerseSearchHit] = []
    private(set) var totalCount: Int64 = 0
    private(set) var isLoading: Bool = false
    private(set) var error: String?

    var query: String = ""
    var mode: SearchMode = .semantic

    private var debounceTask: Task<Void, Never>?
    private var fetchTask: Task<Void, Never>?

    func onQueryChanged() {
        debounceTask?.cancel()
        let snapshotQuery = query
        let snapshotMode = mode

        debounceTask = Task { [weak self] in
            try? await Task.sleep(for: .milliseconds(300))
            guard !Task.isCancelled else { return }
            await self?.fire(query: snapshotQuery, mode: snapshotMode)
        }
    }

    func onModeChanged() {
        // Mode change is intentional — fetch immediately, no debounce.
        debounceTask?.cancel()
        Task { [query, mode] in
            await self.fire(query: query, mode: mode)
        }
    }

    private func fire(query: String, mode: SearchMode) async {
        let trimmed = query.trimmingCharacters(in: .whitespaces)
        guard trimmed.count >= 2 else {
            fetchTask?.cancel()
            results = []
            totalCount = 0
            error = nil
            isLoading = false
            return
        }

        fetchTask?.cancel()
        isLoading = true
        error = nil

        fetchTask = Task { [weak self] in
            guard let self else { return }
            do {
                let paged = try await ApiClient.shared.searchVerses(
                    query: trimmed,
                    mode: mode,
                    page: 1,
                    pageSize: 20
                )
                guard !Task.isCancelled else { return }
                self.results = paged.items
                self.totalCount = paged.totalCount
            } catch is CancellationError {
                // ignored
            } catch let apiError as ApiError {
                self.error = apiError.errorDescription ?? "Bilinmeyen hata"
                self.results = []
                self.totalCount = 0
            } catch let other {
                self.error = other.localizedDescription
                self.results = []
                self.totalCount = 0
            }
            self.isLoading = false
        }
    }
}

struct SearchView: View {
    @State private var model = SearchModel()
    @FocusState private var inputFocused: Bool

    var body: some View {
        ScrollView {
            VStack(spacing: 0) {
                Header()
                    .padding(.top, Spacing.xxl)
                    .padding(.bottom, Spacing.lg)

                SectionRule()
                    .padding(.bottom, Spacing.xxl)

                searchField
                    .padding(.horizontal, Spacing.lg)
                    .padding(.bottom, Spacing.md)

                modeBar
                    .padding(.horizontal, Spacing.lg)
                    .padding(.bottom, Spacing.xl)

                content
            }
            .frame(maxWidth: Spacing.readingMaxWidth)
            .frame(maxWidth: .infinity)
            .padding(.horizontal, Spacing.md)
            .padding(.bottom, Spacing.huge)
        }
        .background(Color.qcBackground.ignoresSafeArea())
    }

    private var searchField: some View {
        TextField(
            "",
            text: $model.query,
            prompt: Text(model.mode == .semantic
                ? "Tahammül, takva, helal–haram, israf…"
                : "örn. sabır · namaz · adalet")
                .foregroundStyle(Color.qcTextMuted.opacity(0.6))
        )
        .font(.qcReading(22))
        .foregroundStyle(Color.qcText)
        .textInputAutocapitalization(.never)
        .autocorrectionDisabled()
        .focused($inputFocused)
        .submitLabel(.search)
        .padding(.vertical, Spacing.sm)
        .overlay(alignment: .bottom) {
            Rectangle()
                .fill(inputFocused ? Color.qcAccent : Color.qcBorder)
                .frame(height: 1)
        }
        .animation(.softSkill, value: inputFocused)
        .onChange(of: model.query) { _, _ in
            model.onQueryChanged()
        }
    }

    private var modeBar: some View {
        HStack {
            ModeSegmented(value: $model.mode) {
                model.onModeChanged()
            }
            Spacer()
            Text(model.mode == .semantic ? "KAVRAMSAL" : "BİREBİR EŞLEŞME")
                .font(.qcMono(10))
                .tracking(2.2)
                .foregroundStyle(Color.qcTextMuted)
        }
    }

    @ViewBuilder
    private var content: some View {
        if let err = model.error {
            errorState(err)
        } else if model.isLoading {
            Text("ARANIYOR")
                .font(.qcMono(11))
                .tracking(2.4)
                .foregroundStyle(Color.qcTextMuted)
                .padding(.vertical, Spacing.huge)
        } else if !model.results.isEmpty {
            VStack(spacing: 0) {
                HStack {
                    Eyebrow(text: totalLabel)
                    Spacer()
                    Eyebrow(text: "Elmalılı")
                }
                .padding(.vertical, Spacing.md)
                .overlay(alignment: .bottom) {
                    Rectangle().fill(Color.qcBorder).frame(height: 1)
                }

                ForEach(model.results) { hit in
                    NavigationLink(value: SearchDestination(hit: hit)) {
                        SearchResultRow(
                            hit: hit,
                            highlight: model.mode == .keyword ? model.query : ""
                        )
                    }
                    .buttonStyle(.plain)
                }
            }
            .padding(.horizontal, Spacing.lg)
        } else if model.query.trimmingCharacters(in: .whitespaces).count >= 2 {
            Text("Sonuç bulunamadı.")
                .font(.qcReading(16))
                .foregroundStyle(Color.qcTextMuted)
                .frame(maxWidth: .infinity)
                .padding(.vertical, Spacing.huge)
        } else {
            Text("Aramaya başlamak için bir kelime veya kavram yazın.")
                .font(.qcReading(16))
                .foregroundStyle(Color.qcTextMuted)
                .multilineTextAlignment(.center)
                .padding(.horizontal, Spacing.lg)
                .padding(.vertical, Spacing.huge)
        }
    }

    private func errorState(_ message: String) -> some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Hata")
            Text(message)
                .font(.qcReading(16))
                .foregroundStyle(Color.qcTextMuted)
                .multilineTextAlignment(.center)
        }
        .frame(maxWidth: .infinity)
        .padding(.vertical, Spacing.huge)
    }

    private var totalLabel: String {
        if model.mode == .semantic {
            return "İlk \(model.results.count) sonuç"
        }
        if model.totalCount == 0 { return "Sonuç yok" }
        if model.totalCount > Int64(model.results.count) {
            return "\(model.totalCount) sonuç · ilk \(model.results.count)"
        }
        return "\(model.totalCount) sonuç"
    }
}

// MARK: - Destination payload

/// Identifies a search-hit navigation so the NavigationStack can land us on
/// the right surah and verse.
struct SearchDestination: Hashable {
    let surahNumber: Int
    let verseNumber: Int

    init(hit: VerseSearchHit) {
        self.surahNumber = hit.surahNumber
        self.verseNumber = hit.verseNumber
    }
}

// MARK: - Pieces

private struct Header: View {
    var body: some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Elmalılı meali içinde arama")
            Text("Arama")
                .font(.qcDisplay(72))
                .tracking(-1.5)
                .foregroundStyle(Color.qcText)
                .lineLimit(1)
                .minimumScaleFactor(0.5)
        }
        .frame(maxWidth: .infinity)
    }
}

private struct ModeSegmented: View {
    @Binding var value: SearchMode
    let onChange: () -> Void

    var body: some View {
        HStack(spacing: 0) {
            chip(label: "Anlamsal", mode: .semantic)
            chip(label: "Sözcük", mode: .keyword)
        }
        .padding(2)
        .background(
            Capsule().fill(Color.qcAccent.opacity(0.04))
        )
        .overlay(
            Capsule().stroke(Color.qcBorder, lineWidth: 1)
        )
    }

    @ViewBuilder
    private func chip(label: String, mode: SearchMode) -> some View {
        let active = value == mode
        Button {
            value = mode
            onChange()
        } label: {
            Text(label.uppercased())
                .font(.qcMono(10))
                .tracking(2.0)
                .foregroundStyle(active ? Color.qcAccent : Color.qcTextMuted)
                .padding(.horizontal, Spacing.md)
                .padding(.vertical, 6)
                .background(
                    Capsule().fill(active ? Color.qcAccentSoft : .clear)
                )
                .animation(.softSkill, value: active)
        }
        .buttonStyle(.plain)
    }
}
