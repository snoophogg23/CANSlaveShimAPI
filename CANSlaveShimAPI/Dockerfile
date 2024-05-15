
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk add clang binutils musl-dev build-base zlib-static
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CANSlaveShimAPI/CANSlaveShimAPI.csproj", "CANSlaveShimAPI/"]
RUN dotnet restore --runtime linux-musl-x64 "./CANSlaveShimAPI/CANSlaveShimAPI.csproj"
COPY . .
WORKDIR "/src/CANSlaveShimAPI"
RUN dotnet build --runtime linux-musl-x64 "./CANSlaveShimAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CANSlaveShimAPI.csproj" -c $BUILD_CONFIGURATION -r linux-musl-x64 --no-restore -o /app/publish

FROM scratch AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["./CANSlaveShimAPI"]