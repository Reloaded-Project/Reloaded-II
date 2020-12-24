using System;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModV1
    {
        /* Events */
        Action Disposing { get; }

        /* Actions */

        /// <summary>
        /// Represents the entry point of the modification.
        /// </summary>
        /// <param name="loader">Interface which allows for the access of Mod Loader specific functionality.</param>
        void Start(IModLoaderV1 loader);

        /// <summary>
        /// Pauses any active mod behaviour.
        /// </summary>
        /// <remarks>
        ///     This means that you should undo your changes but NOT to the point whereby you cannot undo your undoing.
        ///     Namely you should undo any modifications that you may have done to game code such as direct ASM modifications
        ///     or function hooks.
        ///
        ///     It is also recommended to pause any active threads that are running in the background performing miscellaneous
        ///     functions.
        /// </remarks>
        void Suspend();

        /// <summary>
        /// Resumes any active mod behaviour.
        /// </summary>
        /// <remarks>
        ///     This means that you should redo the changes that were undone in <see cref="Suspend"/>.
        /// </remarks>
        void Resume();

        /// <summary>
        /// Pauses any active mod behaviour (same as suspend), and releases resources (e.g. File Handles, Native Memory)
        /// for the mod to be ejected (unloaded entirely) from the target program.
        /// </summary>
        /// <remarks>
        ///     This is essentially the same as the Suspend function, and in fact it is recommended you call that at the
        ///     start of Unload to make your life easier. The only difference this time is you're supposed to clear
        ///     all traces of your mod's existence.        
        /// </remarks>
        void Unload(); 

        /* Capability */

        /// <summary>
        /// Returns true if <see cref="Unload"/> is supported.
        /// If you return false from this function, the mod loader will never call the <see cref="Unload"/> function.
        /// </summary>
        bool CanUnload();

        /// <summary>
        /// Returns true if <see cref="Suspend"/> is supported.
        /// If you return false from this function, the mod loader will never call the <see cref="Suspend"/> function.
        /// </summary>
        bool CanSuspend();
    }
}
