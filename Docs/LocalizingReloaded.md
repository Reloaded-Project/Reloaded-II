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

### How does Localization Work?

The launcher loads a XAML (basically XML) file present in the `Languages` folder of the launcher. In-launcher, the dropdown is automatically populated with the list of every file available in that folder, by name.

If no translation exists for a piece of text, the text will default to English (Great Britain), i.e. a string from `en-GB.xaml` will be used.

### Where are the Localization Files 

For a downloaded/release version of Reloaded II, the languages are stored in the `Languages` folder inside the launcher directory.

If you are working with the Source Code of Reloaded II, the localizations are stored in `Source/Reloaded.Mod.Launcher/Languages`.

### Creating Translations

Creating translations is very easy.

If you are creating a new translation, copy the file `en-GB.xaml` (default language), and modify the containing text in a text editor.

If you are updating a translation, check `en-GB.xaml`for any missing entries and append them to the end of your translation file. 

As Reloaded gets updated, new text is always added to the bottom of  `en-GB.xaml`. You can determine if a translation is complete if the last text entry for any language and `en-GB.xaml` is the same.