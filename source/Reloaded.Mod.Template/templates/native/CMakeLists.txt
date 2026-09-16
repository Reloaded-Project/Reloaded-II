cmake_minimum_required(VERSION 3.15)
project(Reloaded.Native.Template LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_library(${PROJECT_NAME} SHARED main.cpp)

# Match the game's architecture:
#   64-bit game: cmake -B build -A x64
#   32-bit game: cmake -B build -A Win32
if (CMAKE_SIZEOF_VOID_P EQUAL 8)
    set_target_properties(${PROJECT_NAME} PROPERTIES OUTPUT_NAME "${PROJECT_NAME}")
else()
    set_target_properties(${PROJECT_NAME} PROPERTIES OUTPUT_NAME "${PROJECT_NAME}32")
endif()


if (DEFINED ENV{RELOADEDIIMODS})
    set(MOD_OUTPUT_DIR "$ENV{RELOADEDIIMODS}/${PROJECT_NAME}")
    file(MAKE_DIRECTORY "${MOD_OUTPUT_DIR}")

    add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_if_different "$<TARGET_FILE:${PROJECT_NAME}>" "${MOD_OUTPUT_DIR}/$<TARGET_FILE_NAME:${PROJECT_NAME}>"
        COMMAND ${CMAKE_COMMAND} -E copy_if_different "${CMAKE_CURRENT_SOURCE_DIR}/ModConfig.json" "${MOD_OUTPUT_DIR}/ModConfig.json"
        COMMAND ${CMAKE_COMMAND} -E copy_if_different "${CMAKE_CURRENT_SOURCE_DIR}/ConfigSchema.json" "${MOD_OUTPUT_DIR}/ConfigSchema.json"
        COMMENT "Deploying mod to ${MOD_OUTPUT_DIR}")
endif()
