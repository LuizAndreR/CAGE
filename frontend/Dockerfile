# ==========================================
# Estágio 1: Build (Construção da aplicação)
# ==========================================
FROM node:22-alpine AS build

WORKDIR /app

# Copia os arquivos de dependência primeiro para aproveitar o cache de camadas do Docker
COPY package*.json ./

# Instala as dependências
RUN npm install --legacy-peer-deps

# Copia todo o código do frontend para dentro do contêiner
COPY . .

# Executa o build de produção do Angular
RUN npm run build --configuration=production

# ==========================================
# Estágio 2: Servidor Web (Nginx)
# ==========================================
FROM nginx:alpine

# Limpa a pasta padrão do Nginx
RUN rm -rf /usr/share/nginx/html/*

# ---> ADIÇÃO SÊNIOR: Copia a configuração customizada de rotas do Nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copia os arquivos estáticos gerados no build para o Nginx.
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]