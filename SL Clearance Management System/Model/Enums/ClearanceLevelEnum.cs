using System.Diagnostics.CodeAnalysis;

namespace SLCMS.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ClearanceLevelEnum : byte {
        None = 0,
        A = 1,
        AB = 2,
        ABC = 3,
        ABCD = 4,
        ABCDW = 5
    }
}
