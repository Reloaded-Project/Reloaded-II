#include "pch.h"
#include "Utilities.h"
#include <fstream>

bool Utilities::file_exists(string_t file_path)
{
	std::ifstream file_stream;
	file_stream.open(file_path);
	return file_stream.good();
}

string_t Utilities::get_current_directory(HMODULE hModule)
{
	char_t host_path[MAX_PATH];
	int bufferSize = sizeof(host_path) / sizeof(char_t);
	GetModuleFileNameW(hModule, host_path, bufferSize);

	return get_directory_name(host_path);
}

string_t Utilities::get_directory_name(string_t filePath)
{
	auto pos = filePath.find_last_of('\\');
	filePath = filePath.substr(0, pos + 1);
	return filePath;
}
