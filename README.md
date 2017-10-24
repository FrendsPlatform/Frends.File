# Frends.File
FRENDS Tasks to preform various file system based operations.

- [Installing](#installing)
- [Documentation](#documentation)
    - [Pattern matching](#pattern-matching)
- [Tasks](#tasks)
    - [Read](#read)
	- [ReadBytes](#readbytes)
    - [Write](#write)
	- [WriteBytes](#writebytes)
    - [Find](#find)
    - [Move](#move)
    - [Copy](#copy)
    - [Delete](#delete)
    - [Rename](#rename)
- [License](#license)
- [Building](#building)
- [Contributing](#contributing)   

Installing
==========
You can install the task via FRENDS UI Task view or you can find the nuget package from the following nuget feed
`https://www.myget.org/F/frends/api/v2`

Documentation
===============
Pattern matching
----------------

Find, Move and Delete tasks use pattern matching for finding files.

The search starts from the root directory defined in the input parameters.

`* to match one or more characters in a path segment`

`** to match any number of path segments, including none`

Examples:

`**\output\*\temp\*.txt` matches:

* `test\subfolder\output\2015\temp\file.txt`
* `production\output\2016\temp\example.txt`


`**\temp*` matches

* `prod\test\temp123.xml`
* `test\temp234.xml`

`subfolder\**\temp\*.xml` matches

* `subfolder\temp\test.xml`
* `subfolder\foo\bar\is\here\temp\test.xml`

Tasks
=====

## Read
File.Read task reads the string contents of one file.

Input:

| Property        | Type     | Description                      | Example                             |
|-----------------|----------|----------------------------------|-------------------------------------|
| Path            | string   | Full path to the file to be read.| `c:\temp\foo.txt` `c:/temp/foo.txt` |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to read files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format `domain\username`        | `example\Admin`           |
| Password                                    | string         | | |
| FileEncoding                                | Enum           | Encoding for the read content. By selecting 'Other' you can use any encoding. | |
| EncodingInString                            | string         | The name of encoding to use. Required if the FileEncoding choice is 'Other'. A partial list of supported encoding names: https://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings(v=vs.110).aspx | `iso-8859-1` |

Result:

| Property        | Type     | Description                 |
|-----------------|----------|-----------------------------|
| Content         | string   |                             |
| Path            | string   | Full path for the read file |
| SizeInMegaBytes | double   |                             |
| CreationTime    | DateTime |                             |
| LastWriteTime   | DateTime |                             |


## ReadBytes
File.ReadBytes task reads contents of a file as a byte array.

Input:

| Property        | Type     | Description                       | Example                               |
|-----------------|----------|-----------------------------------|---------------------------------------|
| Path            | string   | Full path to the file to be read. | `c:\temp\img1.png` `c:/temp/img2.png` |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to read files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format `domain\username`        | `example\Admin`           |
| Password                                    | string         |                                                |                           |

Result:

| Property        | Type     | Description                 |
|-----------------|----------|-----------------------------|
| ContentBytes    | byte[]   |                             |
| Path            | string   | Full path for the read file |
| SizeInMegaBytes | double   |                             |
| CreationTime    | DateTime |                             |
| LastWriteTime   | DateTime |                             |


## Write
File.Write task writes string content to a file.

Input:

| Property        | Type     | Description                  | Example                 |
|-----------------|----------|------------------------------|---------------------------|
| Path            | string   | Full path to the file to be written to. | `c:\temp\foo.txt` `c:/temp/foo.txt` |
| Content         | string   | | |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to write files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format `domain\username`        | `example\Admin`           |
| Password                                    | string         | | |
| FileEncoding                                | Enum           | Encoding for the content. By selecting 'Other' you can use any encoding. | |
| EncodingInString                            | string         | This should be filled if the FileEncoding choice is 'Other' A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List | `iso-8859-1` |
| EnableBom                                   | bool           | Visible if UTF-8 is used as the option for FileEncoding. | |
| WriteBehaviour                              | Enum{Append,Overwrite,Throw} | Determines how the File.Write works when the destination file already exists | |

Result: 

| Property        | Type   | Description                 |
|-----------------|--------|-----------------------------|
| Path            | string | Full path to the written file |
| SizeInMegaBytes | double |                             |


## WriteBytes
File.WriteBytes task writes a byte array to a file.

Input:

| Property        | Type     | Description                             | Example                             |
|-----------------|----------|-----------------------------------------|-------------------------------------|
| Path            | string   | Full path to the file to be written to. | `c:\temp\img.png` `c:/temp/img.png` |
| ContentBytes    | byte[]   | | |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to write files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format `domain\username`        | `example\Admin`           |
| Password                                    | string         |                                                |                           |
| WriteBehaviour                              | Enum{Append,Overwrite,Throw} | Determines how the File.Write works when the destination file already exists | |

Result:

| Property        | Type   | Description                   |
|-----------------|--------|-------------------------------|
| Path            | string | Full path to the written file |
| SizeInMegaBytes | double |                               |


## Find
File.Find task is used for finding detailed information about multiple- or a single file. The task uses [pattern matching](#pattern-matching)  for finding files.

Input:

| Property        | Type     | Description                         | Example                 |
|-----------------|----------|-------------------------------------|-------------------------|
| Directory       | string   | Root folder where the search starts.| `c:\root\folder`        |
| Pattern         | string   | Pattern to match for files.         | `**\*.xml` `sub\*.txt`  |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to find files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format domain\username | `example\Admin` |
| Password                                    | string         | | |

Result is a list of objects with following properties:

| Property          | Type     | Description                       |
|-------------------|----------|-----------------------------------|
| Extension         | string   | Extension for the file            |
| DirectoryName     | string   | Directory where the file exists   |
| FullPath          | string   | Full path for the file            |
| FileName          | string   | File name including the extension |
| IsReadOnly        | bool     |                                   |
| SizeInMegaBytes   | double   |                                   |
| CreationTime      | DateTime |                                   |
| CreationTimeUtc   | DateTime |                                   |
| LastAccessTime    | DateTime |                                   |
| LastAccessTimeUtc | DateTime |                                   |
| LastWriteTime     | DateTime |                                   |
| LastWriteTimeUtc  | DateTime |                                   |

## Move
The File.Move task is used for moving one or more files from a directory to another. The task uses [pattern matching](#pattern-matching) for finding files to move.

Input:

| Property        | Type     | Description                         | Example                 |
|-----------------|----------|-------------------------------------|-------------------------|
| Directory       | string   | Root folder where the search starts.| `c:\root\folder`        |
| Pattern         | string   | Pattern to match for files.         | `**\*.xml` `sub\*.txt`  |
| TargetDirectory | string   | Target folder where the files are to be moved | `\\shared\folder`       |

Options: 

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to move files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format domain\username | `example\Admin` |
| Password                                    | string         | | |
| PreserveDirectoryStructure                  | bool           | If set, will recreate the directory structure from the SourceDirectory under the TargetDirectory for moved files | |
| CreateTargetDirectories                     | bool           | If set, will create the target directory if it does not exist, as well as any sub directories if `PreserveDirectoryStructure` is set. | |
| IfTargetFileExists                          | Enum {Throw,Rename,Overwrite} | What should happen if a file with the same name already exists in the target directory. Rename will rename the transferred file by appending a number to the end.

Result is a list of objects with following properties:

| Property   | Type   | Description               |
|------------|--------|---------------------------|
| SourcePath | string | Original path of the file |
| Path       | string | New path of the file      |

## Copy
The File.Copy task is used for copying one or more files from a directory to another. The task uses [pattern matching](#pattern-matching) for finding files to copy.

Input:

| Property        | Type     | Description                         | Example                 |
|-----------------|----------|-------------------------------------|-------------------------|
| Directory       | string   | Root folder where the search starts.| `c:\root\folder`        |
| Pattern         | string   | Pattern to match for files.         | `**\*.xml` `sub\*.txt`  |
| TargetDirectory | string   | Target folder where the files are to be copied | `\\shared\folder`       |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to copy files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format domain\username | `example\Admin` |
| Password                                    | string         | | |
| PreserveDirectoryStructure                  | bool           | If set, will recreate the directory structure from the SourceDirectory under the TargetDirectory for copied files | |
| CreateTargetDirectories                     | bool           | If set, will create the target directory if it does not exist, as well as any sub directories if `PreserveDirectoryStructure` is set. | |
| IfTargetFileExists                          | Enum {Throw,Rename,Overwrite} | What should happen if a file with the same name already exists in the target directory. Rename will rename the transferred file by appending a number to the end.

Result is a list of objects with following properties:

| Property   | Type   | Description               |
|------------|--------|---------------------------|
| SourcePath | string | Original path of the file |
| Path       | string | New path of the file      |

## Delete 
File.Delete task is used for deleting multiple- or a single file from one directory to another. The task uses [pattern matching](#pattern-matching)  for finding files to delete.

Input:

| Property        | Type     | Description                         | Example                 |
|-----------------|----------|-------------------------------------|-------------------------|
| Directory       | string   | Root folder where the search starts.| `c:\root\folder`        |
| Pattern         | string   | Pattern to match for files.         | `**\*.xml` `sub\*.txt`  |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to delete files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format domain\username | `example\Admin` |
| Password                                    | string         | | |

Result is a list of objects with following properties:

| Property        | Type   | Description                           |
|-----------------|--------|---------------------------------------|
| Path            | string | Path for the deleted file             |
| SizeInMegaBytes | string |  |

## Rename

File.Rename is used for renaming a single file.

Input:

| Property        | Type     | Description                          | Example                 |
|-----------------|----------|--------------------------------------|-------------------------|
| Path            | string   | Full path to the file to be renamed. | `c:\root\folder\example.txt`        |
| NewFileName     | string   | The new filename including extension | `newName.txt`  |

Options:

| Property                                    | Type           | Description                                    | Example                   |
|---------------------------------------------|----------------|------------------------------------------------|---------------------------|
| UseGivenUserCredentialsForRemoteConnections | bool           | If set, allows you to give the user credentials to use to delete files on remote hosts. If not set, the agent service user credentials will be used. Note: For deleting directories on the local machine, the agent service user credentials will always be used, even if this option is set.| |
| UserName                                    | string         | Needs to be of format domain\username | `example\Admin` |
| Password                                    | string         | | |
| RenameBehaviour         | Enum{Throw, Overwrite, Rename} | How the file rename should work if a file with the new name already exists. If Rename is selected, will append a number to the new file name, e.g. `renamed(2).txt` | |

Result:

| Property        | Type   | Description                           |
|-----------------|--------|---------------------------------------|
| Path            | string | Path for the renamed file             |

License
=======

This project is licensed under the MIT License - see the LICENSE file for details

Building
========
Ensure that you have `https://www.myget.org/F/frends/api/v2` added to your nuget feeds

Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.File.git`

Restore dependencies

`nuget restore frends.file`

Rebuild the project

Run Tests with nunit3. Tests can be found under

`Frends.File.Tests\bin\Release\Frends.File.Tests.dll`

Create a nuget package

`nuget pack nuspec/Frends.File.nuspec`

Contributing
============
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!
