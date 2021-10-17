FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY bin/Release/net5.0/publish/ App/
COPY Data/ML/Model/ Model/
WORKDIR /App
ENTRYPOINT ["dotnet", "camapi.predict.dll"]