#!/bin/bash

cd deps/Neuralia.Data.HashFunction.xxHash

if test -f "Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg"; then
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
fi

cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../neuralia.blockchains.tools/nuget-source/
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../../nuget-source/
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.Data.HashFunction.xxHash.1.0.0.nupkg ../Neuralia.BouncyCastle/nuget-source/

cd ../neuralia.blockchains.tools

if test -f "neuralia.Blockchains.Tools.1.0.0.nupkg"; then
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
fi

cp neuralia.Blockchains.Tools.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp neuralia.Blockchains.Tools.1.0.0.nupkg ../Neuralia.BouncyCastle/nuget-source/
cp neuralia.Blockchains.Tools.1.0.0.nupkg ../../nuget-source/

cd ../Neuralia.STUN

if test -f "Neuralia.STUN.1.0.0.nupkg"; then
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
fi

cp Neuralia.STUN.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.STUN.1.0.0.nupkg ../../nuget-source/


cd ../neuralia.BouncyCastle/nuget-source/

if test -f "Neuralia.BouncyCastle.1.0.0.nupkg"; then
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
fi

cp Neuralia.BouncyCastle.1.0.0.nupkg ../Neuralia.Blockchain/nuget-source/
cp Neuralia.BouncyCastle.1.0.0.nupkg ../../nuget-source/

cd ../neuralia.Blockchain/nuget-source/

if [test -f "Neuralia.Blockchains.Common.1.0.0.nupkg"] & [test -f "Neuralia.Blockchains.Core.1.0.0.nupkg"]; then
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
fi

cp Neuralia.Blockchains.Common.1.0.0.nupkg ../../nuget-source/
cp Neuralia.Blockchains.Core.1.0.0.nupkg ../../nuget-source/
