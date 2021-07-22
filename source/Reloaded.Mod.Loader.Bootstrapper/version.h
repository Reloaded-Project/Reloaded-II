#pragma once
#include "EntryPointParameter.h"

#define STRINGIZE2(s) #s
#define STRINGIZE(s) STRINGIZE2(s)

#define VERSION_MAJOR               CURRENT_VERSION

#define VER_FILE_DESCRIPTION_STR    "C++ Bootstrapper to Load Reloaded II"
#define VER_FILE_VERSION            VERSION_MAJOR, 0, 0, 0
#define VER_FILE_VERSION_STR        STRINGIZE(VERSION_MAJOR)

#define VER_PRODUCTNAME_STR         "Reloaded II Bootstrapper"
#define VER_PRODUCT_VERSION         VER_FILE_VERSION
#define VER_PRODUCT_VERSION_STR     VER_FILE_VERSION_STR
#define VER_ORIGINAL_FILENAME_STR   VER_PRODUCTNAME_STR ".dll"
#define VER_INTERNAL_NAME_STR       VER_ORIGINAL_FILENAME_STR
#define VER_COPYRIGHT_STR           "Copyright (C) Sewer56, Licensed under GPLV3"

#ifdef _DEBUG
#define VER_VER_DEBUG             VS_FF_DEBUG
#else
#define VER_VER_DEBUG             0
#endif

#define VER_FILEOS                  VOS_NT_WINDOWS32
#define VER_FILEFLAGS               VER_VER_DEBUG
#define VER_FILETYPE                VFT_APP
