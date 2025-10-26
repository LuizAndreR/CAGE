# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copia apenas os arquivos de projeto da API e suas dependências
COPY backend/src/CakeGestao.API/CakeGestao.API.csproj ./backend/src/CakeGestao.API/
COPY backend/src/CakeGestao.Application/CakeGestao.Application.csproj ./backend/src/CakeGestao.Application/
COPY backend/src/CakeGestao.Domain/CakeGestao.Domain.csproj ./backend/src/CakeGestao.Domain/
COPY backend/src/CakeGestao.Infrastructure/CakeGestao.Infrastructure.csproj ./backend/src/CakeGestao.Infrastructure/

# 2. Restaura as dependências *apenas* do projeto da API
RUN dotnet restore "backend/src/CakeGestao.API/CakeGestao.API.csproj"

# 3. Copia todo o código-fonte restante da pasta 'src'
COPY backend/src/ ./backend/src/

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# 4. Publica o projeto da API
RUN dotnet publish "backend/src/CakeGestao.API/CakeGestao.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CakeGestao.API.dll"]