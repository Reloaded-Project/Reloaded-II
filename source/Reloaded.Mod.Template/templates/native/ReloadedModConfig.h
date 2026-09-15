/*
    ReloadedModConfig.h

    Single header helper for Reloaded-II native (C/C++) mods with configuration.
    Drop this file next to your sources, include it, and define the entry macro
    in exactly one source file:

        #include "ReloadedModConfig.h"

        void mod_start()
        {
            auto& config = reloaded::config();
            bool enabled = config.get_bool("EnableThing", true);
            int volume   = (int)config.get_int("Volume", 100);
        }

        RELOADED_MOD_CONFIG_IMPL(mod_start)

    The macro exports ReloadedStartEx, which the mod loader calls with the mod's
    directories before anything else. reloaded::config() then reads the values
    written by the launcher from  <user config folder>/<your values file>.json,
    falling back to the defaults declared in your ConfigSchema.json.

    Requires C++17 or newer. Windows only. No external dependencies.
*/

#ifndef RELOADED_MOD_CONFIG_H
#define RELOADED_MOD_CONFIG_H

#include <windows.h>

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <functional>
#include <iterator>
#include <map>
#include <optional>
#include <string>
#include <thread>
#include <utility>
#include <vector>

#ifndef RELOADED_MOD_CONFIG_DEFAULT_FILE
#define RELOADED_MOD_CONFIG_DEFAULT_FILE L"Config.json"
#endif

namespace reloaded
{
    /*
        ---------------
        Small JSON tree
        ---------------
    */
    class Json
    {
    public:
        enum class Type { Null, Bool, Number, String, Array, Object };

        Type type = Type::Null;
        bool boolean = false;
        double number = 0.0;
        std::string text;
        std::vector<Json> items;
        std::vector<std::pair<std::string, Json>> members;

        const Json* find(const char* key) const
        {
            if (type != Type::Object)
                return nullptr;

            for (const auto& member : members)
            {
                if (member.first == key)
                    return &member.second;
            }

            return nullptr;
        }

        // Parses a UTF-8 JSON document.
        static std::optional<Json> parse(const std::string& utf8)
        {
            size_t pos = 0;
            Json result;
            if (!parse_value(utf8, pos, result) || !skip_ws(utf8, pos) || pos != utf8.size())
                return std::nullopt;

            return result;
        }

        // Parses a UTF-8 (or ASCII) JSON file.
        static std::optional<Json> parse_file(const std::wstring& path)
        {
            std::string utf8;
            if (!read_all_text(path, utf8))
                return std::nullopt;

            return parse(utf8);
        }

    private:
        static bool read_all_text(const std::wstring& path, std::string& out)
        {
            HANDLE file = CreateFileW(path.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
            if (file == INVALID_HANDLE_VALUE)
                return false;

            out.clear();
            char buffer[8192];
            DWORD read = 0;
            while (ReadFile(file, buffer, sizeof(buffer), &read, nullptr) && read > 0)
                out.append(buffer, read);

            CloseHandle(file);
            return true;
        }

        static bool skip_ws(const std::string& s, size_t& pos)
        {
            while (pos < s.size() && (s[pos] == ' ' || s[pos] == '\t' || s[pos] == '\r' || s[pos] == '\n'))
                pos++;
            return true;
        }

        static bool parse_value(const std::string& s, size_t& pos, Json& out)
        {
            if (!skip_ws(s, pos) || pos >= s.size())
                return false;

            char c = s[pos];
            if (c == '{')
                return parse_object(s, pos, out);
            if (c == '[')
                return parse_array(s, pos, out);
            if (c == '"')
            {
                out.type = Type::String;
                return parse_string(s, pos, out.text);
            }
            if (c == 't' || c == 'f')
                return parse_bool(s, pos, out);
            if (c == 'n')
                return parse_null(s, pos, out);

            return parse_number(s, pos, out);
        }

        static bool parse_object(const std::string& s, size_t& pos, Json& out)
        {
            out.type = Type::Object;
            pos++; // consume '{'
            if (!skip_ws(s, pos))
                return false;
            if (pos < s.size() && s[pos] == '}')
            {
                pos++;
                return true;
            }

            while (true)
            {
                if (!skip_ws(s, pos) || pos >= s.size() || s[pos] != '"')
                    return false;

                std::string key;
                if (!parse_string(s, pos, key))
                    return false;

                if (!skip_ws(s, pos) || pos >= s.size() || s[pos] != ':')
                    return false;
                pos++;

                Json value;
                if (!parse_value(s, pos, value))
                    return false;

                out.members.emplace_back(std::move(key), std::move(value));
                if (!skip_ws(s, pos))
                    return false;

                if (pos >= s.size())
                    return false;

                if (s[pos] == ',')
                {
                    pos++;
                    continue;
                }

                if (s[pos] == '}')
                {
                    pos++;
                    return true;
                }

                return false;
            }
        }

        static bool parse_array(const std::string& s, size_t& pos, Json& out)
        {
            out.type = Type::Array;
            pos++; // consume '['
            if (!skip_ws(s, pos))
                return false;
            if (pos < s.size() && s[pos] == ']')
            {
                pos++;
                return true;
            }

            while (true)
            {
                Json value;
                if (!parse_value(s, pos, value))
                    return false;

                out.items.push_back(std::move(value));
                if (!skip_ws(s, pos))
                    return false;

                if (pos >= s.size())
                    return false;

                if (s[pos] == ',')
                {
                    pos++;
                    continue;
                }

                if (s[pos] == ']')
                {
                    pos++;
                    return true;
                }

                return false;
            }
        }

        static bool parse_string(const std::string& s, size_t& pos, std::string& out)
        {
            pos++; // consume '"'
            out.clear();
            while (pos < s.size())
            {
                unsigned char c = (unsigned char)s[pos];
                if (c == '"')
                {
                    pos++;
                    return true;
                }

                if (c == '\\')
                {
                    pos++;
                    if (pos >= s.size())
                        return false;

                    char escape = s[pos++];
                    switch (escape)
                    {
                    case '"':  out += '"';  break;
                    case '\\': out += '\\'; break;
                    case '/':  out += '/';  break;
                    case 'b':  out += '\b'; break;
                    case 'f':  out += '\f'; break;
                    case 'n':  out += '\n'; break;
                    case 'r':  out += '\r'; break;
                    case 't':  out += '\t'; break;
                    case 'u':
                    {
                        if (pos + 4 > s.size())
                            return false;

                        unsigned code = 0;
                        for (int x = 0; x < 4; x++)
                        {
                            char hex = s[pos + x];
                            code <<= 4;
                            if (hex >= '0' && hex <= '9') code |= (unsigned)(hex - '0');
                            else if (hex >= 'a' && hex <= 'f') code |= (unsigned)(hex - 'a' + 10);
                            else if (hex >= 'A' && hex <= 'F') code |= (unsigned)(hex - 'A' + 10);
                            else return false;
                        }
                        pos += 4;

                        // Surrogate pair support.
                        if (code >= 0xD800 && code <= 0xDBFF && pos + 6 <= s.size() && s[pos] == '\\' && s[pos + 1] == 'u')
                        {
                            unsigned low = 0;
                            bool valid = true;
                            for (int x = 0; x < 4; x++)
                            {
                                char hex = s[pos + 2 + x];
                                low <<= 4;
                                if (hex >= '0' && hex <= '9') low |= (unsigned)(hex - '0');
                                else if (hex >= 'a' && hex <= 'f') low |= (unsigned)(hex - 'a' + 10);
                                else if (hex >= 'A' && hex <= 'F') low |= (unsigned)(hex - 'A' + 10);
                                else { valid = false; break; }
                            }

                            if (valid && low >= 0xDC00 && low <= 0xDFFF)
                            {
                                code = 0x10000 + ((code - 0xD800) << 10) + (low - 0xDC00);
                                pos += 6;
                            }
                        }

                        append_utf8(out, code);
                        break;
                    }
                    default:
                        return false;
                    }

                    continue;
                }

                out += (char)c;
                pos++;
            }

            return false;
        }

        static bool parse_bool(const std::string& s, size_t& pos, Json& out)
        {
            if (s.compare(pos, 4, "true") == 0)
            {
                out.type = Type::Bool;
                out.boolean = true;
                pos += 4;
                return true;
            }

            if (s.compare(pos, 5, "false") == 0)
            {
                out.type = Type::Bool;
                out.boolean = false;
                pos += 5;
                return true;
            }

            return false;
        }

        static bool parse_null(const std::string& s, size_t& pos, Json& out)
        {
            if (s.compare(pos, 4, "null") == 0)
            {
                out.type = Type::Null;
                pos += 4;
                return true;
            }

            return false;
        }

        static bool parse_number(const std::string& s, size_t& pos, Json& out)
        {
            const char* start = s.c_str() + pos;
            char* end = nullptr;
            double value = strtod(start, &end);
            if (end == start)
                return false;

            out.type = Type::Number;
            out.number = value;
            pos += (size_t)(end - start);
            return true;
        }

        static void append_utf8(std::string& out, unsigned code)
        {
            if (code <= 0x7F)
            {
                out += (char)code;
            }
            else if (code <= 0x7FF)
            {
                out += (char)(0xC0 | (code >> 6));
                out += (char)(0x80 | (code & 0x3F));
            }
            else if (code <= 0xFFFF)
            {
                out += (char)(0xE0 | (code >> 12));
                out += (char)(0x80 | ((code >> 6) & 0x3F));
                out += (char)(0x80 | (code & 0x3F));
            }
            else
            {
                out += (char)(0xF0 | (code >> 18));
                out += (char)(0x80 | ((code >> 12) & 0x3F));
                out += (char)(0x80 | ((code >> 6) & 0x3F));
                out += (char)(0x80 | (code & 0x3F));
            }
        }
    };

    /*
        ----------------
        String utilities
        ----------------
    */
    inline std::string utf16_to_utf8(const wchar_t* text)
    {
        if (text == nullptr || text[0] == L'\0')
            return std::string();

        int size = WideCharToMultiByte(CP_UTF8, 0, text, -1, nullptr, 0, nullptr, nullptr);
        if (size <= 1)
            return std::string();

        std::string result((size_t)size - 1, '\0');
        WideCharToMultiByte(CP_UTF8, 0, text, -1, result.data(), size, nullptr, nullptr);
        return result;
    }

    inline std::wstring utf8_to_utf16(const std::string& text)
    {
        if (text.empty())
            return std::wstring();

        int size = MultiByteToWideChar(CP_UTF8, 0, text.c_str(), (int)text.size(), nullptr, 0);
        if (size <= 0)
            return std::wstring();

        std::wstring result((size_t)size, L'\0');
        MultiByteToWideChar(CP_UTF8, 0, text.c_str(), (int)text.size(), result.data(), size);
        return result;
    }

    inline std::wstring directory_of(const std::wstring& path)
    {
        size_t separator = path.find_last_of(L"\\/");
        if (separator == std::wstring::npos)
            return std::wstring();

        std::wstring directory = path.substr(0, separator);
        if (!directory.empty() && directory.back() != L'\\' && directory.back() != L'/')
            directory += L'\\';

        return directory;
    }

    // The loader hands us folders with no trailing slash, the paths built from them need one.
    inline std::wstring with_trailing_separator(const std::wstring& path)
    {
        if (path.empty() || path.back() == L'\\' || path.back() == L'/')
            return path;

        return path + L'\\';
    }

    // Folder containing the DLL (or EXE) this code was compiled into.
    inline const std::wstring& this_module_directory()
    {
        static std::wstring directory;
        if (directory.empty())
        {
            HMODULE module = nullptr;
            GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCWSTR)&this_module_directory, &module);

            wchar_t path[MAX_PATH * 4];
            DWORD length = GetModuleFileNameW(module, path, (DWORD)std::size(path));
            directory = directory_of(std::wstring(path, length));
        }

        return directory;
    }

    /*
        ---------------------
        Mod startup information
        ---------------------
    */
    struct NativeModInfo
    {
        std::wstring mod_directory;    // Folder with the mod's own files (ConfigSchema.json, ...).
        std::wstring config_directory; // Folder where the launcher writes user settings.
    };

    // Filled by the ReloadedStartEx export; safe to read after mod start.
    inline NativeModInfo& native_mod_info()
    {
        static NativeModInfo info;
        return info;
    }

    /*
        -----------
        ModConfig
        -----------
    */
    class ModConfig
    {
    public:
        explicit ModConfig(std::wstring values_file = RELOADED_MOD_CONFIG_DEFAULT_FILE)
            : _values_file(std::move(values_file)) {
        }

        // Directory of the mod itself.
        const std::wstring& mod_directory() const
        {
            resolve_paths();
            return _mod_directory;
        }

        // Directory where the launcher stores the values file.
        const std::wstring& config_directory() const
        {
            resolve_paths();
            return _config_directory;
        }

        // Full path of the values file.
        std::wstring values_path() const
        {
            resolve_paths();
            return _config_directory + _values_file;
        }

        // Full path of the schema file.
        std::wstring schema_path() const
        {
            resolve_paths();
            return _mod_directory + L"ConfigSchema.json";
        }

        // Reads the schema defaults and the current values from disk.
        bool load()
        {
            resolve_paths();
            auto schema = Json::parse_file(schema_path());
            if (schema)
                parse_defaults(*schema);

            auto values = Json::parse_file(values_path());
            if (!values)
                return false;

            _values = std::move(*values);
            remember_write_time();
            return true;
        }

        // True when the values file changed on disk since the last load.
        bool changed_on_disk() const
        {
            resolve_paths();
            WIN32_FILE_ATTRIBUTE_DATA data;
            if (!GetFileAttributesExW(values_path().c_str(), GetFileExInfoStandard, &data))
                return false;

            return CompareFileTime(&data.ftLastWriteTime, &_write_time) != 0;
        }

        // Starts a thread that reloads the config and calls the callback on change.
        // Keep the returned thread; detach it or join it on unload.
        // Dropping it on the floor while it still runs kills the process.
        [[nodiscard]] std::thread watch(const std::function<void(ModConfig&)>& callback, int poll_ms = 500)
        {
            return std::thread([this, callback, poll_ms]()
                {
                    while (!_stop_watching.load(std::memory_order_relaxed))
                    {
                        Sleep((DWORD)poll_ms);
                        if (_stop_watching.load(std::memory_order_relaxed))
                            break;

                        if (changed_on_disk())
                        {
                            load();
                            callback(*this);
                        }
                    }
                });
        }

        void stop_watching()
        {
            _stop_watching.store(true, std::memory_order_relaxed);
        }

        /*
            -------
            Getters
            -------
            Look up a property by name; missing or invalid values fall back to
            the schema default, then to the fallback argument.
        */

        bool has(const char* name) const
        {
            const Json* value = find_value(name);
            return value != nullptr;
        }

        bool get_bool(const char* name, bool fallback) const
        {
            const Json* value = find_value(name);
            if (value != nullptr && value->type == Json::Type::Bool)
                return value->boolean;

            const Json* def = find_default(name);
            if (def != nullptr && def->type == Json::Type::Bool)
                return def->boolean;

            return fallback;
        }

        long long get_int(const char* name, long long fallback) const
        {
            const Json* value = find_value(name);
            if (value != nullptr && value->type == Json::Type::Number)
                return (long long)value->number;

            const Json* def = find_default(name);
            if (def != nullptr && def->type == Json::Type::Number)
                return (long long)def->number;

            return fallback;
        }

        double get_float(const char* name, double fallback) const
        {
            const Json* value = find_value(name);
            if (value != nullptr && value->type == Json::Type::Number)
                return value->number;

            const Json* def = find_default(name);
            if (def != nullptr && def->type == Json::Type::Number)
                return def->number;

            return fallback;
        }

        // Strings and enums are stored as UTF-8; enums return the member name.
        std::string get_string(const char* name, const char* fallback = "") const
        {
            const Json* value = find_value(name);
            if (value != nullptr && value->type == Json::Type::String)
                return value->text;

            const Json* def = find_default(name);
            if (def != nullptr && def->type == Json::Type::String)
                return def->text;

            return fallback != nullptr ? fallback : "";
        }

        std::wstring get_wstring(const char* name, const wchar_t* fallback = L"") const
        {
            const Json* value = find_value(name);
            if (value != nullptr && value->type == Json::Type::String)
                return utf8_to_utf16(value->text);

            const Json* def = find_default(name);
            if (def != nullptr && def->type == Json::Type::String)
                return utf8_to_utf16(def->text);

            return fallback != nullptr ? fallback : L"";
        }

        // Returns the index of the enum member in 'members' (order matches ConfigSchema.json),
        // or 'fallback' when the value is missing or unknown.
        int get_enum(const char* name, const char* const* members, int member_count, int fallback) const
        {
            std::string value = get_string(name, "");
            for (int x = 0; x < member_count; x++)
            {
                if (value == members[x])
                    return x;
            }

            return fallback;
        }

    private:
        std::wstring _values_file;
        std::wstring _mod_directory;
        std::wstring _config_directory;
        Json _values;
        Json _schema_defaults;
        FILETIME _write_time = {};
        mutable std::atomic_bool _paths_resolved{ false };
        std::atomic_bool _stop_watching{ false };

        void resolve_paths() const
        {
            if (_paths_resolved.load(std::memory_order_relaxed))
                return;

            // Cast away to keep the getters const; resolution happens at most once.
            auto* self = const_cast<ModConfig*>(this);
            const NativeModInfo& info = native_mod_info();
            if (!info.mod_directory.empty())
            {
                self->_mod_directory = with_trailing_separator(info.mod_directory);
                self->_config_directory = with_trailing_separator(info.config_directory.empty() ? info.mod_directory : info.config_directory);
            }
            else
            {
                // Loaded by an older loader or another injector: assume the values live next to the DLL.
                const std::wstring& dll_directory = this_module_directory();
                self->_mod_directory = dll_directory;
                self->_config_directory = dll_directory;
            }

            self->_paths_resolved.store(true, std::memory_order_relaxed);
        }

        const Json* find_value(const char* name) const
        {
            return _values.find(name);
        }

        const Json* find_default(const char* name) const
        {
            return _schema_defaults.find(name);
        }

        void remember_write_time()
        {
            WIN32_FILE_ATTRIBUTE_DATA data;
            if (GetFileAttributesExW(values_path().c_str(), GetFileExInfoStandard, &data))
                _write_time = data.ftLastWriteTime;
        }

        // Pulls the defaults of the matching configuration out of the schema.
        void parse_defaults(const Json& schema)
        {
            _schema_defaults = Json();
            _schema_defaults.type = Json::Type::Object;

            const Json* configurations = schema.find("Configurations");
            if (configurations == nullptr || configurations->type != Json::Type::Array)
                return;

            std::string file_name = utf16_to_utf8(_values_file.c_str());
            const Json* selected = nullptr;
            for (const Json& configuration : configurations->items)
            {
                const Json* name = configuration.find("FileName");
                if (name == nullptr || name->text != file_name)
                    continue;

                selected = &configuration;
                break;
            }

            if (selected == nullptr && !configurations->items.empty())
                selected = &configurations->items[0]; // First config is the default one.

            if (selected == nullptr)
                return;

            const Json* properties = selected->find("Properties");
            if (properties == nullptr || properties->type != Json::Type::Array)
                return;

            for (const Json& property : properties->items)
            {
                const Json* name = property.find("Name");
                const Json* value = property.find("DefaultValue");
                if (name == nullptr || value == nullptr)
                    continue;

                _schema_defaults.members.emplace_back(name->text, *value);
            }
        }
    };

    // The config instance used by RELOADED_MOD_CONFIG_IMPL.
    inline ModConfig& config()
    {
        static ModConfig instance;
        return instance;
    }
}

/*
    Implement this macro in exactly one source file of the mod.
    FN is a function 'void FN()' called on start, with directories known and config loaded.
*/
#define RELOADED_MOD_CONFIG_IMPL(FN)                                                                          \
    extern "C" __declspec(dllexport) void ReloadedStartEx(const wchar_t* mod_directory, const wchar_t* user_config_directory) \
    {                                                                                                         \
        auto& info = reloaded::native_mod_info();                                                             \
        if (mod_directory != nullptr)                                                                         \
            info.mod_directory = mod_directory;                                                               \
        if (user_config_directory != nullptr)                                                                 \
            info.config_directory = user_config_directory;                                                    \
        reloaded::config().load();                                                                            \
        FN();                                                                                                 \
    }

// Same as above but without a start callback, for mods driven by DllMain or other entry points.
#define RELOADED_MOD_CONFIG_IMPL_NO_START()                                                                   \
    extern "C" __declspec(dllexport) void ReloadedStartEx(const wchar_t* mod_directory, const wchar_t* user_config_directory) \
    {                                                                                                         \
        auto& info = reloaded::native_mod_info();                                                             \
        if (mod_directory != nullptr)                                                                         \
            info.mod_directory = mod_directory;                                                               \
        if (user_config_directory != nullptr)                                                                 \
            info.config_directory = user_config_directory;                                                    \
        reloaded::config().load();                                                                            \
    }

#endif // RELOADED_MOD_CONFIG_H
