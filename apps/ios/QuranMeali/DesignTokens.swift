//
//  DesignTokens.swift
//  QuranMeali
//
//  Shared visual language for the iOS client. Mirrors `docs/design-language.md`
//  and the web's `theme.css`. Custom fonts ship under QuranMeali/Fonts/ and
//  are registered through Info.plist (UIAppFonts).
//

import SwiftUI

// MARK: - Colour palette
// Warm parchment background and muted emerald accent — kept identical to the web tokens.

extension Color {
    static let qcBackground       = Color("BackgroundColor", bundle: nil).fallback(light: 0xFBFBFA, dark: 0x1A1612)
    static let qcSurface          = Color("SurfaceColor",    bundle: nil).fallback(light: 0xFFFFFF, dark: 0x221C16)
    static let qcText             = Color("TextColor",       bundle: nil).fallback(light: 0x111111, dark: 0xF2E9DA)
    static let qcTextMuted        = Color("TextMutedColor",  bundle: nil).fallback(light: 0x6B6660, dark: 0x9E907C)
    static let qcAccent           = Color("AccentColor",     bundle: nil).fallback(light: 0x346538, dark: 0x7FB88A)

    // Hairline ~7% of text colour — paints every list divider and field underline.
    static let qcBorder           = Color.qcText.opacity(0.07)

    // Tinted accent surface for chips and toggle backgrounds.
    static let qcAccentSoft       = Color.qcAccent.opacity(0.12)
}

// Asset-catalog fallback: keeps the build alive until each token gets a real
// colour set in Assets.xcassets — `Color("Name")` resolves to clear otherwise.
private extension Color {
    func fallback(light: UInt32, dark: UInt32) -> Color {
        Color(uiColor: UIColor { trait in
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
// Each role resolves to a bundled face; `Font.custom` is wrapped to keep
// call-sites compact and to centralise font-family changes.

private enum FontName {
    // Display — Instrument Serif (editorial, large titles).
    static let serif         = "InstrumentSerif-Regular"
    static let serifItalic   = "InstrumentSerif-Italic"
    // Reading body — Newsreader variable font; ships as a 16pt-anchored named instance.
    static let reading       = "Newsreader16pt-Regular"
    static let readingItalic = "Newsreader16pt-Italic"
    // UI sans — Geist (neutral, used for labels and chrome).
    static let ui            = "Geist-Regular"
    static let uiMedium      = "Geist-Medium"
    // Monospace — Geist Mono (eyebrows, verse counters, captions).
    static let mono          = "GeistMono-Regular"
    // Arabic mushaf — Amiri (Naskh, the canonical typesetting choice for Qur'an text).
    static let arabic        = "Amiri-Regular"
}

extension Font {
    /// Editorial display — hero H1s, surah titles, brand wordmark.
    static func qcDisplay(_ size: CGFloat, italic: Bool = false) -> Font {
        Font.custom(italic ? FontName.serifItalic : FontName.serif, size: size)
    }

    /// Reading body — Turkish meal prose. Variable wght axis honoured via SwiftUI's weight modifier.
    static func qcReading(_ size: CGFloat = 17, weight: Font.Weight = .regular) -> Font {
        Font.custom(FontName.reading, size: size).weight(weight)
    }

    /// UI sans — buttons, navigation labels, small chrome.
    static func qcUI(_ size: CGFloat = 15, weight: Font.Weight = .regular) -> Font {
        let name = weight == .medium ? FontName.uiMedium : FontName.ui
        return Font.custom(name, size: size)
    }

    /// Monospace — eyebrows, verse numbers, metadata captions.
    static func qcMono(_ size: CGFloat = 11) -> Font {
        Font.custom(FontName.mono, size: size)
    }

    /// Arabic mushaf — verse Arabic + surah name Arabic.
    static func qcArabic(_ size: CGFloat = 28) -> Font {
        Font.custom(FontName.arabic, size: size, relativeTo: .body)
    }
}

// MARK: - Spacing
// 4-8-12-16-24-32-48-64-96 ladder matches the web token set.

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

    // Reading column ≈ web's max-w-3xl, capped so iPad/landscape don't sprawl.
    static let readingMaxWidth: CGFloat = 672
    // Browse column kept narrower than web — iPhone landscape needs the cap.
    static let browseMaxWidth: CGFloat = 720
}

// MARK: - Motion
// Single canonical curve matches web's cubic-bezier(0.16, 1, 0.3, 1) — "soft skill".

extension Animation {
    static var softSkill: Animation {
        .timingCurve(0.16, 1, 0.3, 1, duration: 0.20)
    }

    static var softSkillSlow: Animation {
        .timingCurve(0.16, 1, 0.3, 1, duration: 0.60)
    }
}

// MARK: - Debug helpers
// Prints every registered font family on first launch in DEBUG builds — used
// to confirm the bundled TTFs resolved to the PostScript names DesignTokens
// expects. Remove the call from QuranMealiApp once verified.
#if DEBUG
enum FontDebug {
    static func dumpAvailableFonts() {
        for family in UIFont.familyNames.sorted() {
            let names = UIFont.fontNames(forFamilyName: family).joined(separator: ", ")
            print("[Font] \(family): \(names)")
        }
    }
}
#endif
