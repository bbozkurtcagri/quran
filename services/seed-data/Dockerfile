# syntax=docker/dockerfile:1.7

# Data-only image. Amaç: seed data'yı taşıyan taşınabilir bir "artifact".
# Kullanım: K8s Job'ında init container olarak çalışır, /data'yı shared emptyDir'e
# kopyalar, ardından main container (API) o emptyDir'i mount edip "seed" komutunu koşar.
#
# Neden ayrı image?
#   - API runtime image'ı temiz kalır (sadece kod)
#   - Data ayrı version'lanır (tag'lenir) — meal güncellenirse sadece bu image rebuild
#   - Init container pattern: CNCF projelerinin standart data-provision yaklaşımı
#     (örn. Prometheus rules image, Grafana dashboards image).

# busybox: 5 MB, sh + cp + standart POSIX araçları içerir.
# Distroless de tercih edilebilir ama cp için sh gerekiyor init container komutunda.
FROM busybox:1.37

# Non-root — kubelet güvenlik best-practice
RUN adduser -D -u 1000 seed
USER seed

WORKDIR /data

# Seed data'yı image'a al. Build context repo root'undan.
COPY --chown=seed:seed seed-data/ /data/

# Default komut — direkt çalıştırılırsa data listesini göster (debug için).
# Init container mode'da bu ENTRYPOINT override edilecek: `cp -r /data/. /shared/`
CMD ["ls", "-lh", "/data"]
