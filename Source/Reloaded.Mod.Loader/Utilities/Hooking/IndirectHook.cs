using System;
using System.Runtime.InteropServices;
using Reloaded.Memory.Sources;

namespace Reloaded.Mod.Loader.Utilities.Hooking
{
    /// <summary>
    /// A very basic hook that works on the basis of replacing an existing function pointer
    /// to a function with a new pointer. Used for hooking basic Windows APIs.
    /// </summary>
    public unsafe class IndirectHook<TFunction> where TFunction : Delegate
    {
        private static Memory.Sources.Memory Memory = Reloaded.Memory.Sources.Memory.CurrentProcess;

        /// <summary>
        /// Returns true if the hook is enabled and currently functional, else false.
        /// </summary>
        public bool IsHookEnabled { get; private set; }

        /// <summary>
        /// Returns true if the hook has been activated.
        /// The hook may only be activated once.
        /// </summary>
        public bool IsHookActivated { get; private set; }

        /// <summary>
        /// Allows you to call the original function.
        /// </summary>
        public TFunction OriginalFunction { get; }

        /// <summary>
        /// Allows you to call the new function.
        /// </summary>
        public TFunction NewFunction { get; }

        /// <summary>
        /// Address of the original function in memory at the time of the creation of the hook.
        /// </summary>
        public nint OriginalFunctionAddress { get; }

        /// <summary>
        /// Address of the new function in memory.
        /// </summary>
        public nint NewFunctionAddress { get; }

        private nint* _addressOfOriginalFunctionPointer { get; }
        
        public IndirectHook(nint* addressOfOriginalFunctionPointer, TFunction newFunction)
        {
            _addressOfOriginalFunctionPointer = addressOfOriginalFunctionPointer;
            OriginalFunctionAddress = *_addressOfOriginalFunctionPointer;
            NewFunctionAddress = Marshal.GetFunctionPointerForDelegate(newFunction);

            NewFunction      = newFunction;
            OriginalFunction = Marshal.GetDelegateForFunctionPointer<TFunction>(OriginalFunctionAddress);
        }

        public IndirectHook<TFunction> Activate()
        {
            IsHookActivated = true;
            Enable();
            return this;
        }

        public void Disable()
        {
            Memory.SafeWrite((IntPtr)_addressOfOriginalFunctionPointer, OriginalFunctionAddress);
            IsHookEnabled = false;
        }

        public void Enable()
        {
            Memory.SafeWrite((IntPtr)_addressOfOriginalFunctionPointer, NewFunctionAddress);
            IsHookEnabled = true;
        }
    }
}
