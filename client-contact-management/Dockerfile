FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["client-contact-management.csproj", "."]
RUN dotnet restore "./client-contact-management.csproj"
COPY . .
RUN dotnet build "./client-contact-management.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./client-contact-management.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "client-contact-management.dll"]