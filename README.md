# cap2musicxml

This small tool converts [Capella](https://www.capella-software.com) V2 files into [MusicXml](https://www.musicxml.com) V3.0.
The implementation is based on an old developer package provided by the vendor years ago (C2FORMAT.TXT).

The software can be used e.g. to load scores into current editors (like my current favorit [MuseScore](https://musescore.org)) which are not able to read Capella files from version 2 (only version 3 and above).

# usage

 > cap2musicxml.exe MyScore.cap

Running this command will try to load the score and try to save it as MusicXml into MyScore.cap.musicxml.

# build and test

The software is build and tested currently only using Visual Studio 2017 (Community Edition).

[![Build status](http://ci.appveyor.com/api/projects/status/github/lg2de/cap2musicxml?svg=true)](https://ci.appveyor.com/project/lg2de/cap2musicxml)

# contribution

You are welcome to create pull requests for bug fixes and other improvements.

If you are not a developer you are welcome to report bugs.
Therefore uploaded capella files are welcome including information on expected and actual result.
