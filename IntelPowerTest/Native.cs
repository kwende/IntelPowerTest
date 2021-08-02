using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp5
{
    public enum ModelSpecificRegisterFunction
    { 
        Unknown = -1,
        ReadFrequency = 0,
        ReadPower = 1,
        ReadTemperature = 2,
        ReadPowerLimit = 3,
    }

    public static class Native
    {

        [DllImport("Externals/EnergyLib64.dll", EntryPoint ="IntelEnergyLibInitialize")]
        public static extern bool Initialize();

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "IntelEnergyLibShutdown")]
        public static extern bool Shutdown();

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetNumNodes")]
        public static extern bool GetNumberOfNodes(out int count);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetMaxTemperature")]
        public static extern bool GetMaxTemperature(int node, out int degreesInC);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetTDP")]
        public static extern bool GetThermalDesignPower(int node, out double designPower);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetNumMsrs")]
        public static extern bool GetNumberOfModelSpecificRegisters(out int msrCount);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetBaseFrequency")]
        public static extern bool GetBaseFrequency(int node, out double baseFrequency);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "ReadSample")]
        public static extern bool ReadSample();

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetPowerData")]
        public static extern bool GetPowerData(int iNode, int iMSR, double[] result, out int nResult);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetMsrName", CharSet = CharSet.Unicode)]
        public static extern bool GetModelSpecificRegisterName(int msr, StringBuilder szName);

        [DllImport("Externals/EnergyLib64.dll", EntryPoint = "GetMsrFunc")]
        public static extern bool GetModelSpecificRegisterFunction(int msr, out ModelSpecificRegisterFunction function); 
    }
}
