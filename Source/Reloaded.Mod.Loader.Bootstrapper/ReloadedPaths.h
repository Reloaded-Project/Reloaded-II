#pragma once
#include <string>
#include <utility>
#include "nethost/nethost.h"
using string_t = std::basic_string<char_t>;

class ReloadedPaths
{
	public:
		string_t dllPath;
		string_t runtimeConfigPath;

		ReloadedPaths() { }
		ReloadedPaths(string_t reloadedDllPath, string_t runtimeConfigPath) : dllPath(std::move(reloadedDllPath)), 
																			  runtimeConfigPath(std::move(runtimeConfigPath))
		{ };

		~ReloadedPaths() = default;
};
