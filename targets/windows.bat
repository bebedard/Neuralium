
cd ../

dotnet restore


dotnet publish Neuralium/src/Neuralium.csproj -c Release -o ../../build -r win-x64
dotnet clean
echo "publish completed"




   
