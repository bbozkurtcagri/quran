//
//  DesignTokens.swift
//  QuranMeali
//
//  Shared visual language for the iOS client.
//  Mirrors `docs/design-language.md` and the web's `theme.css`.
//
//  Custom fonts (Instrument Serif, Newsreader, Geist, Geist Mono, Amiri) are
//  added in a follow-up step — for now everything resolves to system faces
//  so the app runs immediately.
//

import SwiftUI

// MARK: - Colour palette

extension Color {
    // Warm bone / warm dark
    static let qcBackground       = Color("BackgroundColor", bundle: nil).fallback(light: 0xFBFBFA, dark: 0x1A1612)
    static let qcSurface          = Color("SurfaceColor",    bundle: nil).fallback(light: 0xFFFFFF, dark: 0x221C16)
    static let qcText             = Color("TextColor",       bundle: nil).fallback(light: 0x111111, dark: 0xF2E9DA)
    static let qcTextMuted        = Color("TextMutedColor",  bundle: nil).fallback(light: 0x6B6660, dark: 0x9E907C)
    static let qcAccent           = Color("AccentColor",     bundle: nil).fallback(light: 0x346538, dark: 0x7FB88A)

    /// Hairline border — black/white at ~7% opacity, depending on appearance.
    static let qcBorder           = Color.qcText.opacity(0.07)

    /// Tinted hover / active surface for the accent.
    static let qcAccentSoft       = Color.qcAccent.opacity(0.12)
}

// MARK: - Color asset fallback helper
//
// Until the asset catalog actually carries each token, every reference
// silently falls back to a hex pair (light, dark). Once an entry exists in
// Assets.xcassets the named-asset wins and this helper becomes a no-op for
// that token.

private extension Color {
    func fallback(light: UInt32, dark: UInt32) -> Color {
        // `Color("Name")` does not error when the asset is missing — it just
        // resolves to a transparent fallback. We can't introspect that here,
        // so we always wrap with a dynamic UIColor based on userInterfaceStyle.
        return Color(uiColor: UIColor { trait in
            trait.userInterfaceStyle == .dark ? UIColor(hex: dark) : UIColor(hex: light)
        })
    }
}

private extension UIColor {
    convenience init(hex: UInt32, alpha: CGFloat = 1) {
        let r = CGFloat((hex >> 16) & 0xFF) / 255
        let g = CGFloat((hex >>  8) & 0xFF) / 255
        let b = CGFloat( hex        & 0xFF) / 255
        self.init(red: r, green: g, blue: b, alpha: alpha)
    }
}

// MARK: - Typography
//
// `Font.qc*` properties keep the call-site noise low and let us swap the
// underlying family in one place once custom fonts are bundled.

extension Font {
    /// Editorial display — used for the wordmark and hero H1s. Currently
    /// SwiftUI system serif until Instrument Serif is bundled.
    static func qcDisplay(_ size: CGFloat, italic: Bool = false) -> Font {
        let base = Font.system(size: size, weight: .regular, design: .serif)
        return italic ? base.italic() : base
    }

    /// Reading body for Turkish meal prose. Becomes Newsreader once bundled.
    static func qcReading(_ size: CGFloat = 17) -> Font {
        Font.system(size: size, weight: .regular, design: .serif)
    }

    /// UI sans-serif. Becomes Geist once bundled.
    static func qcUI(_ size: CGFloat = 15, weight: Font.Weight = .regular) -> Font {
        Font.system(size: size, weight: weight, design: .default)
    }

    /// Eyebrows, verse numbers, metadata. Becomes Geist Mono once bundled.
    static func qcMono(_ size: CGFloat = 11) -> Font {
        Font.system(size: size, weight: .regular, design: .monospaced)
    }

    /// Arabic mushaf. Becomes Amiri once bundled. iOS already ships several
    /// usable Naskh faces; we fall back to the platform Arabic UI text.
    static func qcArabic(_ size: CGFloat = 28) -> Font {
        Font.custom("Geeza Pro", size: size, relativeTo: .body)
    }
}

// MARK: - Spacing

enum Spacing {
    static let xxs: CGFloat = 4
    static let xs: CGFloat = 8
    static let sm: CGFloat = 12
    static let md: CGFloat = 16
    static let lg: CGFloat = 24
    static let xl: CGFloat = 32
    static let xxl: CGFloat = 48
    static let xxxl: CGFloat = 64
    static let huge: CGFloat = 96

    /// Reading column width (matches web's max-w-3xl ≈ 672 px).
    static let readingMaxWidth: CGFloat = 672
    /// Browse column width (matches web's max-w-5xl ≈ 1024 px).
    static let browseMaxWidth: CGFloat = 720 // capped for iPhone landscape
}

// MARK: - Motion

extension Animation {
    /// Single canonical curve used everywhere. Matches
    /// cubic-bezier(0.16, 1, 0.3, 1) on the web.
    static var softSkill: Animation {
        .timingCurve(0.16, 1, 0.3, 1, duration: 0.20)
    }

    static var softSkillSlow: Animation {
        .timingCurve(0.16, 1, 0.3, 1, duration: 0.60)
    }
}
