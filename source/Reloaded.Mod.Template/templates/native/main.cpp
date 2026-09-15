#include <windows.h>
#include "ReloadedModConfig.h"

// Called by ReloadedStartEx after the mod loader handed us the directories
static void mod_start()
{
    auto& config = reloaded::config();

    bool enabled = config.get_bool("EnableThing", true);
    long long volume = config.get_int("Volume", 75);
    double brightness = config.get_float("Brightness", 1.5);

    static const char* qualityMembers[] = { "Low", "Medium", "High" };
    int quality = config.get_enum("Quality", qualityMembers, 3, 2);

    std::wstring file = config.get_wstring("CustomFile", L"");

    // Example: handle the user changing settings in the launcher while the game runs.
    // auto watcher = config.watch([](reloaded::ModConfig& cfg) { ... });

    wchar_t message[512];
    swprintf_s(message, L"[Native Template] enabled=%d volume=%lld brightness=%.2f quality=%d file='%ls'\n",
               enabled ? 1 : 0, volume, brightness, quality, file.c_str());
    OutputDebugStringW(message);
}

// Exports ReloadedStartEx and wires it to mod_start.
// The other Reloaded exports (ReloadedSuspend, ReloadedResume, ...) are optional.
RELOADED_MOD_CONFIG_IMPL(mod_start)
