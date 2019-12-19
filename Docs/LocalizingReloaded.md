<div align="center">
	<h1>Reloaded II: Localization</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>¿Cómo está?</strong>
</div>

# Localization

Reloaded-II supports localization, meaning that the entirety of the launcher's user interface can be translated to, or viewed in another language.

### State of Localization

At the current moment in time, the implementation of localization is primitive and only covers the launcher (loader & mods are not localized).  I am not sure how much demand there is for localizations.

The language displayed to the user depends on the user's own system format language.

![](https://i.imgur.com/smucOpM.png)

There is no dropdown to let the user override the current language yet. (I'm lazy ¯\\_(ツ)_/¯).

If you would like a dropdown, or more extensive localization support, consider adding an Issue to the Github Issues list; I have no idea how large the demand is for localization so only bare minimum effort was made.

### How does Localization Work?

The launcher loads a XAML (basically XML) file with the same name as the ISO3166 country code for the user's current locale. A list of localed can be found on various websites and resources [with a simple google search](https://lonewolfonline.net/list-net-culture-country-codes/).

For example, if the user's locale is Portuguese (Brazilian), the file loaded will be `pt-BR.xaml`. Meanwhile if they are in Portugal, the file loaded will be `pt-PT.xaml`.

If no translation exists for a piece of text, the text will default to English (Great Britain), i.e. a string from `en-GB.xaml` will be used.

### Where are the Localization Files 

For a downloaded/release version of Reloaded II, the languages are stored in the `Languages` folder inside the launcher directory.

If you are working with the Source Code of Reloaded II, the localizations are stored in `Source/Reloaded.Mod.Launcher/Languages`.

### Creating Translations

Creating translations is very easy.

If you are creating a new translation, copy the file `en-GB.xaml`\ (default language), and modify the containing text in a text editor.

If you are updating a translation, check `en-GB.xaml`for any missing entries and append them to the end of your translation file.