FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy all of registration-api (so src/Registration.Api and src/Registration.Application are both included)
COPY . .

WORKDIR /src/src/Registration.Api
RUN dotnet publish Registration.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "Registration.Api.dll"]