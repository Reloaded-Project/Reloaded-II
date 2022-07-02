#pragma once
#include "CoreCLR.hpp"
#include "ReloadedPaths.h"
#include "tiny-json/tiny-json.h"

using json = json_t;

class LoaderConfig
{
	public:
		static constexpr int MaxFields = 256;

		LoaderConfig();
		~LoaderConfig() = default;

		std::string configText;
		json const* config_parent;
		json config_buffer[MaxFields];

		string_t get_loader_path();
		string_t get_launcher_path();
		string_t get_runtime_config_path();
	
		ReloadedPaths get_loader_paths();
};

