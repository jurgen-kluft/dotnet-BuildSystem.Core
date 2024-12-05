# Bigfile Format

## Bigfile

This just stores many files into a single file, when a Bigfile is generated the offsets to each file are cached which are then used to write the TOC.
Every file is aligned in the Bigfile to 64 bytes, the reason for this is so that we can keep the offset 32-bit which would limit us to 4GB for the Bigfile but now that we align to 64 bytes we can have a Bigfile of 64 * 4GB = 256GB.

## TOC (Table of Contents)

A TOC contains N sections and each section contains M files, and the size of a TOC is limited to 1GB due to the offsets being 32-bit.

### Section

We introduce the concept of a 'Section' which is nothing more than a TOC. Since we can have multiple generated Bigfiles where we merge all of them into a single one we want to be able to merge them without changing a File-Id. So a Bigfile is given a fixed 'Index' and a file within a Bigfile also has an 'Index'. A File-Id thus exists of [Bigfile-Index, File-Index].

So a Bigfile can contain multiple Sections and each Section contains one or multiple Files. 

## Reading a Bigfile TOC

When reading a bigfile we first read the header. The header contains the following information:

- Number of Sections: 4 bytes
- Sections[]:
  - Offset to SectionTOC (from the start of the file): 4 bytes
  - Count (number of files in this section): 4 bytes
- SectionTOC:
  - [Offset to file: 4 bytes, Size of file: 4 bytes, Offset to Children: 4 bytes]

## Children

A File-Id can have children, for example when one or more files are converted into more than 1 file. In such a case there is the main output file and the children. The TOC entry indicates if it has children, if so the 'Offset to Children' is set to the offset of the first child. The order of the children is always the same and is determined by the implementation of the 'Compiler' that was responsible for converting the file(s).

- Count: 4 bytes
- Index[Count]: 4 * Count bytes  (Index is an index into this SectionTOC)

## Filenames

The filenames are stored in a separate file. The section is stored at the end of the file. The section starts with the number of filenames and then the filenames are stored one after the other. The filenames are stored as UTF-8 strings. The filenames are stored in the order of the File-Ids.


## Content Hashes

Every file has a content hash. The content hash is a SHA-256 hash of the content of the file. The content hashes are stored in a separate file.
