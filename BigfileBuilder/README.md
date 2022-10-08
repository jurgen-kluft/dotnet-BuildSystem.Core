# Bigfile

## TOC

The TOC file can contain multiple TOC-Sections.

The main TOC layout:
  - Int32: Number of Entries (Total)
  - Int32: Number of Sections
  - Array {Int32:toc count, Int32:toc offset, Int64:data offset}

## TOC Section

A TOC Section layout:
  - Array {Int32: file offset, Int32: file size}
    - Offset can be an offset to a FileId[], depends on bit 31 in FileSize
  - Array {Int32: length, FileId[]: array}

## Bigfile Filenames and Hashes

These are now following the same layout as mentioned above containing multiple sections.

