# BlackOps3MapRenamer
Easily rename your custom zombies map file with few clicks. I searched through all of the files of black ops 3 for my map name and made note of them to make this tool. I skipped linker, cache files, binary files. These will be generated for the renamed map after you compile & link again after you rename the map. I tested this tool with a few maps and they all work perfectly fine as long as you follow the usage steps.  
  
If you believe that there are still some files that need to be processed that I may have not noticed, please create an issue for that.

## Features
- Makes a backup (found in the same folder of the executable)
- Duplicates the map
- Renames inside files
- Renames led (built light) files
- Renames map source files
- Renames <map_name>::<function> calls inside scripts
- Renames usings of the <map_name> inside scripts
- Renames all files inside the map folder that's found in usermap

## Installation
- dotnet framework 4.8 runtime (https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
- The tool .exe

## Usage
- Close mod tools.
- Click the browse button and locate to your map folder found in usermaps folder (e.g. ...common\Call of Duty Black Ops III\usermaps\zm_giant).
- Write a new name for the map (without zm_).
- (Optional) Tick Duplicate checkbox if you want to keep the original map and instead rename a duplicated map.
- Click the Rename button and wait for it to finish processing.
- Open mod tools and build for all languages (compile & link) and you're done!

![Screenshot](BlackOps3MapRenamer.png)
