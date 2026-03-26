FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY Api.Back.csproj .
RUN dotnet restore

COPY . .

# On expose le port standard 8080
EXPOSE 8080

# CORRECTION : On écoute sur le port 8080 à l'intérieur du conteneur
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]