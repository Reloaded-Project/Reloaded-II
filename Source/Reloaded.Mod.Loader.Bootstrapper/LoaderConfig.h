#pragma once
#include "json.hpp"
#include "CoreCLR.hpp"
#include "ReloadedPaths.h"

using json = nlohmann::json;

class LoaderConfig
{
	public:
		LoaderConfig();
		~LoaderConfig() = default;

		json config;

		string_t get_loader_path();
		string_t get_launcher_path();
		string_t get_runtime_config_path();
	
		ReloadedPaths get_loader_paths();
};

