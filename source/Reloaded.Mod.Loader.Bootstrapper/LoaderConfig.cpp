#include "pch.h"
#include "LoaderConfig.h"
#include "Utilities.h"
#include <Shlobj_core.h>
#include <locale>
#include "ReloadedPaths.h"

#pragma warning(disable:4996) // _CRT_SECURE_NO_WARNINGS

string_t LoaderConfig::get_loader_path()
{
	#if _WIN64
	const string_t loaderPath = _wgetenv(L"RELOADEDII_LOADER64");
	#else
	const string_t loaderPath = _wgetenv(L"RELOADEDII_LOADER32");
	#endif
	
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
	const string_t launcherPath = _wgetenv(L"RELOADEDII_LAUNCHER");

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