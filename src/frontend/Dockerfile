## ─── Stage 1: build ──────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Instalar dependências primeiro (melhor cache de camadas)
COPY package.json package-lock.json* ./
RUN npm install

# Copiar código e buildar
COPY . .
RUN npm run build

## ─── Stage 2: serve com nginx ────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Remover configuração padrão do nginx
RUN rm /etc/nginx/conf.d/default.conf

# Copiar config customizada e arquivos buildados
COPY nginx.conf /etc/nginx/conf.d/tickethub.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
