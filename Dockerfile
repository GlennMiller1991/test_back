FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR usr/app/back
ARG port

COPY . .
RUN dotnet publish -c release -o ./build

EXPOSE $port
ENTRYPOINT ["dotnet", "./build/WebApplication1.dll"]