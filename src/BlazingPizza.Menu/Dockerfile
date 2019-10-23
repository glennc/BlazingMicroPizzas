FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /src
COPY ["BlazingPizza.Menu/BlazingPizza.Menu.csproj", "BlazingPizza.Menu/"]
COPY ["BlazingPizza.Shared/BlazingPizza.Shared.csproj", "BlazingPizza.Shared/"]
COPY ["BlazingPizza.ComponentsLibrary/BlazingPizza.ComponentsLibrary.csproj", "BlazingPizza.ComponentsLibrary/"]
RUN dotnet restore "BlazingPizza.Menu/BlazingPizza.Menu.csproj"
COPY . .
WORKDIR "/src/BlazingPizza.Menu"
RUN dotnet build "BlazingPizza.Menu.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazingPizza.Menu.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazingPizza.Menu.dll"]