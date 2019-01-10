#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1803 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM microsoft/dotnet:2.1-sdk-nanoserver-1803 AS build
WORKDIR /src
COPY ["WMS/Scheduler/WebAPI/Ferretto.WMS.Scheduler.WebAPI.csproj", "WMS/Scheduler/WebAPI/"]
COPY ["WMS/Scheduler/Core/Ferretto.WMS.Scheduler.Core.csproj", "WMS/Scheduler/Core/"]
COPY ["Common/EF/Ferretto.Common.EF.csproj", "Common/EF/"]
COPY ["Common/Resources/Ferretto.Common.Resources.csproj", "Common/Resources/"]
COPY ["Common/Utils/Ferretto.Common.Utils.csproj", "Common/Utils/"]
COPY ["Common/DataModels/Ferretto.Common.DataModels.csproj", "Common/DataModels/"]
COPY ["WMS/Scheduler/Drivers.Vertimag/Ferretto.WMS.Scheduler.Drivers.Vertimag.csproj", "WMS/Scheduler/Drivers.Vertimag/"]
COPY ["Common/BusinessModels/Ferretto.Common.BusinessModels.csproj", "Common/BusinessModels/"]
COPY ["Common/BLL.Interfaces/Ferretto.Common.BLL.Interfaces.csproj", "Common/BLL.Interfaces/"]
COPY ["WMS/Scheduler/WebAPI.Contracts/Ferretto.WMS.Scheduler.WebAPI.Contracts.csproj", "WMS/Scheduler/WebAPI.Contracts/"]
COPY ["dev-certificate.pfx"] ["/app/dev-certificate.pfx"]
RUN dotnet restore "WMS/Scheduler/WebAPI/Ferretto.WMS.Scheduler.WebAPI.csproj"
COPY . .
WORKDIR "/src/WMS/Scheduler/WebAPI"
RUN dotnet build "Ferretto.WMS.Scheduler.WebAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Ferretto.WMS.Scheduler.WebAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ferretto.WMS.Scheduler.WebAPI.dll"]
