# nrg-seminar
Repo for the NRG seminar on fluid volume generation

## Fluid generator
The fulide generator is written in c# and uses the fast noise library to generate the initial perlin noise. Source: https://github.com/Auburns/FastNoise_CSharp
Open the .sln project and run the code. The exports and visuals are saved in the direcotry of the binary file.

## VPT
The vpt is based on the visual pathe tracer from: https://github.com/terier/vpt
The modified version requires volumes to contain 4 bytes for each voxel. These are [volume, grad.x, grad.y, grad.z]
