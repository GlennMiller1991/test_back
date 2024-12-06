FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR usr/app/back

COPY . .
RUN dotnet publish -c release -o ./build

ENTRYPOINT ["dotnet", "./build/WebApplication1.dll"]