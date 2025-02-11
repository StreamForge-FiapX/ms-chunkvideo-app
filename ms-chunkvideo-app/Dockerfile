# Instalar FFmpeg
FROM ubuntu:20.04 AS ffmpeg-install
RUN apt-get update && apt-get install -y \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Copiar os binários do FFmpeg para o contêiner final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=ffmpeg-install /usr/bin/ffmpeg /usr/bin/ffmpeg
COPY --from=ffmpeg-install /usr/lib/x86_64-linux-gnu/libavcodec.so.58 /usr/lib/x86_64-linux-gnu/libavcodec.so.58
COPY --from=ffmpeg-install /usr/lib/x86_64-linux-gnu/libavformat.so.58 /usr/lib/x86_64-linux-gnu/libavformat.so.58
COPY --from=ffmpeg-install /usr/lib/x86_64-linux-gnu/libavutil.so.56 /usr/lib/x86_64-linux-gnu/libavutil.so.56
COPY --from=ffmpeg-install /usr/lib/x86_64-linux-gnu/libswresample.so.3 /usr/lib/x86_64-linux-gnu/libswresample.so.3


# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

COPY . .
WORKDIR "/WebApi/."
RUN dotnet restore "WebApi.csproj"
RUN dotnet build "WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]