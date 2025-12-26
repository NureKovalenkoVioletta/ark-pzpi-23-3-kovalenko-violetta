FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5006
EXPOSE 5007

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FitnessProject/FitnessProject.csproj", "FitnessProject/"]
RUN dotnet restore "FitnessProject/FitnessProject.csproj"
COPY . .
WORKDIR "/src/FitnessProject"
RUN dotnet build "FitnessProject.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/FitnessProject"
RUN dotnet publish "FitnessProject.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FitnessProject.dll"]

