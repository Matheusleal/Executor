using System.Net.NetworkInformation;

using Hardware.Info;

using MtsCli.Executor.CliLib;

namespace MtsCli.Executor.Commands;

public class ProcessDetailv2 : CommandSync
{
    public override string Name => "Process Detail v2";
    public override string Flag => "process-detail-v2";
    public override string ShortFlag => "pd2";
    public override string Description => "A process detail table (not finished)";
    public override List<Option> Options => [];

    public override CommandOutput Execute(CommandInput input)
    {
        var hardwareInfo = new HardwareInfo();

        try
        {

            //hardwareInfo.RefreshOperatingSystem();
            //hardwareInfo.RefreshMemoryStatus();
            //hardwareInfo.RefreshBatteryList();
            //hardwareInfo.RefreshBIOSList();
            //hardwareInfo.RefreshComputerSystemList();
            //hardwareInfo.RefreshCPUList();
            //hardwareInfo.RefreshDriveList();
            //hardwareInfo.RefreshKeyboardList();
            //hardwareInfo.RefreshMemoryList();
            //hardwareInfo.RefreshMonitorList();
            //hardwareInfo.RefreshMotherboardList();
            //hardwareInfo.RefreshMouseList();
            //hardwareInfo.RefreshNetworkAdapterList();
            //hardwareInfo.RefreshPrinterList();
            //hardwareInfo.RefreshSoundDeviceList();
            //hardwareInfo.RefreshVideoControllerList();

            hardwareInfo.RefreshAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        Console.WriteLine(hardwareInfo.OperatingSystem);

        Console.WriteLine(hardwareInfo.MemoryStatus);

        foreach (var hardware in hardwareInfo.BatteryList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.BiosList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.ComputerSystemList)
            Console.WriteLine(hardware);

        foreach (var cpu in hardwareInfo.CpuList)
        {
            Console.WriteLine(cpu);

            foreach (var cpuCore in cpu.CpuCoreList)
                Console.WriteLine(cpuCore);
        }

        foreach (var drive in hardwareInfo.DriveList)
        {
            Console.WriteLine(drive);

            foreach (var partition in drive.PartitionList)
            {
                Console.WriteLine(partition);

                foreach (var volume in partition.VolumeList)
                    Console.WriteLine(volume);
            }
        }

        foreach (var hardware in hardwareInfo.KeyboardList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.MemoryList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.MonitorList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.MotherboardList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.MouseList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.NetworkAdapterList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.PrinterList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.SoundDeviceList)
            Console.WriteLine(hardware);

        foreach (var hardware in hardwareInfo.VideoControllerList)
            Console.WriteLine(hardware);

        foreach (var address in HardwareInfo.GetLocalIPv4Addresses(NetworkInterfaceType.Ethernet, OperationalStatus.Up))
            Console.WriteLine(address);

        Console.WriteLine();

        foreach (var address in HardwareInfo.GetLocalIPv4Addresses(NetworkInterfaceType.Wireless80211))
            Console.WriteLine(address);

        Console.WriteLine();

        foreach (var address in HardwareInfo.GetLocalIPv4Addresses(OperationalStatus.Up))
            Console.WriteLine(address);

        Console.WriteLine();

        foreach (var address in HardwareInfo.GetLocalIPv4Addresses())
            Console.WriteLine(address);


        return new CommandOutput(0, "", "");
    }
}