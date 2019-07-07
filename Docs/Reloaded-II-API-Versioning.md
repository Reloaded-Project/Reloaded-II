# Upgrades & API Versioning

*Note: Updates to the mod loader API are expected to be very infrequent. Frequency will depend on developer requests however the estimate is less than once a year.*

## Backwards Compatibility




## Interface Versioning



## Loader Server Calls

- The Mod Loader Server will expose all available commands for the most recent version of Reloaded-II and its interfaces.

- If a given mod-related command is not supported by a mod, the Server will throw an `ReloadedException` with an accompanying error message. Example:`"Feature unavailable. Interface IModV2 is not supported by this mod."`.