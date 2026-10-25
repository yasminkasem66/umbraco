# Build stage — compiles and publishes the app.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project/package files first so this restore layer is cached between builds that
# don't touch dependencies.
COPY Directory.Packages.props UmbracoDemo.csproj ./
RUN dotnet restore UmbracoDemo.csproj

COPY . .
RUN dotnet publish UmbracoDemo.csproj -c Release -o /app/publish --no-restore

# Runtime stage — the image that actually ships and runs.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

# Shell form so $PORT is resolved when the container actually starts, not baked in at build time
# — Render injects its own PORT value at runtime, and a Docker ENV substitution would only ever
# see the value present at `docker build` time. Defaults to Render's own default (10000) if PORT
# isn't set, so this also runs correctly on a host that doesn't inject PORT at all.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} exec dotnet UmbracoDemo.dll"]
