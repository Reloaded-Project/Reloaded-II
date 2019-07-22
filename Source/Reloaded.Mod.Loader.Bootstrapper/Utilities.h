#pragma once
#include "pch.h"
#include "CoreCLR.hpp"

class Utilities
{
	public:
		/**
		* \brief Returns true if a file with a given path exists.
		* \param file_path The absolute path to the file.
		* \return True if the file exists, else false.
		 */
		static bool file_exists(string_t file_path);

		/**
		* \brief Returns the absolute path to the current directory
		* \return The absolute path to the current directory.
		*/
		static string_t get_current_directory(HMODULE hModule);

		/**
		* \brief Returns the full path to a directory referenced by a file.
		* \param filePath The full path to a file.
		* \return The full path to a directory referenced by a file.
		*/
		static string_t get_directory_name(string_t filePath);
};
