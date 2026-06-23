//
//  SurahListView.swift
//  QuranMeali
//

import SwiftUI

@MainActor
@Observable
final class SurahListModel {
    private(set) var surahs: [SurahListItem] = []
    private(set) var isLoading: Bool = false
    private(set) var error: String? = nil

    var filterText: String = ""

    var filtered: [SurahListItem] {
        let normalized = Self.normalize(filterText)
        guard !normalized.isEmpty else { return surahs }

        return surahs.filter { s in
            let candidates = [
                Self.normalize(s.nameTurkish),
                Self.normalize(s.nameTransliteration),
                s.nameArabic,
                String(s.number),
                String(format: "%02d", s.number),
            ]
            return candidates.contains { $0.contains(normalized) }
        }
    }

    func load() async {
        guard !isLoading else { return }
        isLoading = true
        error = nil
        do {
            surahs = try await ApiClient.shared.getSurahs()
        } catch is CancellationError {
            // The view disappeared; leave state untouched.
        } catch let apiError as ApiError {
            self.error = apiError.errorDescription ?? "Bilinmeyen hata"
        } catch let other {
            self.error = other.localizedDescription
        }
        isLoading = false
    }

    /// Turkish-aware light normaliser, mirrors apps/web/src/pages/SurahListPage.tsx.
    private static func normalize(_ input: String) -> String {
        let lowercased = input.lowercased(with: Locale(identifier: "tr_TR"))
        let folded = lowercased.folding(options: [.diacriticInsensitive, .widthInsensitive], locale: nil)
        var result = ""
        for ch in folded {
            if ch.isLetter || ch.isNumber {
                if ch == "ı" || ch == "İ" {
                    result.append("i")
                } else {
                    result.append(ch)
                }
            } else if ch.isWhitespace {
                result.append(" ")
            }
        }
        return result.trimmingCharacters(in: .whitespaces)
    }
}

struct SurahListView: View {
    @State private var model = SurahListModel()

    var body: some View {
        ScrollView {
            VStack(spacing: 0) {
                Hero()
                    .padding(.top, Spacing.xxl)
                    .padding(.bottom, Spacing.lg)

                SectionRule()
                    .padding(.bottom, Spacing.xxl)

                FilterField(text: $model.filterText)
                    .padding(.horizontal, Spacing.lg)
                    .padding(.bottom, Spacing.lg)

                content
            }
            .frame(maxWidth: Spacing.browseMaxWidth)
            .frame(maxWidth: .infinity)
            .padding(.horizontal, Spacing.md)
            .padding(.bottom, Spacing.xxl)
        }
        .background(Color.qcBackground.ignoresSafeArea())
        .task { await model.load() }
    }

    @ViewBuilder
    private var content: some View {
        if let error = model.error {
            errorState(error)
        } else if model.isLoading && model.surahs.isEmpty {
            loadingState
        } else if model.filtered.isEmpty {
            emptyFilterState
        } else {
            VStack(spacing: 0) {
                ForEach(model.filtered) { surah in
                    NavigationLink(value: surah) {
                        SurahRow(surah: surah)
                    }
                    .buttonStyle(.plain)
                }
            }
            .padding(.horizontal, Spacing.lg)
        }
    }

    // MARK: - States

    private var loadingState: some View {
        Text("Yükleniyor".uppercased())
            .font(.qcMono(11))
            .tracking(2.4)
            .foregroundStyle(Color.qcTextMuted)
            .padding(.vertical, Spacing.xxxl)
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
        .padding(.vertical, Spacing.xxxl)
    }

    private var emptyFilterState: some View {
        VStack(spacing: Spacing.sm) {
            Text("\"\(model.filterText)\" için sure bulunamadı.")
                .font(.qcReading(16))
                .foregroundStyle(Color.qcTextMuted)
                .multilineTextAlignment(.center)
            Button {
                model.filterText = ""
            } label: {
                Text("Aramayı temizle".uppercased())
                    .font(.qcMono(11))
                    .tracking(2.4)
                    .foregroundStyle(Color.qcAccent)
            }
        }
        .frame(maxWidth: .infinity)
        .padding(.vertical, Spacing.xxxl)
    }
}

// MARK: - Pieces

private struct Hero: View {
    var body: some View {
        VStack(spacing: Spacing.md) {
            Eyebrow(text: "Kur'an-ı Kerim · 114 sure · 6236 ayet")
            Text("Sureler")
                .font(.qcDisplay(72))
                .tracking(-1.5)
                .foregroundStyle(Color.qcText)
                .lineLimit(1)
                .minimumScaleFactor(0.5)
        }
        .frame(maxWidth: .infinity)
    }
}

private struct FilterField: View {
    @Binding var text: String

    var body: some View {
        HStack(spacing: Spacing.sm) {
            Image(systemName: "magnifyingglass")
                .font(.system(size: 14, weight: .regular))
                .foregroundStyle(Color.qcTextMuted)
            TextField(
                "",
                text: $text,
                prompt: Text("Sure ara — adı veya numarası")
                    .foregroundStyle(Color.qcTextMuted.opacity(0.6))
            )
            .font(.qcReading(17))
            .textInputAutocapitalization(.never)
            .autocorrectionDisabled()
            .submitLabel(.search)
            .foregroundStyle(Color.qcText)
        }
        .padding(.vertical, Spacing.sm)
        .overlay(alignment: .bottom) {
            Rectangle()
                .fill(Color.qcBorder)
                .frame(height: 1)
        }
        .frame(maxWidth: 420)
        .frame(maxWidth: .infinity)
    }
}

// MARK: - Preview

#Preview("Light") {
    NavigationStack {
        SurahListView()
    }
    .preferredColorScheme(.light)
}

#Preview("Dark") {
    NavigationStack {
        SurahListView()
    }
    .preferredColorScheme(.dark)
}
