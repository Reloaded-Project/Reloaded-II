using System.Runtime.InteropServices;
using Reloaded.Mod.Shared.PeParser.Interfaces;

namespace Reloaded.Mod.Shared.PeParser
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_THUNK_DATA_NATIVE : IThunk
    {
        [FieldOffset(0)]
        public nuint ForwarderString;

        [FieldOffset(0)]
        public nuint Function;

        [FieldOffset(0)]
        public nuint Ordinal;

        [FieldOffset(0)]
        public nuint AddressOfData;

        /// <inheritdoc />
        public bool IsDummy => AddressOfData == 0;
    }
}
