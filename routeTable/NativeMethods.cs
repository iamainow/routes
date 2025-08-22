using System.Runtime.InteropServices;

namespace NativeMethods.Windows;

internal static class NativeMethods
{
    //https://docs.microsoft.com/en-us/windows/win32/iphlp/managing-routing
    //https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-rrasm/5dca234b-bea4-4e67-958e-5459a32a7b71
    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public struct IPForwardTable
    {
        public uint Size;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public MIB_IPFORWARDROW[] Table;
    };

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    public struct MIB_IPFORWARDROW
    {
        internal uint dwForwardDest;
        internal uint dwForwardMask;
        internal uint dwForwardPolicy;
        internal uint dwForwardNextHop;
        internal uint dwForwardIfIndex;
        internal uint dwForwardType;
        internal uint dwForwardProto;
        internal uint dwForwardAge;
        internal uint dwForwardNextHopAS;
        internal uint dwForwardMetric1;
        internal uint dwForwardMetric2;
        internal uint dwForwardMetric3;
        internal uint dwForwardMetric4;
        internal uint dwForwardMetric5;
    };

    public static IPForwardTable ReadIPForwardTable(nint tablePtr)
    {
        var result = Marshal.PtrToStructure<IPForwardTable>(tablePtr);

        MIB_IPFORWARDROW[] table = new MIB_IPFORWARDROW[result.Size];
        nint p = new nint(tablePtr.ToInt64() + Marshal.SizeOf(result.Size));
        for (int i = 0; i < result.Size; ++i)
        {
            table[i] = Marshal.PtrToStructure<MIB_IPFORWARDROW>(p);
            p = new nint(p.ToInt64() + Marshal.SizeOf<MIB_IPFORWARDROW>());
        }
        result.Table = table;

        return result;
    }

    [DllImport("iphlpapi", CharSet = CharSet.Auto)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int GetIpForwardTable(nint pIpForwardTable, ref int pdwSize, bool bOrder);

    [DllImport("iphlpapi", CharSet = CharSet.Auto)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int CreateIpForwardEntry(nint pRoute);

    [DllImport("iphlpapi", CharSet = CharSet.Auto)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int DeleteIpForwardEntry(nint pRoute);

    [DllImport("iphlpapi", CharSet = CharSet.Auto)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int SetIpForwardEntry(nint pRoute);
}