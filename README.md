## Svn2svn

Svn2svn is a tool that replays and applies changes across SVN repositories, on Windows. It's intended to be a lighter version of svnsync or one that doesn't require to start at revision 0 or copying entire repositories from root.

* [Download 1.2](http://code.dblock.org/downloads/svn2svn/Svn2Svn.1.2.zip)

### Features

* supports copying across repositories
* no zero-revision requirements, this is a change replay tool
* supports non-rooted paths (copy a subtree towards another subtree)
* supports add/delete/modify
* optional revision range
* simulation mode
* prompts on commit

### Usage

```
Svn2Svn: 1.0.12201.0
/simulationOnly[+|-]   Simulation mode, don't commit anything. Default value:'-' (short form /x)
/source:<string>       Source SVN path. (short form /s)
/destination:<string>  Target SVN path, default to current. (short form /d)
/revision:<string>     Copy from revision. (short form /r)
/prompt[+|-]           Describe the change and prompt before commiting. Default value:'-' (short form /p)
@<file>                Read response file for more options
```

### Sample Run

I wrote this tool to migrate an existing project to CodePlex. 
[Here's a part](https://gist.github.com/dblock/5278124) of the first run that did revision 2 to 136.

### License and Copyright

Copyright (c) 2009-2013 Daniel Doubrovkine, Vestris Inc., [MIT License](LICENSE)
