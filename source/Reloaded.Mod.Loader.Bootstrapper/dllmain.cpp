// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <iostream>
#include <fstream>
#include "Shlobj_core.h"
#include "ReloadedPaths.h"
#include "CoreCLR.hpp"
#include "nlohmann/json.hpp"

#include <locale>
#include <codecvt>
#include <string>
#include "Utilities.h"
#include "LoaderConfig.h"
#include <shellapi.h>
#include <sstream>
#include "EntryPointParameter.h"

using json = nlohmann::json;

// Constants
static string_t PORTABLE_MODE_FILE	= L"ReloadedPortable.txt";
const string_t PARAMETER_LAUNCH		= L"--launch";
const string_t PARAMETER_ARGUMENTS	= L"--arguments";
const string_t PARAMETER_KILL		= L"--kill";

// Global Variables
CoreCLR* CLR;
HMODULE thisProcessModule;
ReloadedPaths paths;
HANDLE initializeThreadHandle;
HANDLE bootstrapperMemoryMappedFileHandle;
EntryPointParameters entryPointParameters;

// Reloaded Init Functions
DWORD WINAPI load_reloaded_async(LPVOID lpParam);
bool preload_setup_info();

void reboot_via_launcher(); // If ReloadedPortable.txt exists in DLL directory.
bool is_reloaded_already_loaded();
bool is_reloaded_bootstrapper_already_loaded();
void set_reloaded_bootstrapper_already_loaded();
std::wstring get_reloaded_bootstrapper_name();
bool load_reloaded(ReloadedPaths& reloadedPaths);

/* Entry point */
BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
		case DLL_PROCESS_ATTACH:
			thisProcessModule = hModule;
			if (preload_setup_info())
			{
				initializeThreadHandle = CreateThread(nullptr, 0, &load_reloaded_async, 0, 0, nullptr);
			}
			break;

		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
			break;

		case DLL_PROCESS_DETACH:
			// Unloading mod loader not supported.
			CloseHandle(bootstrapperMemoryMappedFileHandle);
			break;
    }
    return TRUE;
}

bool preload_setup_info()
{
	try
	{
		if (!is_reloaded_already_loaded() || !is_reloaded_bootstrapper_already_loaded())
		{
			/* Set bootstrapper loaded flag. */
			set_reloaded_bootstrapper_already_loaded();

			/* Reboot via launcher if in portable mode. */
			auto dllDirectory = Utilities::get_current_directory(thisProcessModule);
			if (Utilities::file_exists(dllDirectory + L"\\" + PORTABLE_MODE_FILE))
				reboot_via_launcher();

			/* Add current directory to list of DLL paths. */
			SetDllDirectoryW(dllDirectory.c_str());

			/* Find Reloaded */
			LoaderConfig config = LoaderConfig();

			/* Load Reloaded */
			paths = config.get_loader_paths();
			return true;
		}
	}
	catch (std::exception& exception)
	{
		std::cerr << exception.what() << std::endl;
		MessageBoxA(nullptr, exception.what(), "[Bootstrapper] Failed to Prepare Reloaded-II Loading", MB_OK);
	}

	return false;
}

DWORD WINAPI load_reloaded_async(LPVOID lpParam)
{
	load_reloaded(paths);
	return 0;
}

/**
 * \brief Reboots the application through the use of the launcher.
 */
void reboot_via_launcher()
{
	// Output
	int numArgs;
	LPWSTR* args = nullptr;

	// Get Command Line
	LPWSTR rawCommandLine = GetCommandLineW();
	args = CommandLineToArgvW(rawCommandLine, &numArgs);

	// Assemble commandline components.
	std::wstring launchPath = args[0];
	std::wstring arguments;

	for (int x = 1; x < numArgs; x++)
	{
		arguments += L" ";
		arguments += args[x];
	}

	// Make launcher commandline.
	LoaderConfig config = LoaderConfig();
	std::wstring commandLine = L"\"" + config.get_launcher_path() + L"\" "; // Reloaded Launcher
	commandLine += PARAMETER_LAUNCH + L" \"" + launchPath + L"\" "; // Launch path
	commandLine += PARAMETER_ARGUMENTS + L" \"" + arguments + L"\" "; // Arguments
	commandLine += PARAMETER_KILL + L" \"" + std::to_wstring(GetCurrentProcessId()) + L"\" "; // Kill this process.

	// Launch launcher.
	STARTUPINFOW si;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	if (CreateProcessW(nullptr, (LPWSTR) commandLine.c_str(), nullptr, nullptr, false, DETACHED_PROCESS, nullptr, nullptr, &si, &pi))
	{
		CloseHandle(pi.hProcess);
		CloseHandle(pi.hThread);
	}
}

/**
 * \brief Loads the Reloaded Mod Loader into the current process given a set of paths.
 * \param reloadedPaths The paths to Reloaded components.
 * \return false if failed to load Reloaded, true otherwise./
 */
bool load_reloaded(ReloadedPaths& reloadedPaths)
{
	int success = 0;
	CLR = new CoreCLR(&success);

	if (!success)
		throw std::exception("Failed to load the `hostfxr` library. Did you copy nethost.dll?");

	// Load runtime and execute our method.
	if (!CLR->load_runtime(reloadedPaths.runtimeConfigPath))
		throw std::exception("Failed to load .NET Core Runtime");

	const string_t typeName = L"Reloaded.Mod.Loader.EntryPoint, Reloaded.Mod.Loader";
	const string_t methodName = L"Initialize";
	component_entry_point_fn initialize = nullptr;

	if (!CLR->load_assembly_and_get_function_pointer(reloadedPaths.dllPath.c_str(), typeName.c_str(), methodName.c_str(),
		nullptr, nullptr, (void**) &initialize))
	{
		throw std::exception("Failed to load .NET assembly.");
	}

	// Set path to current dll
	// Using GetModuleFileNameW
	entryPointParameters.dll_path = new wchar_t[MAX_PATH];
	GetModuleFileNameW(thisProcessModule, entryPointParameters.dll_path, MAX_PATH);
	initialize(&entryPointParameters, sizeof(EntryPointParameters));
	return true;
}

/**
 * \brief Returns true if Reloaded is already loaded, else false.
 */
bool is_reloaded_already_loaded()
{
	const std::wstring memoryMappedFileName = L"Reloaded-Mod-Loader-Server-PID-" + std::to_wstring(GetCurrentProcessId());
	const HANDLE hMapFile = OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, memoryMappedFileName.c_str());

	const bool loaded = (hMapFile != nullptr);
	if (hMapFile != nullptr)
		CloseHandle(hMapFile);
	
	return loaded;
}

/**
 * \brief Returns true if Reloaded is already loaded, else false.
 */
bool is_reloaded_bootstrapper_already_loaded()
{
	const std::wstring memoryMappedFileName = get_reloaded_bootstrapper_name();
	const HANDLE hMapFile = OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, memoryMappedFileName.c_str());

	const bool loaded = (hMapFile != nullptr);
	if (hMapFile != nullptr)
		CloseHandle(hMapFile);

	return loaded;
}

/**
 * \brief Returns true if Reloaded is already loaded, else false.
 */
void set_reloaded_bootstrapper_already_loaded()
{
	const std::wstring memoryMappedFileName = get_reloaded_bootstrapper_name();
	bootstrapperMemoryMappedFileHandle = CreateFileMappingW(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, 4, memoryMappedFileName.c_str());
}

/**
 * \brief Returns the name of the memory mapped file for the Reloaded Bootstrapper.
 */
std::wstring get_reloaded_bootstrapper_name()
{
	return L"Reloaded-Mod-Loader-Bootstrapper-PID-" + std::to_wstring(GetCurrentProcessId());
}

/* Exports for different mod loaders. */
struct ModInfoDummy
{
	int version;
	char padding[256];
};

extern "C"
{
	// Note: MainMemory's Mod Loaders have inconsistent entry points (some having helper functions, some not). Not exporting proper defs.
	__declspec(dllexport) ModInfoDummy MainMemoryModInfo = { 1 };

	// Entry point for Ultimate ASI Loader, to allow waiting for Reloaded II creation thread to terminate.
	__declspec(dllexport) void InitializeASI()
	{
		std::cout << "[Reloaded II Bootstrapper] Ultimate ASI Loader Entrypoint Hit" << std::endl;
		entryPointParameters.flags |= LoadedExternally;
		WaitForSingleObject(initializeThreadHandle, INFINITE);
	}
}