#pragma once
#include "CoreCLR.hpp"
#include "ReloadedPaths.h"


class LoaderConfig
{
	public:
		LoaderConfig() { };
		~LoaderConfig() = default;

		static string_t get_loader_path();
		static string_t get_launcher_path();
		static string_t get_runtime_config_path();

		static ReloadedPaths get_loader_paths();
};

