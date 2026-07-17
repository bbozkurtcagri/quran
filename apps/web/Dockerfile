# Multi-stage build: node ile bundle üret, nginx ile serve.
# nginx image sonunda ~25 MB, node image ise sadece build sırasında yaşar (kalıcı değil).

# Stage 1: build
FROM node:22-alpine AS builder
WORKDIR /app

# Build-time env — CI --build-arg ile override edecek.
# VITE_ prefix'i şart: Vite yalnızca VITE_*'ı client bundle'a gömer (guvenlik).
ARG VITE_API_BASE_URL=https://api.kuranmeali.app
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

# Deps katmanı önce: package*.json değişmediyse npm ci'ı cache'den geç.
# Layer cache stratejisi — build hızını 10x artırır.
COPY package.json package-lock.json ./
RUN npm ci

# Source
COPY . .

# Build: tsc type-check + vite bundle → /app/dist
RUN npm run build

# Stage 2: nginx serve
FROM nginx:1.27-alpine

# SPA routing + cache headers + gzip için custom config.
# Default /etc/nginx/conf.d/default.conf'un yerine geçer.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Static bundle build stage'inden kopyala.
COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80

# nginx foreground'da çalışsın (container process'i olarak); daemon mode = process biter, container ölür.
CMD ["nginx", "-g", "daemon off;"]
