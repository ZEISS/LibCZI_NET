@echo off
setlocal enabledelayedexpansion

set BUILD_TYPE=Debug

REM Save current directory
set ORIGINAL_DIR=%CD%

REM Navigate to the libCZI directory
cd /d ..\submodules\libCZI || exit /b 1

REM Create the build directory if it doesn't exist
if not exist build (
    mkdir build
)

REM Change into the build directory
cd build || exit /b 1

REM Run CMake with the specified options
cmake .. ^
 -DCMAKE_BUILD_TYPE=%BUILD_TYPE% ^
 -DLIBCZI_BUILD_CZICMD=OFF ^
 -DLIBCZI_BUILD_DYNLIB=OFF ^
 -DLIBCZI_BUILD_CURL_BASED_STREAM=ON ^
 -DLIBCZI_BUILD_AZURESDK_BASED_STREAM=ON ^
 -DLIBCZI_BUILD_PREFER_EXTERNALPACKAGE_LIBCURL=ON ^
 -DCMAKE_TOOLCHAIN_FILE=%VCPKG_ROOT%\scripts\buildsystems\vcpkg.cmake ^
 -DVCPKG_TARGET_TRIPLET=x64-windows-static ^
 -DLIBCZI_BUILD_UNITTESTS=OFF ^
 -DLIBCZI_BUILD_PREFER_EXTERNALPACKAGE_RAPIDJSON=OFF ^
 -DLIBCZI_BUILD_LIBCZIAPI=ON ^
 -DLIBCZI_DESTINATION_FOLDER_LIBCZIAPI=%ORIGINAL_DIR%\..\dotnet\LibCZI_Net\runtimes\win-x64\native 

cmake --build . --config %BUILD_TYPE% || exit /b 1

REM Return to the original directory
cd /d "%ORIGINAL_DIR%"

endlocal