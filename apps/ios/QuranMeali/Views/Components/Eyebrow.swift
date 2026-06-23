//
//  Eyebrow.swift
//  QuranMeali
//

import SwiftUI

/// Small, all-caps, monospace label used above headings and as section heads.
struct Eyebrow: View {
    let text: String

    var body: some View {
        Text(text.uppercased())
            .font(.qcMono(10))
            .tracking(2.4)
            .foregroundStyle(Color.qcTextMuted)
    }
}

#Preview {
    VStack(spacing: 12) {
        Eyebrow(text: "Kur'an-ı Kerim · 114 sure")
        Eyebrow(text: "Hakkında")
    }
    .padding()
}
