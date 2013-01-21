dandy-doc
=========

A simple documentation generation library to help you make documentation how you want it.

About
=====

Dandy Doc is a simple library to help you generate documentation for your projects how you want it.
Instead of configuring a tool you can use Dandy Doc to write your own code that will generate docs.

License
-------

Using an MIT/X11 license. See `dandy-doc.license` for details.

Known Issues
============

Cecil
-----

Currently Mono.Cecil is used to read assemblies. Cecil is great and all but is not designed to be used concurrently, even for read operations. This project contains tests that confirm the theading issues and also tests that confirm the workaround for those issues. So far it appears to be thread safe for the use of Dandy Doc with some workarounds (immediate loading and an immediate assembly resolver) but for now it is best to keep things serial.

Build
=====

Prerequisites
-------------

The solution should "work" with only Visual Studio 2012 but I havn't tested that. You should have the following installed before you attempt to build:

- Visual Studio 2012 Update 1 or later.
- NuGet 2.2 or later, make sure you update!
- Code Contracts: [http://research.microsoft.com/en-us/projects/contracts/](http://research.microsoft.com/en-us/projects/contracts/)

Configurations
--------------

After meeting the prerequisites you can build one of 3 configurations.

- Debug: the typical configuration, used for most development.
- DebugCC: a debug build with code contracts and code analysis enabled (slow)
- Release: uhh, release.

Tests
=====

I am lazy and a bad person so the tests will not work everywhere.
The MSDN link tests will require an internet connection and also will require MSDN to be up and running. Any tests involving documentation resulting from code contracts will require code contracts.

Thanks
======

Thanks to [@yallie](https://github.com/yallie "yallie") for leading the way on generating MSDN links.