# Bigfile

## TOC

The TOC file can contains multiple TOC-Sections, so the first Array is an array containing Offsets to Sections.

The main TOC layout:
  - Int32: Total number of Entries
  - Int32: Number of Sections
  - {Int32:toc offset, Int32:toc count, Int64:data offset}: Offset and Count of each Section

## TOC Section

A TOC Section layout:
  - Entry[]:
    - File Offset (may offset to a FileId[], depends on bit 31 in FileSize)
    - File Size
  - Many {Int32: length, FileId[]: array}

## Bigfile Filenames and Hashes

These are now following the same layout as mentioned above containing multiple sections.

