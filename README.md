# Neuralium

The Neuralium crypto token console server node

### Blockchains.Neuralium
The Neuralium blockchain implementation
### Neuralium.Api.Common
API public Interfaces
### Neuralium.Shell
The Neuralium server node components
### Neuralium
The actual command line interface for the Server node.

## Build Instructions

##### First, ensure dotnet core 2.2 SDK is installed
Run the following bash script in the base directory
>./pack-dependencies.sh

Then, simply invoke the right build file for your needs

>cd targets
> ./linux.sh

or if you want to build for arm

>./arm.sh

this will produce the executable in the folder /build
