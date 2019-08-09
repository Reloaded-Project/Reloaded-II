using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Loader.IO
{
    public static class Utility
    {
        /// <summary>
        /// Finds all properties which have a null values and gives them the default value for the type.
        /// </summary>
        public static void SetNullPropertyValues(object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(obj, null);
                if (propertyValue == null)
                {
                    property.SetValue(obj,
                        property.PropertyType.IsArray
                            ? Activator.CreateInstance(property.PropertyType, 0)
                            : Activator.CreateInstance(property.PropertyType));
                }
            }
        }
    }
}
