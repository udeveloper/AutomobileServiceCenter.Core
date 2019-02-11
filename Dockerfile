FROM microsoft/dotnet:2.1.503-sdk AS build-env

COPY . /app
WORKDIR /app
RUN dotnet restore

WORKDIR /app/ASC.Tests
RUN dotnet test

WORKDIR /app/ASC.WebMVC
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/ASC.WebMVC/out .
ENTRYPOINT ["dotnet", "ASC.WebMVC.dll"]

#FROM microsoft/dotnet:2.1.503-sdk
#
#COPY . /app
#WORKDIR /app
#RUN dotnet restore -s https://dotnet.myget.org/F/aspnetcore-dev/api/v3/index.json -s https://api.nuget.org/v3/index.json
#
#WORKDIR /app/ASC.Tests
#RUN dotnet test
#
#WORKDIR /app
#RUN dotnet build
#
#EXPOSE 5000/tcp
#ENV ASPNETCORE_URLS http://*:5000
#ENV ASPNETCORE_ENVIRONMENT Production
#
#WORKDIR /app/ASC.WebMVC
#ENTRYPOINT dotnet run