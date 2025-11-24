# Multi-stage Dockerfile para o projeto C# em ./Estoque
# Usa SDK para build/publish e a imagem ASP.NET Runtime para execução

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Estoque.sln", "./"]
COPY ["Estoque/Estoque.csproj", "Estoque/"]

# Restaura usando o arquivo de solução
RUN dotnet restore "Estoque.sln"

COPY . ./
WORKDIR /src/Estoque
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Expor porta padrão e configurar variável de ambiente para ASP.NET
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80

# Ajuste o nome do DLL se o assembly final tiver outro nome
ENTRYPOINT ["dotnet", "Estoque.dll"]
