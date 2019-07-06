dotnet build ../HttpExceptions.sln --configuration Release
dotnet test ../HttpExceptions.sln --configuration Release --framework netcoreapp2.2 --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Include="[Opw.*]*" /p:Exclude="[*.Tests]*%2c[*.*Tests]*"
PAUSE