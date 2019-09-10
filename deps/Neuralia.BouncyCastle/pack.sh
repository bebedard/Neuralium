#!/bin/bash

dotnet restore

if dotnet build Neuralia.BouncyCastle.sln -c Release --no-incremental ; then
    if  dotnet pack Neuralia.BouncyCastle.sln -c Release -o ./ ; then
	dotnet clean ;
         echo "pack completed"
    else
        echo "build failed"
    fi
else
    echo "build failed"
fi


   
