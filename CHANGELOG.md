1.2 (6/9/2011)
--------------

### Features

* Added `/incremental` (`/i`) mode to svn2svn; detects all previously synced revisions and starts at the latest revision that has not been synced.
* Added `fs2svn`, a tool that synchronizes a file system folder to SVN.
* Added `svn2`, a tool that can recycle unversioned files (sync).
* Added `/stopOnCopy[+/-]` to `svn2svn`, default isto follow branch/copy history.
* Added `/root` option to `svn2svn`, specifies ancestor svn path.
* Added support for specifying `HEAD` in `svn2svn --revision:start:end`.
* `Svn2svn` will assume `HEAD` revision when end revision omitted in `--revision`.
* Modified files are displayed in `svn2svn` console output before each action with an `M` marker.

### Bugs

* Bug: fixed unclear "index out of range" error in `svn2svn` when specifying an invalid revision range with `--revision`.

1.1 (11/29/2009)
----------------

* Updated to SharpSVN 1.6003.1304, supports SVN 1.6.

1.0 (02/22/2009)
----------------

* Initial release, supports SVN 1.5.
