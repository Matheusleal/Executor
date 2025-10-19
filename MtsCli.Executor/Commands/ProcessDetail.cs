using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using MtsCli.Executor.CliLib;
using MtsCli.Executor.Helpers;

namespace MtsCli.Executor.Commands;

public class ProcessDetail : CommandSync
{
    public override string Name => "Process Detail";
    public override string Flag => "process-detail";
    public override string ShortFlag => "pd";
    public override string Description => "A process detail table (not finished)";
    public override List<Option> Options => [];

    public override CommandOutput Execute(CommandInput input)
    {
        var processes = ProcessHelper.GetAllProcesses();

        var tcpPorts = NetworkHelper.GetTcpConnections();
        var udpPorts = NetworkHelper.GetUdpConnections();

        List<string[]> table = [];

        table.Add(new[] { "Pid", "Name", "Path", "TCP", "UDP" });

        foreach (var p in processes)
        {
            if (p.Name == "Unknown")
                continue;

            var sbTcp = new StringBuilder();
            var sbUdp = new StringBuilder();

            if (tcpPorts.TryGetValue(p.Pid, out var tcpList))
                sbTcp.AppendJoin(';', tcpList);

            if (udpPorts.TryGetValue(p.Pid, out var udpList))
                sbUdp.AppendJoin(';', udpList);

            table.Add(new[] { p.Pid.ToString(), p.Name, p.Path, sbTcp.ToString(), sbUdp.ToString() });
        }

        Printer.PrintTable(table);

        return new CommandOutput(0, "", "");
    }
}


class ProcessInfo
{
    public int Pid { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
}

static class ProcessHelper
{
    private const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    [DllImport("psapi.dll")]
    private static extern bool EnumProcesses([Out] int[] lpidProcess, int cb, [MarshalAs(UnmanagedType.I4)] out int lpcbNeeded);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref uint lpdwSize);

    public static List<ProcessInfo> GetAllProcesses()
    {
        int[] processIds = new int[1024];
        EnumProcesses(processIds, processIds.Length * sizeof(int), out int bytesReturned);

        int count = bytesReturned / sizeof(int);
        var list = new List<ProcessInfo>(count);

        for (int i = 0; i < count; i++)
        {
            int pid = processIds[i];
            if (pid == 0) continue;

            string name = "";
            string path = "";

            try
            {
                var handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
                if (handle != IntPtr.Zero)
                {
                    var sb = new StringBuilder(1024);
                    uint size = (uint)sb.Capacity;
                    if (QueryFullProcessImageName(handle, 0, sb, ref size) != 0)
                    {
                        path = sb.ToString();
                        name = Path.GetFileName(path);
                    }
                    CloseHandle(handle);
                }
            }
            catch { }

            list.Add(new ProcessInfo
            {
                Pid = pid,
                Name = string.IsNullOrEmpty(name) ? "Unknown" : name,
                Path = path
            });
        }

        return list;
    }
}

static class NetworkHelper
{
    private const int AF_INET = 2; // IPv4

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool sort, int ipVersion, UdpTableClass tblClass, uint reserved = 0);

    public static Dictionary<int, List<int>> GetTcpConnections()
    {
        var result = new Dictionary<int, List<int>>();
        int bufferSize = 0;
        GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            if (GetExtendedTcpTable(buffer, ref bufferSize, true, AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
            {
                var table = Marshal.PtrToStructure<TcpTable>(buffer);
                IntPtr rowPtr = (IntPtr)((long)buffer + Marshal.SizeOf(table.dwNumEntries));

                for (int i = 0; i < table.dwNumEntries; i++)
                {
                    var row = Marshal.PtrToStructure<TcpRowOwnerPid>(rowPtr);
                    int port = IPAddress.NetworkToHostOrder((short)row.dwLocalPort);
                    if (!result.ContainsKey((int)row.dwOwningPid))
                        result[(int)row.dwOwningPid] = new List<int>();
                    result[(int)row.dwOwningPid].Add(port);

                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf<TcpRowOwnerPid>());
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return result;
    }

    public static Dictionary<int, List<int>> GetUdpConnections()
    {
        var result = new Dictionary<int, List<int>>();
        int bufferSize = 0;
        GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID, 0);
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            if (GetExtendedUdpTable(buffer, ref bufferSize, true, AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID, 0) == 0)
            {
                var table = Marshal.PtrToStructure<UdpTable>(buffer);
                IntPtr rowPtr = (IntPtr)((long)buffer + Marshal.SizeOf(table.dwNumEntries));

                for (int i = 0; i < table.dwNumEntries; i++)
                {
                    var row = Marshal.PtrToStructure<UdpRowOwnerPid>(rowPtr);
                    int port = IPAddress.NetworkToHostOrder((short)row.dwLocalPort);
                    if (!result.ContainsKey((int)row.dwOwningPid))
                        result[(int)row.dwOwningPid] = new List<int>();
                    result[(int)row.dwOwningPid].Add(port);

                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf<UdpRowOwnerPid>());
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return result;
    }

    #region Structs & Enums

    private enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    private enum UdpTableClass
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TcpTable
    {
        public uint dwNumEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TcpRowOwnerPid
    {
        public uint state;
        public uint dwLocalAddr;
        public uint dwLocalPort;
        public uint dwRemoteAddr;
        public uint dwRemotePort;
        public uint dwOwningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UdpTable
    {
        public uint dwNumEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UdpRowOwnerPid
    {
        public uint dwLocalAddr;
        public uint dwLocalPort;
        public uint dwOwningPid;
    }

    #endregion
}