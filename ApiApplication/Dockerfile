# Stage 1: Build (using .NET 9 SDK for arm64)
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine-arm64v8 AS build

WORKDIR /app

# Install required dependencies
RUN apk update && apk add --no-cache \
    build-base \
    curl \
    unzip \
    autoconf \
    automake \
    libtool \
    m4 \
    gcc \
    g++ \
    make \
    linux-headers \
    protobuf \
    protobuf-dev \
    grpc \
    grpc-dev

# Set environment variables for protobuf and gRPC plugin
ENV PROTOBUF_PROTOC=/usr/bin/protoc
ENV gRPC_PluginFullPath=/usr/bin/grpc_csharp_plugin

# Copy the generated certificate from the host into the container and movies db
COPY ./Certs/aspnetapp.pfx /app/aspnetapp.pfx
COPY ./Data/movies-db.json /app/movies-db.json

# Copy necessary project folders from parent directory
COPY ./ApiApplication /app/ApiApplication
COPY ./ApiApplication.Core /app/ApiApplication.Core
COPY ./ApiApplication.Domain /app/ApiApplication.Domain
COPY ./ApiApplication.Infrastructure /app/ApiApplication.Infrastructure

# Change to the correct working directory
WORKDIR /app/ApiApplication

# Restore dependencies
RUN dotnet restore ApiApplication.csproj

# Build the application
RUN dotnet build ApiApplication.csproj -c Release -o /tmp/build

# Publish the application
RUN dotnet publish ApiApplication.csproj -c Release -o /tmp/publish

# Stage 2: Final Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-arm64v8 AS final

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=build /tmp/publish .

# Set entry point
ENTRYPOINT ["dotnet", "ApiApplication.dll"]
