#!/usr/bin/env swift
//
// generate-icon.swift
// 1024×1024 AppIcon — kapalı mushaf silueti (yan görünüm).
//
//   swift apps/ios/scripts/generate-icon.swift <light|dark|tinted> <out.png>
//

import Foundation
import CoreGraphics
import ImageIO
import AppKit

// Renk paleti — her variant için arkaplan, kapak, spine, sayfa-kenarı ve ornament renkleri.
struct Palette {
    let bg: CGColor       // canvas
    let cover: CGColor    // cilt / kapak rengi
    let spine: CGColor    // spine (kapaktan biraz koyu)
    let pageEdge: CGColor // sayfa kenarındaki ince altın hat
    let ornament: CGColor // ön kapaktaki merkez motif
}

func palette(for variant: String) -> Palette {
    switch variant {
    case "light":
        return Palette(
            bg:       CGColor(srgbRed: 0xFB/255, green: 0xFB/255, blue: 0xFA/255, alpha: 1),
            cover:    CGColor(srgbRed: 0x23/255, green: 0x41/255, blue: 0x28/255, alpha: 1),
            spine:    CGColor(srgbRed: 0x17/255, green: 0x2C/255, blue: 0x1A/255, alpha: 1),
            pageEdge: CGColor(srgbRed: 0xC9/255, green: 0xA8/255, blue: 0x75/255, alpha: 1),
            ornament: CGColor(srgbRed: 0xC9/255, green: 0xA8/255, blue: 0x75/255, alpha: 1)
        )
    case "dark":
        return Palette(
            bg:       CGColor(srgbRed: 0x1F/255, green: 0x18/255, blue: 0x12/255, alpha: 1),
            cover:    CGColor(srgbRed: 0x2D/255, green: 0x5A/255, blue: 0x32/255, alpha: 1),
            spine:    CGColor(srgbRed: 0x1F/255, green: 0x42/255, blue: 0x22/255, alpha: 1),
            pageEdge: CGColor(srgbRed: 0xDF/255, green: 0xC0/255, blue: 0x89/255, alpha: 1),
            ornament: CGColor(srgbRed: 0xDF/255, green: 0xC0/255, blue: 0x89/255, alpha: 1)
        )
    case "tinted":
        // Tinted icon — iOS recolours; tek tonlu beyaz nüansları kullan.
        return Palette(
            bg:       CGColor(srgbRed: 0, green: 0, blue: 0, alpha: 1),
            cover:    CGColor(srgbRed: 1, green: 1, blue: 1, alpha: 0.95),
            spine:    CGColor(srgbRed: 1, green: 1, blue: 1, alpha: 0.7),
            pageEdge: CGColor(srgbRed: 1, green: 1, blue: 1, alpha: 0.55),
            ornament: CGColor(srgbRed: 1, green: 1, blue: 1, alpha: 0.85)
        )
    default:
        fatalError("Unknown variant: \(variant)")
    }
}

guard CommandLine.arguments.count >= 3 else {
    print("Usage: swift generate-icon.swift <light|dark|tinted> <out.png>")
    exit(1)
}

let variant = CommandLine.arguments[1]
let outPath = CommandLine.arguments[2]
let pal = palette(for: variant)

let size: CGFloat = 1024
let cs = CGColorSpaceCreateDeviceRGB()
guard let ctx = CGContext(
    data: nil,
    width: Int(size), height: Int(size),
    bitsPerComponent: 8, bytesPerRow: 0,
    space: cs,
    bitmapInfo: CGImageAlphaInfo.premultipliedLast.rawValue
) else { exit(1) }

// 1. Arkaplan — solid fill (App Store icon kuralı, transparency yok).
ctx.setFillColor(pal.bg)
ctx.fill(CGRect(x: 0, y: 0, width: size, height: size))

// 2. Mushaf gövdesi — yandan görünüm, dikdörtgen kapak.
//    600×740 boyut, square canvas içinde ortalı; dikey kitap oranı (yaklaşık 5:6).
let coverW: CGFloat = 600
let coverH: CGFloat = 740
let coverX = (size - coverW) / 2
let coverY = (size - coverH) / 2

let coverRect = CGRect(x: coverX, y: coverY, width: coverW, height: coverH)
let coverPath = CGPath(roundedRect: coverRect, cornerWidth: 18, cornerHeight: 18, transform: nil)
ctx.addPath(coverPath)
ctx.setFillColor(pal.cover)
ctx.fillPath()

// 3. Spine — sol kenarda dikey şerit; kapaktan biraz daha koyu, deri ciltin sırtı.
//    Cover'ın rounded corner'ları içinde kalsın diye clipping uygulanıyor.
ctx.saveGState()
ctx.addPath(coverPath)
ctx.clip()

let spineW: CGFloat = 50
ctx.setFillColor(pal.spine)
ctx.fill(CGRect(x: coverX, y: coverY, width: spineW, height: coverH))

// Spine'da ince yatay altın çizgiler — geleneksel cilt detayı (üst + alt headband).
ctx.setFillColor(pal.pageEdge)
ctx.fill(CGRect(x: coverX, y: coverY + coverH - 40, width: spineW, height: 4))
ctx.fill(CGRect(x: coverX, y: coverY + 36,            width: spineW, height: 4))

ctx.restoreGState()

// 4. Ön kapakta altın çerçeve — kapaktan içeri inset edilmiş ince dikdörtgen.
let frameInset: CGFloat = 56
let frameRect = CGRect(
    x: coverX + spineW + 30,
    y: coverY + frameInset,
    width: coverW - spineW - 30 - frameInset,
    height: coverH - 2 * frameInset
)
ctx.setStrokeColor(pal.pageEdge)
ctx.setLineWidth(3)
ctx.stroke(frameRect)

// 5. Merkez ornament — şemse (dört yapraklı rozet). Dört dairenin örtüşmesiyle çizilir,
//    iç bölgede ince altın halka kalır.
//    Motifin kültürel arka planı için: QuranMeali/Views/SplashView.swift → `Rosette`.
let cx = frameRect.midX
let cy = frameRect.midY
let petalR: CGFloat = 70
let offset: CGFloat = 50

ctx.setFillColor(pal.ornament)
for (dx, dy) in [(0.0, offset), (0.0, -offset), (offset, 0.0), (-offset, 0.0)] {
    ctx.beginPath()
    ctx.addArc(center: CGPoint(x: cx + dx, y: cy + dy),
               radius: petalR, startAngle: 0, endAngle: .pi * 2,
               clockwise: false)
    ctx.fillPath()
}
// Ortadaki küçük disk — rozetin göbeği (kapak rengiyle delinmiş "negative" merkez).
ctx.setFillColor(pal.cover)
ctx.beginPath()
ctx.addArc(center: CGPoint(x: cx, y: cy), radius: 32, startAngle: 0, endAngle: .pi * 2, clockwise: false)
ctx.fillPath()
ctx.setFillColor(pal.ornament)
ctx.beginPath()
ctx.addArc(center: CGPoint(x: cx, y: cy), radius: 10, startAngle: 0, endAngle: .pi * 2, clockwise: false)
ctx.fillPath()

// 6. PNG export.
guard let cgImage = ctx.makeImage() else { exit(1) }
let url = URL(fileURLWithPath: outPath)
guard let dest = CGImageDestinationCreateWithURL(url as CFURL, "public.png" as CFString, 1, nil) else { exit(1) }
CGImageDestinationAddImage(dest, cgImage, nil)
CGImageDestinationFinalize(dest)
print("[icon] \(variant) → \(outPath)")
