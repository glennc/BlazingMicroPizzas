FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["BlazingPizza.Web/BlazingPizza.Web.csproj", "BlazingPizza.Web/"]
COPY ["BlazingPizza.ComponentsLibrary/BlazingPizza.ComponentsLibrary.csproj", "BlazingPizza.ComponentsLibrary/"]
COPY ["BlazingPizza.Shared/BlazingPizza.Shared.csproj", "BlazingPizza.Shared/"]
COPY ["BlazingComponents/BlazingComponents.csproj", "BlazingComponents/"]
RUN dotnet restore "BlazingPizza.Web/BlazingPizza.Web.csproj"
COPY . .
WORKDIR "/src/BlazingPizza.Web"
RUN dotnet build "BlazingPizza.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazingPizza.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazingPizza.Web.dll"]