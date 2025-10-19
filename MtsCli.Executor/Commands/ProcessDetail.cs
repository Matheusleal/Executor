using System.Diagnostics;
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

        table.Add(new[] { "Pid", "Name", "Path", "TCP", "UDP", "CPU (%)", "RAM (MB)" });

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

            table.Add(new[] { p.Pid.ToString(), p.Name, p.Path, sbTcp.ToString(), sbUdp.ToString(), p.CpuUsage.ToString("F2"), p.RamUsage.ToString("F2") });
        }

        MtsCli.Executor.Helpers.Printer.PrintTable(table);

        return new CommandOutput(0, "", "");
    }
}


class ProcessInfo
{
    public int Pid { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public double CpuUsage { get; set; }
    public double RamUsage { get; set; }
}

static class ProcessHelper
{
    public static List<ProcessInfo> GetAllProcesses()
    {
        var list = new List<ProcessInfo>();

        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            try
            {
                double cpuUsage = 0;
                double ramUsage = 0;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
                    cpuUsage = counter.NextValue();
                    ramUsage = process.WorkingSet64 / (1024.0 * 1024.0);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // https://stackoverflow.com/questions/23367857/accurate-calculation-of-cpu-usage-given-in-percentage-in-linux
                    var stat = System.IO.File.ReadAllLines("/proc/stat")[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var totalTime = ulong.Parse(stat[1]) + ulong.Parse(stat[2]) + ulong.Parse(stat[3]) + ulong.Parse(stat[4]) + ulong.Parse(stat[5]) + ulong.Parse(stat[6]) + ulong.Parse(stat[7]);

                    var pstat = System.IO.File.ReadAllLines($"/proc/{process.Id}/stat")[0].Split(' ');
                    var utime = ulong.Parse(pstat[13]);
                    var stime = ulong.Parse(pstat[14]);
                    var cutime = ulong.Parse(pstat[15]);
                    var cstime = ulong.Parse(pstat[16]);

                    var totalProcessTime = utime + stime + cutime + cstime;

                    var seconds = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds;

                    cpuUsage = 100 * (totalProcessTime / (double)totalTime);
                    ramUsage = process.WorkingSet64 / (1024.0 * 1024.0);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var processInfo = new ProcessStartInfo("sysctl", $"-p {process.Id}");
                    processInfo.RedirectStandardOutput = true;
                    var p = Process.Start(processInfo);
                    p.WaitForExit();
                    var output = p.StandardOutput.ReadToEnd();

                    var match = System.Text.RegularExpressions.Regex.Match(output, @"cpu\s+([0-9\.]+)");
                    if (match.Success)
                    {
                        cpuUsage = double.Parse(match.Groups[1].Value);
                    }

                    ramUsage = process.WorkingSet64 / (1024.0 * 1024.0);
                }
                else
                {
                    throw new NotImplementedException();
                }

                list.Add(new ProcessInfo
                {
                    Pid = process.Id,
                    Name = process.ProcessName,
                    Path = process.MainModule?.FileName ?? "",
                    CpuUsage = cpuUsage,
                    RamUsage = ramUsage
                });
            }
            catch { }
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