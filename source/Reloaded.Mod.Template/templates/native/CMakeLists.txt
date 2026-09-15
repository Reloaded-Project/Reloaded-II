cmake_minimum_required(VERSION 3.15)
project(Reloaded.Native.Template LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_library(Reloaded.Native.Template SHARED main.cpp)

# Match the game's architecture:
#   64-bit game: cmake -B build -A x64
#   32-bit game: cmake -B build -A Win32
# then build and copy the DLL next to ModConfig.json.
if (CMAKE_SIZEOF_VOID_P EQUAL 8)
    set_target_properties(Reloaded.Native.Template PROPERTIES OUTPUT_NAME "Reloaded.Native.Template")
else()
    set_target_properties(Reloaded.Native.Template PROPERTIES OUTPUT_NAME "Reloaded.Native.Template32")
endif()
