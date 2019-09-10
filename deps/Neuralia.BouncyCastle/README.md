# Neuralia.BouncyCastle

Neuralia modified bouncy castle implementation


## Build Instructions

##### First, ensure dotnet core 2.2 SDK is installed

#### The first step is to ensure that the dependencies have been built and copied into the nuget-source folder.

 - Neuralia.System.Data.HashFunction.xxHash
 - Neuralia.Blockchains.Tools

the best way to include it into other projects is to build it as a nuget package. 
To do so, simply invoke pack.sh
> ./pack.sh
this will produce a package name # Neuralia.BouncyCastle.*[version]*.nupkg
