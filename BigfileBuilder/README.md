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

## Bigfile Organization

So during development we will have multiple loose Bigfiles and only for a final release we will merge them into one Bigfile.
Every Bigfile has an associated `index`, and each FileId that is written in the game data is composed of a Bigfile `index` 
and a BigfileFile `index`.

There is one Bigfile per DataUnit.

Note: At this moment we do not care about de-duplication, we could add it later.
