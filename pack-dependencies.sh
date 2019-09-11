#!/bin/bash

cd deps/Neuralia.Data.HashFunction.xxHash

dotnet restore

if dotnet build -c Release --no-incremental ; then
    if  dotnet pack -c Release -o ../ ; then
	dotnet clean ;
         echo "pack completed"
    else
        echo "build failed"
    fi
else
    echo "build failed"
fi

cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../neuralia.blockchains.tools/nuget-source/
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../../nuget-source/
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../Neuralia.BouncyCastle/nuget-source/

cd ../neuralia.blockchains.tools

dotnet restore

if dotnet build -c Release --no-incremental ; then
    if  dotnet pack -c Release -o ../ ; then
	dotnet clean ;
         echo "pack completed"
    else
        echo "build failed"
    fi
else
    echo "build failed"
fi

cp Neuralia.Blockchains.Tools.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.Blockchains.Tools.1.0.0.nupkg ../euralia.BouncyCastle/nuget-source/
cp Neuralia.Blockchains.Tools.1.0.0.nupkg ../../nuget-source/

cd ../Neuralia.STUN

dotnet restore

if dotnet build -c Release --no-incremental ; then
    if  dotnet pack -c Release -o ../ ; then
	dotnet clean ;
         echo "pack completed"
    else
        echo "build failed"
    fi
else
    echo "build failed"
fi

cp Neuralia.STUN.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.STUN.1.0.0.nupkg ../../nuget-source/


cd ../neuralia.BouncyCastle/nuget-source/

dotnet restore

if dotnet build Neuralia.BouncyCastle.sln -c Release --no-incremental ; then
    if  dotnet pack Neuralia.BouncyCastle.sln -c Release -o ../ ; then
	dotnet clean ;
         echo "pack completed"
    else
        echo "build failed"
    fi
else
    echo "build failed"
fi

cp Neuralia.BouncyCastle.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.BouncyCastle.1.0.0.nupkg ../../nuget-source/

cd ../neuralia.Blockchain/nuget-source/


dotnet restore

if dotnet build -c Release --no-incremental ; then
    if  dotnet pack -c Release -o ../../ ; then
	dotnet clean ;
         echo "pack completed"
    else
        echo "build failed"
    fi
else
    echo "build failed"
fi

cp Neuralia.Blockchains.Common.1.0.0.nupkg ../../nuget-source/
cp Neuralia.Blockchains.Core.1.0.0.nupkg ../../nuget-source/
