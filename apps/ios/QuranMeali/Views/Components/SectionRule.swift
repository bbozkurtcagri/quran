//
//  SectionRule.swift
//  QuranMeali
//

import SwiftUI

/// Decorative editorial rule — two hairlines around an accent dot.
struct SectionRule: View {
    var body: some View {
        HStack(spacing: 16) {
            Rectangle()
                .fill(Color.qcBorder)
                .frame(width: 48, height: 1)
            Text("◆")
                .foregroundStyle(Color.qcAccent)
                .font(.qcMono(12))
            Rectangle()
                .fill(Color.qcBorder)
                .frame(width: 48, height: 1)
        }
    }
}

#Preview {
    SectionRule()
        .padding()
}
