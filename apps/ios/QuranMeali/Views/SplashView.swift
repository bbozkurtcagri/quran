//
//  SplashView.swift
//  QuranMeali
//
//  Animated brand splash gösterilirken arkasında statik launch screen
//  (warm dark) bulunur. Bu sayede uygulama açıldığı andan itibaren sıçrama
//  olmadan SwiftUI animasyonuna geçilir.
//

import SwiftUI
import UIKit

struct SplashView: View {
    // Element-bazlı animasyon durumları; her biri kendi delay/curve'üyle akıyor.
    @State private var rosetteOpacity: Double = 0
    @State private var rosetteScale: CGFloat = 0.6
    @State private var rosetteRotation: Double = -22
    @State private var ringOpacity: Double = 0
    @State private var ringRotation: Double = 0

    @State private var wordmarkOpacity: Double = 0
    @State private var wordmarkOffset: CGFloat = 18

    @State private var eyebrowOpacity: Double = 0
    @State private var ruleScale: CGFloat = 0.01   // soldan-sağa açılır

    var body: some View {
        ZStack {
            // Launch screen ile aynı renk — statikten animasyonlu geçişte sıçrama olmaz.
            Color.qcBackground
                .ignoresSafeArea()

            // Çok ince diagonal "noise" yerine premium hissi için aşağıya doğru hafif vignette.
            LinearGradient(
                colors: [Color.qcAccent.opacity(0.04), .clear, .clear],
                startPoint: .top,
                endPoint: .bottom
            )
            .ignoresSafeArea()

            VStack(spacing: 0) {
                Spacer()

                // Rosette + etrafında ince dönen halka (mushaf icon'undaki şemsenin büyük varyantı).
                ZStack {
                    OrbitalRing()
                        .stroke(Color.qcAccent.opacity(0.35), style: .init(lineWidth: 1, dash: [2, 6]))
                        .frame(width: 220, height: 220)
                        .rotationEffect(.degrees(ringRotation))
                        .opacity(ringOpacity)

                    Rosette()
                        .frame(width: 140, height: 140)
                        .scaleEffect(rosetteScale)
                        .opacity(rosetteOpacity)
                        .rotationEffect(.degrees(rosetteRotation))
                }
                .padding(.bottom, Spacing.xxxl)

                // Eyebrow — section rule ile birlikte soldan açılır.
                VStack(spacing: Spacing.lg) {
                    Rectangle()
                        .fill(Color.qcBorder)
                        .frame(width: 96, height: 1)
                        .scaleEffect(x: ruleScale, y: 1, anchor: .center)

                    Text("KUR'AN-I KERİM · TÜRKÇE MEAL")
                        .font(.qcMono(11))
                        .tracking(3.0)
                        .foregroundStyle(Color.qcTextMuted)
                        .opacity(eyebrowOpacity)
                }
                .padding(.bottom, Spacing.lg)

                // Wordmark — Instrument Serif, alt'tan kayıp fade in.
                Text("Kur'an Meali")
                    .font(.qcDisplay(56))
                    .tracking(-0.8)
                    .foregroundStyle(Color.qcText)
                    .opacity(wordmarkOpacity)
                    .offset(y: wordmarkOffset)

                Spacer()
                Spacer()
            }
        }
        .onAppear(perform: runIntro)
    }

    // MARK: - Animation choreography

    private func runIntro() {
        // Hafif haptic — ilk frame ile birlikte tetiklenir; premium uygulamalardaki "anchor" hissi.
        let haptic = UIImpactFeedbackGenerator(style: .soft)
        haptic.impactOccurred(intensity: 0.6)

        // 0.10s — rosette scale + rotation + fade in
        withAnimation(.softSkillSlow.delay(0.1)) {
            rosetteOpacity = 1
            rosetteScale = 1
            rosetteRotation = 0
        }

        // 0.30s — dış halka belirir + sürekli yavaş dönüş
        withAnimation(.softSkillSlow.delay(0.3)) {
            ringOpacity = 1
        }
        withAnimation(.linear(duration: 14).repeatForever(autoreverses: false).delay(0.3)) {
            ringRotation = 360
        }

        // 0.55s — eyebrow rule soldan-sağa açılır
        withAnimation(.softSkillSlow.delay(0.55)) {
            ruleScale = 1
        }

        // 0.70s — eyebrow yazısı fade in
        withAnimation(.softSkillSlow.delay(0.7)) {
            eyebrowOpacity = 1
        }

        // 0.90s — wordmark alttan kayıp belirir
        withAnimation(.softSkillSlow.delay(0.9)) {
            wordmarkOpacity = 1
            wordmarkOffset = 0
        }
    }
}

// MARK: - Rosette (şemse)

/// Şemse motifi.
///
/// "Güneş" anlamına gelen şemse, Osmanlı / İslâmî mushaf tezhibinde kullanılan
/// geleneksel bir süslemedir: dört yapraklı, stilize bir çiçek. El yazması
/// Kur'an'ların kapak ortasında ve sayfa serlevhalarında (başlangıç sayfası)
/// sıkça karşılaşılan klasik bir motif.
///
/// Burada path-based çiziliyor (asset yok); aynı geometri app icon'da da
/// kullanılır — bkz. `apps/ios/scripts/generate-icon.swift`.
private struct Rosette: View {
    var body: some View {
        Canvas { ctx, size in
            let cx = size.width / 2
            let cy = size.height / 2
            let petalR = size.width * 0.28
            let off    = size.width * 0.20

            // Dört yapraklı şemse — overlapping circles.
            for (dx, dy) in [(0.0, off), (0.0, -off), (off, 0.0), (-off, 0.0)] {
                let rect = CGRect(x: cx + dx - petalR, y: cy + dy - petalR,
                                  width: petalR * 2, height: petalR * 2)
                ctx.fill(Path(ellipseIn: rect), with: .color(Color.qcAccent))
            }

            // Negatif göbek (background rengiyle delik açılır gibi).
            let negR = size.width * 0.13
            ctx.fill(
                Path(ellipseIn: CGRect(x: cx - negR, y: cy - negR, width: negR * 2, height: negR * 2)),
                with: .color(Color.qcBackground)
            )

            // Merkez küçük disk — ornament'ın ağırlık noktası.
            let dotR = size.width * 0.04
            ctx.fill(
                Path(ellipseIn: CGRect(x: cx - dotR, y: cy - dotR, width: dotR * 2, height: dotR * 2)),
                with: .color(Color.qcAccent)
            )
        }
    }
}

// MARK: - Orbital dashed ring

private struct OrbitalRing: Shape {
    func path(in rect: CGRect) -> Path {
        Path(ellipseIn: rect.insetBy(dx: 1, dy: 1))
    }
}
