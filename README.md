# OFC
OFC is a tool for compressing OpenFOAM® files. It can compress files lossless or by rounding values to a specified amount of digits. It does compression by grouping numbers which have a similarity. They might have the same exponents, can all be expressed by a linear function, switch between other groups (ping-pong) or need the same amount of bits to write.

When an input file is compressed it is split up into two different files: 

 - _*.meta_ - contains all the metadata which is not getting compressed by the algorithm
 - _*.dat_ - contains all the data which is compressed by the algorithm

These files are then compressed with LZMA (http://www.7-zip.org/sdk.html), which again improves compression. If the file can not be compressed by the algorithm. It might not contain any values or is not in the format we expect it to be (e.g. script files, log files, executables, ...) the files will still be compressed with LZMA. Because there is no compressable data the file is not split up in two different parts and is simply saved as _*.lzma_.

# Usage

```
A command line tool for compressing Open Foam (r) files.

Usage:
  ofc.exe [-h|--help]
  ofc.exe [--version]
  ofc.exe compress directory <input> <output> [-f] [-r] [-p] [-s] [--rounding=<digits>]
  ofc.exe compress file <input> <output> [-f] [-s] [--rounding=<digits>]
  ofc.exe decompress directory <input> <output> [-f] [-r] [-p]
  ofc.exe decompress file <input> <output> [data] [-f]

Methods:
  compress directory    Compress the specified directory.
  compress file         Compress the specified file.
  decompress directory  Decompress the specified compressed directory.
  decompress file       Decompress the specified compressed file or directory.

Options:
  -h --help            Display this help message.
  --version            Display the current version of the tool.
  --rounding           Enable rounding to the specified amount of digits.
  -f                   Force overriding of files.
  -r                   Enable recursive compression/decompression.
  -p                   Enable parallel compression/decompression.
  -s                   Treat anonymous lists as lists of one type.
```

> General note: always provide __absolute__ paths instead of relative paths when providing paths.

# Requirements
__Windows__: In order to run OFC on windows you need to have the _.NET framwork 4.5_ installed, which is preinstalled on most machines nowadays. If the _.NET framework 4.5_ is not installed on your machine you can download it [here](http://www.microsoft.com/de-at/download/details.aspx?id=30653).

__Windows, Linux, Mac & Docker__: In order to run the _core_ version of OFC you will need to install _.NET core_. Downloads for the different platforms and the installation instructions can be found [here](http://www.microsoft.com/net/core).

# Installation
Make sure you are meeting the requirements above before continuing with the installation. 

Download the latest release version for your platform from [here](../../releases). Choose the __Win__ version if you are running on windows and choose the __Core__ version if you are running on either Linux, Mac or Docker.

Extract all the files in a directory of your choosing. It might be a good idea to create a symbolic link in order to have easier access to the tool. 

In order to run the tool under the __Win__ version you simply have to run the _Ofc.exe_. If you want to run the __Core__ version you have to use the _dotnet toolchain_ and run the tool like this: `dotnet Ofc.dll`. 

# Future
While the project is on hold there are ideas that might be implemented in the future:

 - Integration into OpenFOAM®
 - Integration into ParaView
 - File specific configuration
 - More lossless compression methods
 - LZMA specific pre-processing
 - Faster processing of files

# Bugs/Features & Co
If you find any bugs, have any new feature requests or something else related to the project leave an [issue](../../issues). Please remember that the project is currently on hold and there _might_ not be a quick (or even any) response to the issues raised.
