#include "pch.h"
#include "CoreCLR.hpp"
#include <iostream>

CoreCLR::CoreCLR(int* success)
{
	if (load_hostfxr())
		*success = 1;

	success = 0;
}

/* Core public functions */
bool CoreCLR::load_hostfxr()
{
	// Get the path to CoreCLR's hostfxr
	char_t buffer[MAX_PATH];
	size_t buffer_size = sizeof(buffer) / sizeof(char_t);
	int rc = get_hostfxr_path(buffer, &buffer_size, NULL);
	if (rc != 0)
		return false;

	// Load hostfxr and get desired exports
	void* lib = load_library(buffer);
	m_init_fptr = (hostfxr_initialize_for_runtime_config_fn)get_export(lib, "hostfxr_initialize_for_runtime_config");
	m_get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)get_export(lib, "hostfxr_get_runtime_delegate");
	m_close_fptr = (hostfxr_close_fn)get_export(lib, "hostfxr_close");

	return (m_init_fptr && m_get_delegate_fptr && m_close_fptr);
}

bool CoreCLR::load_runtime(const string_t runtime_config_path)
{
	m_load_assembly_and_get_function_pointer = get_dotnet_load_assembly(runtime_config_path.c_str());
	return m_load_assembly_and_get_function_pointer != nullptr;
}

bool CoreCLR::load_assembly_and_get_function_pointer(const char_t* assembly_path, const char_t* type_name,
	const char_t* method_name, const char_t* delegate_type_name, void* reserved, void** delegate)
{
	int result = m_load_assembly_and_get_function_pointer(assembly_path, type_name, method_name, delegate_type_name, reserved, delegate);
	return (result == 0 && delegate != nullptr);
};


/* Helpers */
void* CoreCLR::load_library(const char_t* path)
{
	HMODULE handle = LoadLibraryW(path);
	return static_cast<void*>(handle);
}

void* CoreCLR::get_export(void* h, const char* name)
{
	void* f = GetProcAddress((HMODULE)h, name);
	return f;
}

load_assembly_and_get_function_pointer_fn CoreCLR::get_dotnet_load_assembly(const char_t* config_path)
{
	// Load .NET Core
	hostfxr_handle context = nullptr;
	int rc = m_init_fptr(config_path, nullptr, &context);
	if (rc != 0 || context == nullptr)
	{
		m_close_fptr(context);
		return nullptr;
	}

	// Get the load assembly function pointer
	m_get_delegate_fptr(context, hdt_load_assembly_and_get_function_pointer, (void**)&m_load_assembly_and_get_function_pointer);
	m_close_fptr(context);
	return (load_assembly_and_get_function_pointer_fn) m_load_assembly_and_get_function_pointer;
}