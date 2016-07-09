# OFC
OFC is a tool for compressing OpenFOAM® files. It can compress files lossless or by rounding all values to a specified amount of digits. It does compression by grouping numbers into groups which have the same exponents, can be expressed by a linerar function, switch between groups (ping-pong) or by the amount of bits needed to write them.

Each input file is split up into two files: 

 - _.meta_ - contains all the not compressed data
 - _.dat_ - contains all the compressed data

If the file can not be compressed - the file is not in the right format or does not contain compressable data - the file is simply packed into a _.lzma_ file.

# Usage

```
A command line tool for compressing Open Foam files.

Usage:
  ofc.exe [-h|--help]
  ofc.exe [--version]
  ofc.exe compress directory <input> <output> [-f] [-r] [-p] [-s] [--rounding=<digits>]
  ofc.exe compress file <input> <output> [-f] [-s] [--rounding=<digits>]
  ofc.exe decompress directory <input> <output> [-f] [-r] [-p]
  ofc.exe decompress file <input> <output> [data] [-f]

Methods:
  compress directory    Compresses the specified directory.
  compress file         Compresses the specified file.
  decompress directory  Decompresses the specified compressed directory.
  decompress file       Decompresses the specified compressed file or set of files.

Options:
  -h --help            Displays this help message.
  --version            Displays the current version of the tool.
  -f                   Enables force mode.
  -r                   Enables recursive compression/decompression.
  -p                   Enables parallel compression/decompression.
```

# Future
While the project is on hold there are ideas that might be implemented in the future:

 - Integration into OpenFOAM®
 - Integration into ParaView
 - File specific configuration instead of project configuration (via CLI)
 - More lossless compression methods
 - LZMA specific pre-processing
 - General faster processing of files
