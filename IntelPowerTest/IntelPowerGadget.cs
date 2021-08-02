using System;
using System.Text;

namespace ConsoleApp5
{
    public class IntelPowerGadget : IDisposable
    {
        const int TargetNodeIndex = 0; 
        private bool _running = false;

        private int _numberOfNodes = 0;
        public int NumberOfNodes { get => _numberOfNodes; }

        private int _maxTemperatureInCelsius;
        public int MaxTemperatureInCelsius { get=> _maxTemperatureInCelsius; }

        //https://www.intel.com/content/www/us/en/support/articles/000055611/processors.html
        private double _thermalDesignPower;
        public double ThermalDesignPower { get => _thermalDesignPower; }

        // https://en.wikipedia.org/wiki/Model-specific_register
        private int _numberOfModelSpecificRegisters; 
        public int NumberOfModelSpecificRegisters { get => _numberOfModelSpecificRegisters; }

        private double _baseFrequency; 
        public double BaseFrequency { get => _baseFrequency; }

        public IntelPowerGadget()
        {
        }

        public void Start()
        {
            if(Native.Initialize() && 
                Native.GetNumberOfNodes(out _numberOfNodes) &&
                _numberOfNodes == 1 && 
                Native.GetMaxTemperature(TargetNodeIndex, out _maxTemperatureInCelsius) && 
                Native.GetThermalDesignPower(TargetNodeIndex, out _thermalDesignPower) && 
                Native.GetNumberOfModelSpecificRegisters(out _numberOfModelSpecificRegisters) && 
                Native.GetBaseFrequency(TargetNodeIndex, out _baseFrequency))
            {
                _running = true;
            }
            else
            {
                throw new IntelPowerGadgetException(); 
            }
        }

        public double ReadWattage()
        {
            double ret = 0.0; 
            if(_running && Native.ReadSample())
            {
                StringBuilder sb = new StringBuilder(1024); 
                for(int msrIndex = 0; msrIndex < _numberOfModelSpecificRegisters;msrIndex++)
                {
                    double[] result = new double[3];
                    int nResults;
                    ModelSpecificRegisterFunction function;

                    if (Native.GetModelSpecificRegisterFunction(msrIndex, out function) && 
                        Native.GetPowerData(TargetNodeIndex, msrIndex, result, out nResults) && 
                        Native.GetModelSpecificRegisterName(msrIndex, sb))
                    {
                        if(sb.ToString() == "IA" && function == ModelSpecificRegisterFunction.ReadPower)
                        {
                            //wprintf(L"%s power (W) = %6.2f\n", szName, data[0]);
                            //wprintf(L"%s energy (J) = %6.2f\n", szName, data[1]);
                            //wprintf(L"%s energy (mWh) = %6.2f\n", szName, data[2]);

                            ret = result[0]; 
                        }
                        //switch(function)
                        //{
                        //    case ModelSpecificRegisterFunction.ReadFrequency:
                        //        //CurrentFrequency = result[0];
                        //        break;
                        //    case ModelSpecificRegisterFunction.ReadPower:
                        //        if(sb.ToString() == "IA")
                        //        {
                        //            ret = result[0]; 
                        //        }
                        //        // also can do energy in mwH for result[2]
                        //        break;
                        //    case ModelSpecificRegisterFunction.ReadTemperature:
                        //        //CurrentTemperatureInCelsius = result[0];
                        //        break;
                        //    case ModelSpecificRegisterFunction.ReadPowerLimit:
                        //        //CurrentPowerLimitInWatts = result[0];
                        //        break; 
                        //}
                    }
                }
            }
            else
            {
                throw new IntelPowerGadgetException(); 
            }

            return ret; 
        }

        public void Stop()
        {
            if(_running && !Native.Shutdown())
            {
                throw new IntelPowerGadgetException(); 
            }
            _running = false; 
        }

        public void Dispose()
        {
            Stop(); 
        }
    }
}
