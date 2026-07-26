FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY ["EventHub.API/EventHub.API.csproj", "EventHub.API/"]
COPY ["EventHub.Application/EventHub.Application.csproj", "EventHub.Application/"]
COPY ["EventHub.Domain/EventHub.Domain.csproj", "EventHub.Domain/"]
COPY ["EventHub.Infrastructure/EventHub.Infrastructure.csproj", "EventHub.Infrastructure/"]
RUN dotnet restore "EventHub.API/EventHub.API.csproj"


COPY . .
RUN dotnet build "EventHub.API/EventHub.API.csproj" -c Release -o /app/build


RUN dotnet publish "EventHub.API/EventHub.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventHub.API.dll"]