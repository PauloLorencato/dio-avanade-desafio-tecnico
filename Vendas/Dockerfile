# Multi-stage Dockerfile para o projeto C# em ./Estoque
# Usa SDK para build/publish e a imagem ASP.NET Runtime para execução

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Vendas.sln", "./"]
COPY ["Vendas/Vendas.csproj", "Vendas/"]

RUN dotnet restore "Vendas.sln"

# Copia todo o restante do código e publica em Release
COPY . ./
WORKDIR /src/Vendas
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Expor porta padrão e configurar variável de ambiente para ASP.NET
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

# Ajuste o nome do DLL se o assembly final tiver outro nome
ENTRYPOINT ["dotnet", "Vendas.dll"]
