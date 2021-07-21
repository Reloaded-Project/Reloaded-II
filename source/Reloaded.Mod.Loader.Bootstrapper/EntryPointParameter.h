#pragma once

/* For C# source, see EntryPoint.cs */

enum EntryPointFlags : int
{
	None = 0,
	
	/* Reloaded was initialised by an external mod loader or DLL Hijacking method. */
	LoadedExternally = 1,
};

/**
 * Defines the parameters.
 */
struct EntryPointParameters
{
public:
	EntryPointFlags flags { None };

	EntryPointParameters() = default;
};

DEFINE_ENUM_FLAG_OPERATORS(EntryPointFlags);