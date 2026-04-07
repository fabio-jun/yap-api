FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY src/Blog.Domain/Blog.Domain.csproj src/Blog.Domain/
COPY src/Blog.Application/Blog.Application.csproj src/Blog.Application/
COPY src/Blog.Infrastructure/Blog.Infrastructure.csproj src/Blog.Infrastructure/
COPY src/Blog.API/Blog.API.csproj src/Blog.API/
RUN dotnet restore src/Blog.API/Blog.API.csproj

# Copy everything and publish
COPY src/ src/
RUN dotnet publish src/Blog.API/Blog.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENTRYPOINT ["dotnet", "Blog.API.dll"]
