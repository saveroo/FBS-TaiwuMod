@echo off

REM Define the source and destination paths
set "backend_source_directory=C:\Projects\C#\TaiwuMod\Solution1\FBSBackend\bin\Debug\net6.0"
set "frontend_source_directory=C:\Projects\C#\TaiwuMod\Solution1\FBSFrontend\bin\Debug"
set "destination_directory=E:\Games\Steam\steamapps\common\The Scroll Of Taiwu\Mod\FBS\Plugins"
set "root_destination_directory=E:\Games\Steam\steamapps\common\The Scroll Of Taiwu\Mod\FBS"

REM Create the destination directory if it doesn't exist
if not exist "%destination_directory%" (
    mkdir "%destination_directory%"
)

REM Copy settings
set "source_config=C:\Projects\C#\TaiwuMod\Solution1\Config.lua"
set "source_settings=C:\Projects\C#\TaiwuMod\Solution1\Settings.lua"

REM Specify the file names
set "backfile_name=FBSBackend.dll"
set "frontfile_name=FBSFrontend.dll"

REM Copy the files from the source to the destination directory
copy /y "%backend_source_directory%\%backfile_name%" "%destination_directory%\"
copy /y "%frontend_source_directory%\%frontfile_name%" "%destination_directory%\"
copy /y "%source_config%" "%root_destination_directory%\"
copy /y "%source_settings%" "%root_destination_directory%\"

echo "Build files copied successfully!"