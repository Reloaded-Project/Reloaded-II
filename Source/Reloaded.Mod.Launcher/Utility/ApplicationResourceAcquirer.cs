using System;
using System.Windows;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// A safer resource acquirer for the current applicationn instance.
    /// </summary>
    public static class ApplicationResourceAcquirer
    {
        /// <summary>
        /// Returns a value from the Application's ResourceDictionary.
        /// </summary>
        /// <typeparam name="TType">Type of value to return.</typeparam>
        /// <param name="key">The key of the resource.</param>
        /// <returns>The specific type or if an error occured, the default value for type.</returns>
        public static TType GetTypeOrDefault<TType>(string key)
        {
            try
            {
                if (Application.Current != null)
                    return (TType) Application.Current.Resources[key];
                else
                    return default(TType);
            }
            catch (Exception)
            {
                return default(TType);
            }
        }

        /// <summary>
        /// Returns a value from the Application's ResourceDictionary, or the alternatively supplied string.
        /// </summary>
        /// <typeparam name="TType">Type of value to return.</typeparam>
        /// <param name="key">The key of the resource.</param>
        /// <param name="alternative">Alternative value.</param>
        /// <returns>The specific type or if an error occured, the default value for type.</returns>
        public static TType GetTypeOrAlternative<TType>(string key, TType alternative)
        {
            try
            {
                if (Application.Current != null)
                    return (TType)Application.Current.Resources[key];
                else
                    return alternative;
            }
            catch (Exception)
            {
                return alternative;
            }
        }

    }
}
