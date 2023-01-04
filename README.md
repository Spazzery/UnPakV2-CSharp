# UnPakV2-CSharp

I initially wrote this in Python, but switched over to C#, hence the name.

An unpacker for PAK files on PS Vita

There already exists an UnPAK reposiory for unpacking PAK files on PS Vita, simply called UnPAK. However, that only worked for certain versions of PAK archives.
I created my own version from scratch that uses a lot from that repository. Here's the repo: https://github.com/NameSubjecttoChange/unPAK

I tested this current script on Grisaia Side Episode and Hatsumira.

It's not a very difficult file format, though some things I still don't fully understand.

Byte order: Little Endian

# Structure

Consists of:
* Header (20 bytes)
* File info (8 bytes * [File count])
* Nametable (n * [File count] times)
* Files

## Header
##### Total 24 bytes
The header of the PAK file is

Name | Size | Comment
--- | --- | ---
First file offset | 4 bytes | Offset where the first file is in the archive (only in Grisaia Side Episode and Hatsumira though)
File count | 4 Bytes | Amount of files inside archive
Version number? | 4 bytes | Value is 0x01, so probably means version number
??? | 4 bytes | Some unknown value 0x200 (512 in decimal)
Offset where the nameTable begins | 4 bytes | nameTable contains file names inside archive


## File Info
##### 8 bytes (each)
Repeat that [File count] times

Name | Size | Comment
--- | --- | ---
File offset | 4 bytes | Offset for file inside the archive
File size | 4 bytes | File's size

## Nametable
##### n bytes (each)
Repeat that [File count] times

Each filename is separated by a null (0x00).
Ends with a couple of null bytes that are repeated until [Files offset]

## Raw data
##### Total n bytes (each)
Repeat this [File count] times

Name | Size |
--- | --- 
Raw data | [File Size] bytes from respective info structure

---

NOTE: to create this README, I used https://github.com/DanOl98/MagesPack/blob/master/README.md as a template.
