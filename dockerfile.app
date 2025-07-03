FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /root

COPY ["app/app.csproj", "/root/"]
RUN dotnet restore "/root/app.csproj"


COPY /app /root/app
WORKDIR /root/app
RUN dotnet build "/root/app/app.csproj" -c Release -o /out

FROM build AS publish
RUN dotnet publish -c Release -o /publish
FROM mcr.microsoft.com/dotnet/aspnet:9.0

COPY --from=publish /publish .

ENTRYPOINT ["dotnet", "app.dll"]