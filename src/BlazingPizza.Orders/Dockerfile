FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
#gRPC requires extra compat stuff to run on Alpine
RUN apk --no-cache add ca-certificates wget
RUN wget -q -O /etc/apk/keys/sgerrand.rsa.pub https://alpine-pkgs.sgerrand.com/sgerrand.rsa.pub
RUN wget https://github.com/sgerrand/alpine-pkg-glibc/releases/download/2.30-r0/glibc-2.30-r0.apk
RUN apk add glibc-2.30-r0.apk
WORKDIR /src
COPY ["BlazingPizza.Orders/BlazingPizza.Orders.csproj", "BlazingPizza.Orders/"]
COPY ["BlazingPizza.Shared/BlazingPizza.Shared.csproj", "BlazingPizza.Shared/"]
COPY ["BlazingPizza.ComponentsLibrary/BlazingPizza.ComponentsLibrary.csproj", "BlazingPizza.ComponentsLibrary/"]
RUN dotnet restore "BlazingPizza.Orders/BlazingPizza.Orders.csproj"
COPY . .
WORKDIR "/src/BlazingPizza.Orders"
RUN dotnet build "BlazingPizza.Orders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazingPizza.Orders.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazingPizza.Orders.dll"]