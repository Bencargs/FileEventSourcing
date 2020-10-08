<p align="center"><img width=12.5% src="https://github.com/Bencargs/FileEventSourcing/blob/master/Content/Icon.png"></p>
<p align="center"><img width=60% src="https://github.com/Bencargs/FileEventSourcing/blob/master/Content/Logo.png"></p>


## Basic Overview
Adds a source control backup system for files on disk.

<br>

## Motivation
Typical backup systems rely on creating copies of a file at regular intervals (eg. nightly).

This not only loses the history of modifications during backups, but is also an inefficient use of resources -
    typically modifications only affect a small portion of a file's contents, but a backup will copy the entire file.

By using event sourcing, every modification to a file is tracked, and only the portions of the file that are modified are recorded.

## Recreate a File at a Previous Version
<img src="https://github.com/Bencargs/FileEventSourcing/blob/master/Content/Demo.gif" width=100%>

<br>

## How it works
<p align="center"><img width=95% src="https://github.com/Bencargs/FileEventSourcing/blob/master/Content/Diagram.png"></p>

FileEventSourcing registers a File System Watcher to monitor for updates to a files' last changed timestamp.

On notification of a file update, a rebuild is performed against all previous saved events to construct the file at last known state.

A binary compare is performed against the last known state, and the current file state,
sections of modified binary are then serialized and saved back to the event store.

<br>

#### Add to Source Control
```powershell
Console.exe add -f C:\Temp\test\c.bmp
```

#### Preview a file at previous version
```powershell
Console.exe preview -f C:\Temp\test\c.bmp -v 1 -o C:\Temp\test\preview.bmp
```

#### How to use
To clone and run this application, you'll need Git installed on your computer. From your command line:
```powershell
# Clone this repository
git clone https://github.com/Bencargs/FileEventSourcing/FileEventSourcing.git

# Run the FileEventSourcing process
dotnet FileEvents\bin\FileEventSourcing.dll

# Add a file to monitor via the Console
.\FileEvents\bin\Console add -f C:\Temp\a.txt

# Preview a file at a specific change version
.\FileEvents\bin\Console preview -f 'C:\Temp\a.txt' -v 3
```
