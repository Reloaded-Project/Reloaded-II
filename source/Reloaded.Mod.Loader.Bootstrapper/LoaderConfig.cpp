#include "pch.h"
#include "LoaderConfig.h"
#include "Utilities.h"
#include <Shlobj_core.h>
#include <fstream>
#include <locale>
#include <codecvt>
#include "ReloadedPaths.h"

// Maximum path length supported by Windows extended paths
constexpr DWORD MAX_PATH_BUFFER_SIZE = 32767;

LoaderConfig::LoaderConfig()
{
	// Get path to AppData
	char_t buffer[MAX_PATH_BUFFER_SIZE]; // Max Windows10+ path.
	BOOL result = SHGetSpecialFolderPath(nullptr, buffer, CSIDL_APPDATA, false);

	if (!result)
		throw std::exception("Failed to obtain the path of the AppData folder.");

	// Get path to Reloaded Config
	const string_t appData = string_t(buffer);
	const string_t reloadedConfigPath = appData + L"\\Reloaded-Mod-Loader-II\\ReloadedII.json";

	if (!Utilities::file_exists(reloadedConfigPath))
		throw std::exception("Reloaded config has not been found.");

	// Get loader path.
	std::ifstream configFile = std::ifstream(reloadedConfigPath);
	config = json::parse(configFile);
}

namespace
{
	bool TryGetCurrentModulePath(string_t& outPath)
	{
		const auto raw_path_buffer = std::make_unique<wchar_t[]>(MAX_PATH_BUFFER_SIZE);
		
		// Get current module handle using GetModuleHandleEx with GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS
		// to get the module that contains this function's code address
		HMODULE hModule = nullptr;
		if (!GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | 
								GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
								reinterpret_cast<LPCWSTR>(TryGetCurrentModulePath),
								&hModule))
		{
			return false;
		}

		// Get module filename with extended path support
		const DWORD result = GetModuleFileNameW(hModule, raw_path_buffer.get(), MAX_PATH_BUFFER_SIZE);
		if (result == 0 || result == MAX_PATH_BUFFER_SIZE)
			return false;

		// Normally we would rely on GetModuleFileNameW, but there's this thing called 'Xbox Game Pass', which has some
		// legacy DRM mechanisms inherited from UWP; it pretends files are in a virtualized location. We must ask Windows
		// for the real path of a file.
		const auto final_path_buffer = std::make_unique<wchar_t[]>(MAX_PATH_BUFFER_SIZE);
		string_t result_path;

		// Open the file to get a handle
		const HANDLE file_handle = CreateFileW(
			raw_path_buffer.get(),
			0,
			FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
			nullptr,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			nullptr
		);

		if (file_handle != INVALID_HANDLE_VALUE)
		{
			// Get the canonical path if the file handle is valid
			// final_result has length of string.
			const DWORD final_result = GetFinalPathNameByHandleW(file_handle, final_path_buffer.get(), MAX_PATH_BUFFER_SIZE, FILE_NAME_NORMALIZED);
			CloseHandle(file_handle);

			// Check that the returned string is in_bounds, if not, use the raw path
			// Note: It should always be in bounds unless Windows extends file path limits beyond NTFS limits.
			if (final_result > 0 && final_result < MAX_PATH_BUFFER_SIZE)
				result_path = final_path_buffer.get();
			else
				result_path = raw_path_buffer.get();
		}
		else
			result_path = raw_path_buffer.get();

		// Remove the extended path prefix if present
		const string_t prefix = L"\\\\?\\";
		if (result_path.rfind(prefix, 0) == 0)
			result_path = result_path.substr(prefix.length());

		outPath = result_path;
		return true;
	}
}

string_t LoaderConfig::get_loader_path()
{
	#if _WIN64
	const std::string stringLoaderPath = config["LoaderPath64"];
	#else
	const std::string stringLoaderPath = config["LoaderPath32"];
	#endif

	/* std::string is non-wide and will not handle unicode characters on Windows.
	 * Need to convert back to wide characters.
	 *
	 * This is to support file paths with international locale e.g. Cyrillic, Chinese, Japanese.
	*/
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
	const string_t loaderPath = converter.from_bytes(stringLoaderPath);

	if (!Utilities::file_exists(loaderPath)) {
		std::string errorMessage = "Reloaded-II Loader DLL has not been found.\nTo fix this, start the Reloaded launcher.";
		
		string_t modulePath;
		if (TryGetCurrentModulePath(modulePath)) {
			std::string dllFolderPath = converter.to_bytes(modulePath);
			errorMessage = "Reloaded-II Loader DLL has not been found.\nTo fix this, start the Reloaded launcher.\n\nIf you intended to uninstall Reloaded, delete:\n\n" + dllFolderPath;
		}

		throw std::exception(errorMessage.c_str());
	}

	return loaderPath;
}

string_t LoaderConfig::get_runtime_config_path()
{
	// Get runtime configuration path.
	const string_t loaderPath = get_loader_path();
	const string_t runtimeConfigPath = loaderPath.substr(0, loaderPath.size() - 4) + L".runtimeconfig.json";
	if (!Utilities::file_exists(runtimeConfigPath))
		throw std::exception("Reloaded Mod Loader runtime configuration has not been found.");

	return runtimeConfigPath;
}

string_t LoaderConfig::get_launcher_path()
{
	const std::string stringLauncherPath = config["LauncherPath"];
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
	const string_t launcherPath = converter.from_bytes(stringLauncherPath);

	if (!Utilities::file_exists(launcherPath))
		throw std::exception("Reloaded Mod Loader DLL has not been found.");

	return launcherPath;
}

ReloadedPaths LoaderConfig::get_loader_paths()
{
	const string_t loaderPath = get_loader_path();
	const string_t runtimeConfigPath = get_runtime_config_path();
	return ReloadedPaths(loaderPath, runtimeConfigPath);
}