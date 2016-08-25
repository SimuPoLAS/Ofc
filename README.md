# OFC
OFC is a tool for compressing OpenFOAM® files. It can compress files lossless or by rounding values to a specified amount of digits. It does compression by grouping numbers which have a similarity. They might have the same exponents, can all be expressed by a linear function, switch between other groups (ping-pong) or need the same amount of bits to write.

When an input file is compressed it is split up into two different files: 

 - _*.meta_ - contains all the metadata which is not getting compressed by the algorithm
 - _*.dat_ - contains all the data which is compressed by the algorithm

These files are then compressed with LZMA (http://www.7-zip.org/sdk.html), which again improves compression. If the file can not be compressed by the algorithm. It might not contain any values or is not in the format we expect it to be (e.g. script files, log files, executables, ...) the files will still be compressed with LZMA. Because there is no compressable data the file is not split up in two different parts and is simply saved as _*.lzma_.

# Command-line-interface (CLI)

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

> General note: always provide __absolute__ paths instead of _relative_ paths when providing paths.

# Usage
This section describes the general usage of the tool and the parameters shown in the [#usage section](#usage), but in more detail. When providing paths always provide __absolute__ paths and remember to put them in _double-quotes_  `"` if they contain _spaces_.

## Compressing files
> __Syntax__: `ofc.exe compress file <input> <output> [-f] [-s] [--rounding=<digits>]` 

-----

Compressing the file `/usr/data` and storing the compressed data in the file `/usr/out`. 
We are using the option `-f` in order to __override__ the file if it does already exist:

`ofc.exe compress file /usr/data /usr/out -f` 

-----

Compressing the specified file while rounding all list values to `2` digits after the decimal point (option `--rounding` - who would have thought):

`ofc.exe compress file /usr/data /usr/out -f --rounding=2` 

> __Rounding__ only affects items __inside__ of a _list_. Numbers outside of a list are __not__ rounded.

-----

Compressing the specified file while _treating anonymous lists as lists of one type_ (option `-s` - for _simple-treating_):

`ofc.exe compress file /usr/data /usr/out -f -s` 

> More information about the `-s` option can be found [here](#treating-anonymous-lists-as-lists-of-one-type).


## Compressing directories
> __Syntax__: `ofc.exe compress directory <input> <output> [-f] [-r] [-p] [-s] [--rounding=<digits>]` 

-----

Compressing the __top level__ files of `/usr/data/` and storing the compressed data in the directory  `/usr/out/`.
We are using `-f` in order to create the output directory if it does not exist:

`ofc.exe compress directory /usr/data/ /usr/out/ -f`

-----

Compressing __all__ files inside of the specified directory using the option `-r` (recursive):

`ofc.exe compress directory /usr/data/ /usr/out/ -f -r`

-----

Compressing all files inside of the specified directory by using _parallelization_ (option `-p`), which speeds up the process - especially if there are a lot of files:

`ofc.exe compress directory /usr/data/ /usr/out/ -f -r -p`

-----

Compressing all files inside of the specified directory while rounding all list values to `2` digits after the decimal point (option `--rounding`):

`ofc.exe compress directory /usr/data /usr/out -f -r --rounding=2` 

> __Rounding__ affects only items __inside__ of a _list_. Numbers outside of a list are __not__ rounded.

-----

Compressing all files inside of the specified directory while _treating anonymous lists as lists of one type_ (option `-s` - for _simple_):

`ofc.exe compress directory /usr/data /usr/out -f -s` 

> More information about the `-s` option can be found [here](#treating-anonymous-lists-as-lists-of-one-type).

## Decompressing files
> __Syntax__: `ofc.exe decompress file <input> <output> [data] [-f]` 

-----

Decompressing the file `/usr/out.meta` and storing the decompressed file as `/usr/data`. 
The `-f` (force) option is supplied in order to _override_ the output file:

`ofc.exe decompress file /usr/out.meta /usr/data -f` 

-----

Decompressing the specified file while explicitly specifying a _data file_ (the default is the _input_ file with a `.dat` ending - e.g. _info file_: `out.meta`; _default data file_: `out.dat`):

`ofc.exe decompress file /usr/out.meta /usr/out.dat -f` 

> _Note_: In this case the specification of the _data_ file is irrelevant as it is the _default_ value, but in special cases it is useful to supply a _data file_ explicit.

## Decompressing directories
> __Syntax__: `ofc.exe decompress directory <input> <output> [-f] [-r] [-p]` 

-----

Decompressing the __top level__ files of `/usr/out/` and storing the uncompressed data in the directory  `/usr/data/`.
We are using `-f` in order to create the output directory if it does not exist:

`ofc.exe decompress directory /usr/out/ /usr/data/ -f`

-----

Decompressing __all__ files inside of the specified compressed directory using the option `-r` (recursive):

`ofc.exe decompress directory /usr/out/ /usr/data/ -f -r`

-----

Decompressing all files inside of the specified directory by using _parallelization_ (option `-p`), which speeds up the process - especially if there are a lot of files:

`ofc.exe decompress directory /usr/out/ /usr/data/ -f -r -p`

## Treating anonymous lists as lists of one type
By setting the `-s` option when _compressing_ the behavior when parsing an _anonymous list_ changes.
Normally there is no way to know what type the contents of the list have.
There might be a _number_ then a _string_ - back to _number_ and so on.
This possibility of changing types __dramatically__ reduces the performance of the compression.

By enabling the `-s` option an _anonymous list_ can only have of __one__ type for all items. Here is an example of a list which can be optimized by the `-s` option as all element of the list have the type _number_:

`10 (1 2 3 4 5 6 7 8 9 10)`

If the `-s` option is enabled the following example would fail as the list contains a mix of _numbers_ and _characters_.

`10 (1 a 2 b 3 c 4 d 5 e)`

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

