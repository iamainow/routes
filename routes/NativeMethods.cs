﻿using System.Runtime.InteropServices;

namespace routes
{
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
            internal uint /*DWORD*/ dwForwardDest;
            internal uint /*DWORD*/ dwForwardMask;
            internal uint /*DWORD*/ dwForwardPolicy;
            internal uint /*DWORD*/ dwForwardNextHop;
            internal uint /*DWORD*/ dwForwardIfIndex;
            internal uint /*DWORD*/ dwForwardType;
            internal uint /*DWORD*/ dwForwardProto;
            internal uint /*DWORD*/ dwForwardAge;
            internal uint /*DWORD*/ dwForwardNextHopAS;
            internal uint /*DWORD*/ dwForwardMetric1;
            internal uint /*DWORD*/ dwForwardMetric2;
            internal uint /*DWORD*/ dwForwardMetric3;
            internal uint /*DWORD*/ dwForwardMetric4;
            internal uint /*DWORD*/ dwForwardMetric5;
        };

        public enum MIB_IPFORWARD_TYPE : uint
        {
            MIB_IPROUTE_TYPE_OTHER = 1,
            MIB_IPROUTE_TYPE_INVALID = 2,
            MIB_IPROUTE_TYPE_DIRECT = 3,
            MIB_IPROUTE_TYPE_INDIRECT = 4
        }



        public enum MIB_IPFORWARD_PROTO : uint
        {
            MIB_IPPROTO_OTHER = 1,
            MIB_IPPROTO_LOCAL = 2,
            MIB_IPPROTO_NETMGMT = 3,
            MIB_IPPROTO_ICMP = 4,
            MIB_IPPROTO_EGP = 5,
            MIB_IPPROTO_GGP = 6,
            MIB_IPPROTO_HELLO = 7,
            MIB_IPPROTO_RIP = 8,
            MIB_IPPROTO_IS_IS = 9,
            MIB_IPPROTO_ES_IS = 10,
            MIB_IPPROTO_CISCO = 11,
            MIB_IPPROTO_BBN = 12,
            MIB_IPPROTO_OSPF = 13,
            MIB_IPPROTO_BGP = 14,
            MIB_IPPROTO_NT_AUTOSTATIC = 10002,
            MIB_IPPROTO_NT_STATIC = 10006,
            MIB_IPPROTO_NT_STATIC_NON_DOD = 10007
        }


        public static IPForwardTable ReadIPForwardTable(nint tablePtr)
        {
            var result = (IPForwardTable)Marshal.PtrToStructure(tablePtr, typeof(IPForwardTable));

            MIB_IPFORWARDROW[] table = new MIB_IPFORWARDROW[result.Size];
            nint p = new nint(tablePtr.ToInt64() + Marshal.SizeOf(result.Size));
            for (int i = 0; i < result.Size; ++i)
            {
                table[i] = (MIB_IPFORWARDROW)Marshal.PtrToStructure(p, typeof(MIB_IPFORWARDROW));
                p = new nint(p.ToInt64() + Marshal.SizeOf(typeof(MIB_IPFORWARDROW)));
            }
            result.Table = table;

            return result;
        }


        [DllImport("iphlpapi", CharSet = CharSet.Auto)]
        public extern static int GetIpForwardTable(nint /*PMIB_IPFORWARDTABLE*/ pIpForwardTable, ref int /*PULONG*/ pdwSize, bool bOrder);

        [DllImport("iphlpapi", CharSet = CharSet.Auto)]
        //public extern static int CreateIpForwardEntry(ref /*PMIB_IPFORWARDROW*/ Ip4RouteTable.PMIB_IPFORWARDROW pRoute);  Can do by reference or by Pointer
        public extern static int CreateIpForwardEntry(nint /*PMIB_IPFORWARDROW*/ pRoute);

        [DllImport("iphlpapi", CharSet = CharSet.Auto)]
        public extern static int DeleteIpForwardEntry(nint /*PMIB_IPFORWARDROW*/ pRoute);

        [DllImport("iphlpapi", CharSet = CharSet.Auto)]
        public extern static int SetIpForwardEntry(nint /*PMIB_IPFORWARDROW*/ pRoute);

    }
}