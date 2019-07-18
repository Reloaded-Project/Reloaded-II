// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <iostream>
#include <fstream>
#include "Shlobj_core.h"
#include "ReloadedPaths.h"
#include "CoreCLR.hpp"
#include "json.hpp"

#include <locale>
#include <codecvt>
#include <string>

using json = nlohmann::json;
using string_t = std::basic_string<char_t>;

// Global Variables
CoreCLR* CLR;
bool launchExecuted = false;
HMODULE thisProcessModule;

// Reloaded Init Functions
ReloadedPaths find_reloaded(int& success);
extern "C" __declspec(dllexport) void launch_reloaded();
bool load_reloaded(ReloadedPaths& reloadedPaths);
DWORD WINAPI launch_reloaded_async(LPVOID lpParam);

// Utility Functions
string_t get_directory_name(string_t filePath);
string_t get_current_directory(HMODULE hModule);
bool file_exists(string_t file_path);

// For exports.
int port;

/* Entry point */
BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
		case DLL_PROCESS_ATTACH:
			thisProcessModule = hModule;
			CreateThread(NULL, 0, &launch_reloaded_async, 0, 0, nullptr);
			break;

		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
			break;

		case DLL_PROCESS_DETACH:
			// Unloading mod loader not supported.
			break;
    }
    return TRUE;
}

DWORD WINAPI launch_reloaded_async(LPVOID lpParam)
{
	launch_reloaded();
	return 0;
}

void launch_reloaded()
{
	if (!launchExecuted)
	{
		launchExecuted = true;

		/* Add current directory to list of DLL paths. */
		auto dllDirectory = get_current_directory(thisProcessModule);
		SetDllDirectoryW(dllDirectory.c_str());

		/* Find Reloaded */
		int findResult = 0;
		ReloadedPaths paths = find_reloaded(findResult);

		if (findResult == 0)
			return;

		/* Load Reloaded*/
		load_reloaded(paths);


	}
}

/**
 * \brief Finds the path to Reloaded Mod Loader components.
 * \param success Returns 1 if the operation succeeds, else 0.
 * \return The path to Reloaded Mod Loader components. This path is null if the components are not found.
 */
ReloadedPaths find_reloaded(int& success)
{
	// Get path to AppData
	char_t buffer[MAX_PATH];
	BOOL result = SHGetSpecialFolderPath(nullptr, buffer, CSIDL_APPDATA, false);

	if (!result)
	{
		success = 0;
		std::cerr << "Failed to obtain the path of the AppData folder." << std::endl;
		return ReloadedPaths();
	}

	// Get path to Reloaded Config
	string_t appData = string_t(buffer);
	string_t reloadedConfigPath = appData + L"\\Reloaded-Mod-Loader-II\\ReloadedII.json";

	if (!file_exists(reloadedConfigPath))
	{
		success = 0;
		std::cerr << "Reloaded config has not been found." << std::endl;
		return ReloadedPaths();
	}

	// Get loader path.
	std::ifstream configFile = std::ifstream(reloadedConfigPath);
	json configJson = json::parse(configFile);
	std::string stringLoaderPath = configJson["LoaderPath"];

	/* std::string is non-wide and will not handle unicode characters on Windows. 
	 * Need to convert back to wide characters. 
	 * 
	 * This is to support file paths with international locale e.g. Cyrillic, Chinese, Japanese.
	*/
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
	string_t loaderPath = converter.from_bytes(stringLoaderPath);

	if (!file_exists(loaderPath))
	{
		success = 0;
		std::cerr << "Reloaded Mod Loader DLL has not been found." << std::endl;
		return ReloadedPaths();
	}

	// Get runtime configuration path.
	string_t runtimeConfigPath = loaderPath.substr(0, loaderPath.size() - 4) + L".runtimeconfig.json";
	if (!file_exists(runtimeConfigPath))
	{
		success = 0;
		std::cerr << "Reloaded Mod Loader runtime configuration has not been found." << std::endl;
		return ReloadedPaths();
	}

	success = 1;
	return ReloadedPaths(loaderPath, runtimeConfigPath);
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
	{
		std::cerr << "Failed to load the `hostfxr` library." << std::endl;
		return false;
	}

	// Load runtime and execute our method.
	if (!CLR->load_runtime(reloadedPaths.runtimeConfigPath))
	{
		std::cerr << "Failed to load .NET Core Runtime" << std::endl;
		return false;
	}

	const string_t typeName = L"Reloaded.Mod.Loader.EntryPoint, Reloaded.Mod.Loader";
	const string_t methodName = L"GetPort";
	component_entry_point_fn getPort = nullptr;

	if (!CLR->load_assembly_and_get_function_pointer(reloadedPaths.dllPath.c_str(), typeName.c_str(), methodName.c_str(),
		nullptr, nullptr, (void**) &getPort))
	{
		std::cerr << "Failed to load C# assembly." << std::endl;
	}

	port = getPort(nullptr, 0);
	return true;
}

/* Utility functions. */

/**
 * \brief Returns true if a file with a given path exists.
 * \param file_path The absolute path to the file.
 * \return True if the file exists, else false.
 */
bool file_exists(string_t file_path)
{
	std::ifstream file_stream;
	file_stream.open(file_path);
	return file_stream.good();
}


/**
 * \brief Returns the absolute path to the current directory
 * \return The absolute path to the current directory.
 */
string_t get_current_directory(HMODULE hModule)
{
	char_t host_path[MAX_PATH];
	int bufferSize = sizeof(host_path) / sizeof(char_t);
	GetModuleFileNameW(hModule, host_path, bufferSize);

	return get_directory_name(host_path);
}

/**
 * \brief Returns the full path to a directory referenced by a file.
 * \param filePath The full path to a file.
 * \return The full path to a directory referenced by a file.
 */
string_t get_directory_name(string_t filePath)
{
	auto pos = filePath.find_last_of('\\');
	filePath = filePath.substr(0, pos + 1);
	return filePath;
}