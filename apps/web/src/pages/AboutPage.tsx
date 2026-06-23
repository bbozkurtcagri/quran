import { Eyebrow } from "../components/Eyebrow";
import { SectionRule } from "../components/SectionRule";

export function AboutPage() {
  return (
    <article className="mx-auto max-w-3xl px-6">
      <header className="pt-20 pb-16 md:pt-28 md:pb-20 text-center">
        <Eyebrow className="mb-6">Hakkında</Eyebrow>
        <h1 className="font-serif text-6xl md:text-7xl leading-[1.05] tracking-tight optical-display">
          Kur'an Meali
        </h1>
        <div className="mt-10">
          <SectionRule />
        </div>
      </header>

      <div className="pb-32 font-reading text-[17px] leading-[1.8] text-text">
        <section className="mb-14">
          <Eyebrow className="mb-5">Proje</Eyebrow>
          <p>
            Kur'an-ı Kerim'i Arapça aslıyla okumak ve Türkçe mealinde
            kelime ya da kavram aramak için sade bir okuma uygulaması.
            Reklamsız, ücretsiz, açık kaynaklara dayalı.
          </p>
        </section>

        <section className="mb-14">
          <Eyebrow className="mb-5">Kaynaklar</Eyebrow>
          <dl className="space-y-6">
            <div>
              <dt className="font-serif text-xl mb-1">Türkçe meal</dt>
              <dd className="text-text-muted">
                Elmalılı Muhammed Hamdi Yazır,{" "}
                <em className="not-italic font-medium text-text">Hak Dini Kur'an Dili</em>{" "}
                (1935-1938). Müellif 27 Mayıs 1942'de vefat etmiştir; eser,{" "}
                <span className="text-text">5846 sayılı Fikir ve Sanat Eserleri Kanunu</span>{" "}
                Madde 27 uyarınca <strong className="text-text font-medium">1 Ocak 2013'ten itibaren kamu malı</strong>{" "}
                statüsündedir. Metin verisi Quran.com üzerinden alınmış,
                meal kısmı dışına müdahale edilmemiştir.
              </dd>
            </div>

            <div>
              <dt className="font-serif text-xl mb-1">Arapça mushaf</dt>
              <dd className="text-text-muted">
                Tanzil Project,{" "}
                <em className="not-italic font-medium text-text">Uthmani Minimal</em>{" "}
                sürümü (v1.1, Şubat 2021). Metin lisansı verbatim
                dağıtıma izin verir; değiştirilmemiştir.{" "}
                <a
                  href="https://tanzil.net"
                  target="_blank"
                  rel="noreferrer"
                  className="hover:underline underline-offset-[3px]"
                >
                  tanzil.net
                </a>
              </dd>
            </div>

            <div>
              <dt className="font-serif text-xl mb-1">Anlamsal arama</dt>
              <dd className="text-text-muted">
                Sorgu ve ayet metinleri,{" "}
                <em className="not-italic font-medium text-text">intfloat/multilingual-e5-small</em>{" "}
                embedding modeliyle vektörleştirilir. Tüm çıkarımlar yerel
                ortamda yapılır; hiçbir kullanıcı verisi üçüncü taraf
                servislere gönderilmez.
              </dd>
            </div>
          </dl>
        </section>

        <section className="mb-14">
          <Eyebrow className="mb-5">Hata bildirimi</Eyebrow>
          <p className="text-text-muted">
            Mealde ya da arayüzde bir hata fark ederseniz lütfen bildirin —{" "}
            <a
              href="mailto:bbozkurtcagri@gmail.com"
              className="text-text hover:underline underline-offset-[3px]"
            >
              bbozkurtcagri@gmail.com
            </a>
            .
          </p>
        </section>

        <section>
          <Eyebrow className="mb-5">Ücretsizdir</Eyebrow>
          <p className="text-text-muted">
            Kur'an metni ve meali üzerinden gelir elde edilmez. Hiçbir
            reklam, takip kodu ya da abonelik yoktur.
          </p>
        </section>
      </div>
    </article>
  );
}
