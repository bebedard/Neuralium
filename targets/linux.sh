#!/bin/bash


dotnet restore


if  dotnet publish ../Neuralium/src/Neuralium.csproj -c Release -o ../../build -r linux-x64 ; then
dotnet clean ;
 echo "publish completed"
else
echo "publish failed"
fi



   
