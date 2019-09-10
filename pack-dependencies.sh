#!/bin/bash

mkdir deps/neuralia.blockchains.tools/local-source
mkdir local-source
mkdir deps/Neuralia.Blockchain/local-source
mkdir deps/Neuralia.BouncyCastle/local-source

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

cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../neuralia.blockchains.tools/local-source/Neuralia.System.Data.HashFunction.xxHash.1.0.0.nupkg
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../../local-source/Neuralia.System.Data.HashFunction.xxHash.1.0.0.nupkg
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../Neuralia.Blockchain/local-source/Neuralia.System.Data.HashFunction.xxHash.1.0.0.nupkg
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../Neuralia.BouncyCastle/local-source/Neuralia.System.Data.HashFunction.xxHash.1.0.0.nupkg

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

cp Neuralia.Blockchains.Tools.1.0.0.nupkg ../Neuralia.Blockchain/local-source/
cp Neuralia.Blockchains.Tools.1.0.0.nupkg ../euralia.BouncyCastle/local-source/
cp Neuralia.Blockchains.Tools.1.0.0.nupkg ../../local-source/

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

cp Neuralia.STUN.1.0.0.nupkg ../Neuralia.Blockchain/local-source/
cp Neuralia.STUN.1.0.0.nupkg ../../local-source/


cd ../neuralia.BouncyCastle/local-source/

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

cp Neuralia.BouncyCastle.1.0.0.nupkg ../Neuralia.Blockchain/local-source/
cp Neuralia.BouncyCastle.1.0.0.nupkg ../../local-source/

cd ../neuralia.Blockchain/local-source/


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

cp Neuralia.Blockchains.Common.1.0.0.nupkg ../../local-source/
cp Neuralia.Blockchains.Core.1.0.0.nupkg ../../local-source/
