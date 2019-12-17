#include "pch.h"
#include "LoaderConfig.h"
#include "Utilities.h"
#include <Shlobj_core.h>
#include <fstream>
#include <locale>
#include <codecvt>
#include "ReloadedPaths.h"

LoaderConfig::LoaderConfig()
{
	// Get path to AppData
	char_t buffer[MAX_PATH];
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

	if (!Utilities::file_exists(loaderPath))
		throw std::exception("Reloaded Mod Loader DLL has not been found.");

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