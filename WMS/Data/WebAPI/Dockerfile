#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1803 AS base
WORKDIR /app
EXPOSE 6000 6001

FROM microsoft/dotnet:2.1-sdk-nanoserver-1803 AS build
WORKDIR /src
COPY ["WMS/Data/WebAPI/Ferretto.WMS.Data.WebAPI.csproj", "WMS/Data/WebAPI/"]
COPY ["Common/EF/Ferretto.Common.EF.csproj", "Common/EF/"]
COPY ["Common/Resources/Ferretto.Common.Resources.csproj", "Common/Resources/"]
COPY ["Common/Utils/Ferretto.Common.Utils.csproj", "Common/Utils/"]
COPY ["Common/DataModels/Ferretto.Common.DataModels.csproj", "Common/DataModels/"]
COPY ["dev-certificate.pfx", "/app/dev-certificate.pfx"]
RUN dotnet restore "WMS/Data/WebAPI/Ferretto.WMS.Data.WebAPI.csproj"
COPY . .
WORKDIR "/src/WMS/Data/WebAPI"
RUN dotnet build "Ferretto.WMS.Data.WebAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Ferretto.WMS.Data.WebAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ferretto.WMS.Data.WebAPI.dll"]
