namespace Reloaded.Mod.Loader.IO.Config
{
    /// <summary>
    /// Interface used to categorize configuration files.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Sets null values to default where appropriate.
        /// </summary>
        void SetNullValues();
    }
}
