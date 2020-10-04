using System.Runtime.InteropServices;
using Reloaded.Mod.Shared.PeParser.Interfaces;

namespace Reloaded.Mod.Shared.PeParser
{
	[StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_THUNK_DATA32 : IThunk
    {
        [FieldOffset(0)]
        public uint ForwarderString;

        [FieldOffset(0)]
        public uint Function;

        [FieldOffset(0)]
        public uint Ordinal;

        [FieldOffset(0)]
        public uint AddressOfData;

        /// <inheritdoc />
        public bool IsDummy => AddressOfData == 0;
    }
}
