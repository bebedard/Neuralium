# Neuralia.STUN

This library contains a modification on the STUN package to query stun servers and obtain a public IP address.

## Build Instructions

##### First, ensure dotnet core 2.2 SDK is installed

the best way to include it into other projects is to build it as a nuget package. 
To do so, simply invoke pack.sh
> ./pack.sh
this will produce a package nameNeuralia.STUN.*[version]*.nupkg

<b>NOTE from original creators:</b> Currently the [RFC3489] is implemented which is deprecated.

