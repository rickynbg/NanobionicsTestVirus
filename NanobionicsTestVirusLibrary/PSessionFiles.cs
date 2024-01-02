using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NanobionicsTestVirusLibrary
{
    public enum MeasurementType
    {
        IDC = 0,
        VOLT = 1,
        TIME = 2,
        FREQUENCY = 3,
        ZRE = 4,
        ZIM = 5,
        Z = 6,
        PHASE = 7,
        IAC = 8,
        NPOINTSAC = 9,
        REALTINTAC = 10,
        YMEAN = 11,
        DEBUGTEXT = 12,
        Y = 13,
        YRE = 14,
        YIM = 15
    }

    public class PSessionFiles
    {
        public Measurement BaseValues;
        public Measurement MeasValues;

        public PSessionFiles(string psessionBase, string psessionMeas)
        {
            JObject baseDSObj = JObject.Parse(psessionBase);
            JObject MeasDSObj = JObject.Parse(psessionMeas);

            var baseObj = baseDSObj["values"];
            var MeasObj = MeasDSObj["values"];

            BaseValues = new Measurement();
            MeasValues = new Measurement();

            int BaseCountountValues = baseObj[(int)MeasurementType.FREQUENCY]["datavalues"].Count();
            int MeasCountountValues = MeasObj[(int)MeasurementType.FREQUENCY]["datavalues"].Count();

            for(int i = 0; i < BaseCountountValues; i++)
            {

                double _F = double.Parse(baseObj[(int)MeasurementType.FREQUENCY]["datavalues"][i]["v"].ToString());
                double _Z = double.Parse(baseObj[(int)MeasurementType.Z]["datavalues"][i]["v"].ToString());
                double _Zre = double.Parse(baseObj[(int)MeasurementType.ZRE]["datavalues"][i]["v"].ToString());
                double _Zim = double.Parse(baseObj[(int)MeasurementType.ZIM]["datavalues"][i]["v"].ToString());
                double _Phase = double.Parse(baseObj[(int)MeasurementType.PHASE]["datavalues"][i]["v"].ToString());

                double _Cre = _Zim / (2 * Math.PI * _F * Math.Pow(_Z, 2));
                double _Cim = _Zre / (2 * Math.PI * _F * Math.Pow(_Z, 2));

                BaseValues.Freq.Add(_F);
                BaseValues.Z.Add(_Z);
                BaseValues.Zre.Add(_Zre);
                BaseValues.Zim.Add(_Zim);
                BaseValues.Phase.Add(_Phase);
                BaseValues.Cre.Add(_Cre);
                BaseValues.Cim.Add(_Cim);
            }

            for (int i = 0; i < MeasCountountValues; i++)
            {

                double _F = double.Parse(MeasObj[(int)MeasurementType.FREQUENCY]["datavalues"][i]["v"].ToString());
                double _Z = double.Parse(MeasObj[(int)MeasurementType.Z]["datavalues"][i]["v"].ToString());
                double _Zre = double.Parse(MeasObj[(int)MeasurementType.ZRE]["datavalues"][i]["v"].ToString());
                double _Zim = double.Parse(MeasObj[(int)MeasurementType.ZIM]["datavalues"][i]["v"].ToString());
                double _Phase = double.Parse(MeasObj[(int)MeasurementType.PHASE]["datavalues"][i]["v"].ToString());

                double _Cre = _Zim / (2 * Math.PI * _F * Math.Pow(_Z, 2));
                double _Cim = _Zre / (2 * Math.PI * _F * Math.Pow(_Z, 2));

                MeasValues.Freq.Add(_F);
                MeasValues.Z.Add(_Z);
                MeasValues.Zre.Add(_Zre);
                MeasValues.Zim.Add(_Zim);
                MeasValues.Phase.Add(_Phase);
                MeasValues.Cre.Add(_Cre);
                MeasValues.Cim.Add(_Cim);
            }

        }

        public CalResult CalculateSValue(bool filter = true, double infFreq = 1, double supFreq = 100)
        {
            double[] baseCimArrayFilter;
            double[] measCimArrayFilter;

            double[] basePhaArrayFilter;
            double[] measPhaArrayFilter;

            double before = 0;
            bool foundMax = false;
            bool foundMin = false;

            double peakBaseMax = 0d;
            int peakBaseMaxIndex = 0;
            double peakBaseMin = 0d;
            int peakBaseMinIndex = 0;

            double peakMeasMax = 0d;
            int peakMeasMaxIndex = 0;
            double peakMeasMin = 0d;
            int peakMeasMinIndex = 0;

            double valleyBaseMin = 0d;
            int valleyBaseMinIndex = 0;
            double valleyMeasMin = 0d;
            int valleyMeasMinIndex = 0;

            if (filter)
            {
                baseCimArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(BaseValues.Cim.ToArray());
                measCimArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(MeasValues.Cim.ToArray());

                basePhaArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(BaseValues.Phase.ToArray());
                measPhaArrayFilter = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(MeasValues.Phase.ToArray());
            }
            else
            {
                baseCimArrayFilter = BaseValues.Cim.ToArray();
                measCimArrayFilter = MeasValues.Cim.ToArray();

                basePhaArrayFilter = BaseValues.Phase.ToArray();
                measPhaArrayFilter = MeasValues.Phase.ToArray();
            }

            for (int i = 0; i < baseCimArrayFilter.Length; i++)
            {
                if (BaseValues.Freq.ElementAt(i) <= supFreq && infFreq <= BaseValues.Freq.ElementAt(i))
                {
                    if (baseCimArrayFilter[i] < before && !foundMax)
                    {
                        peakBaseMax = before;
                        peakBaseMaxIndex = i;
                        foundMax = true;
                    }
                    if (baseCimArrayFilter[i] > before && foundMax && !foundMin)
                    {
                        peakBaseMin = before;
                        peakBaseMinIndex = i;
                        foundMin = true;
                    }
                    before = baseCimArrayFilter[i];
                }
            }

            before = 0;
            foundMax = false;
            foundMin = false;
            for (int i = 0; i < measCimArrayFilter.Length; i++)
            {
                if (MeasValues.Freq.ElementAt(i) <= supFreq && infFreq <= MeasValues.Freq.ElementAt(i))
                {
                    if (measCimArrayFilter[i] < before && !foundMax)
                    {
                        peakBaseMax = before;
                        peakMeasMaxIndex = i;
                        foundMax = true;
                    }
                    if (measCimArrayFilter[i] > before && foundMax && !foundMin)
                    {
                        peakMeasMin = before;
                        peakMeasMinIndex = i;
                        foundMin = true;
                    }
                    before = measCimArrayFilter[i];
                }
            }

            before = 180;
            foundMin = false;
            for (int i =0; i < basePhaArrayFilter.Length - 1; i++)
            {
                if (BaseValues.Freq.ElementAt(i) <= supFreq && infFreq <= BaseValues.Freq.ElementAt(i))
                {
                    if (basePhaArrayFilter[i] > before && !foundMin)
                    {
                        valleyBaseMin = before;
                        valleyBaseMinIndex = i;
                        foundMin = true;
                    }
                    before = basePhaArrayFilter[i];
                }
            }

            before = 180;
            foundMin = false;
            for (int i = 0; i < measPhaArrayFilter.Length - 1; i++)
            {
                if (MeasValues.Freq.ElementAt(i) <= supFreq && infFreq <= MeasValues.Freq.ElementAt(i))
                {
                    if (measPhaArrayFilter[i] > before && !foundMin)
                    {
                        valleyMeasMin = before;
                        valleyMeasMinIndex = i;
                        foundMin = true;
                    }
                    before = measPhaArrayFilter[i];
                }
            }

            CalResult ret = new();

            if(peakBaseMax > 0 && peakMeasMax > 0)
            {
                ret.FreqBase = MeasValues.Freq.ElementAt(peakBaseMaxIndex);
                ret.FreqMeas = MeasValues.Freq.ElementAt(peakMeasMaxIndex);
                ret.FreqBasePos = peakBaseMaxIndex;
                ret.FreqMeasPos = peakMeasMaxIndex;
                ret.SValuePeakMax = Math.Abs((1.0 / peakBaseMax - 1.0 / peakMeasMax) * peakBaseMax * 100.0);
                ret.UsePhase = false;
                ret.Error = false;
            }
            
            if (peakBaseMin > 0 && peakMeasMin > 0)
            {
                ret.FreqBase = MeasValues.Freq.ElementAt(peakBaseMinIndex);
                ret.FreqMeas = MeasValues.Freq.ElementAt(peakMeasMinIndex);
                ret.FreqBasePos = peakBaseMinIndex;
                ret.FreqMeasPos = peakMeasMinIndex;
                ret.SValuePeakMin = Math.Abs((1.0 / MeasValues.Freq.ElementAt(peakBaseMinIndex) - 1.0 / MeasValues.Freq.ElementAt(peakMeasMinIndex)) * MeasValues.Freq.ElementAt(peakBaseMinIndex) * 100.0);
                ret.UsePhase = false;
                ret.Error = false;
            }
            
            if (valleyBaseMin > 0 && valleyMeasMin > 0)
            {
                ret.FreqBase = MeasValues.Freq.ElementAt(valleyBaseMinIndex);
                ret.FreqMeas = MeasValues.Freq.ElementAt(valleyMeasMinIndex);
                ret.FreqBasePos = valleyBaseMinIndex;
                ret.FreqMeasPos = valleyMeasMinIndex;
                ret.SValuePhaseMin = Math.Abs((1.0 / valleyMeasMin - 1.0 / valleyBaseMin) * valleyBaseMin * 100.0);
                ret.UsePhase = true;
                ret.Error = false;
            }
            
            if(ret.SValuePeakMax == 0d && ret.SValuePeakMin == 0d && ret.SValuePhaseMin == 0d)
            {
                ret.Error = true;
            }

            return ret;
        }

        public string GetMaxMinValues(bool basem = true)
        {
            Measurement meas = new();
            if (basem)
                meas = BaseValues;
            else
                meas = MeasValues;

            var cfreq = new ObservableCollection<double>(meas.Freq.Where(x => x > 5d));

            int ifreq = cfreq.Count - 1;

            var slice_mmx = new ObservableCollection<double>(meas.Cim.Where(x => meas.Cim.IndexOf(x) < ifreq));

            double mmx = slice_mmx.Max();
            int immx = meas.Cim.IndexOf(mmx);

            return String.Format($"Value {mmx * 1e6:0.####} uF; Index {immx}");
        }

        public ResultList GetResultList(double inf, double sup, bool useFilter = false)
        {
            ResultList ret = new();

            int iCutFreqMinIndexBase = new ObservableCollection<double>(BaseValues.Freq.Where(x => x > inf)).Count - 1;
            int iCutFreqMaxIndexBase = BaseValues.Freq.Count - new ObservableCollection<double>(BaseValues.Freq.Where(x => x < sup)).Count;

            int iCutFreqMinIndexMeas = new ObservableCollection<double>(MeasValues.Freq.Where(x => x > inf)).Count - 1;
            int iCutFreqMaxIndexMeas = MeasValues.Freq.Count - new ObservableCollection<double>(MeasValues.Freq.Where(x => x < sup)).Count;

            var CimBaseValues = BaseValues.Cim.ToArray();
            var CimMeasValues = MeasValues.Cim.ToArray();

            var PhaseBaseValues = BaseValues.Phase.ToArray();
            var PhaseMeasValues = MeasValues.Phase.ToArray();

            if (useFilter)
            {
                CimBaseValues = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(CimBaseValues);
                CimMeasValues = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(CimMeasValues);

                PhaseBaseValues = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(PhaseBaseValues);
                PhaseMeasValues = new SavitzkyGolayFilter(Constants.SIDEPOINTS, Constants.POLYNOMIALORDER).Process(PhaseMeasValues);
            }

            var vSliceCimBaseMax = new ObservableCollection<double>(CimBaseValues
                .Where(x => Array.IndexOf(CimBaseValues,x) < iCutFreqMinIndexBase && Array.IndexOf(CimBaseValues, x) > iCutFreqMaxIndexBase));
            var vSliceCimMeasMax = new ObservableCollection<double>(CimMeasValues
                .Where(x => Array.IndexOf(CimMeasValues, x) < iCutFreqMinIndexMeas && Array.IndexOf(CimMeasValues, x) > iCutFreqMaxIndexMeas));

            var vSlicePhaBaseMax = new ObservableCollection<double>(PhaseBaseValues
                .Where(x => Array.IndexOf(PhaseBaseValues, x) < iCutFreqMinIndexBase && Array.IndexOf(PhaseBaseValues, x) > iCutFreqMaxIndexBase));
            var vSlicePhaMeasMax = new ObservableCollection<double>(PhaseMeasValues
                .Where(x => Array.IndexOf(PhaseMeasValues, x) < iCutFreqMinIndexMeas && Array.IndexOf(PhaseMeasValues, x) > iCutFreqMaxIndexMeas));

            ret.DBCimMax = vSliceCimBaseMax.Max();
            ret.IBCimMax = Array.IndexOf(CimBaseValues, ret.DBCimMax);

            ret.IBFreqMax = ret.IBCimMax;
            ret.DBFreqMax = BaseValues.Freq.ElementAt(ret.IBFreqMax);

            var vSliceCimBaseMin = new ObservableCollection<double>(vSliceCimBaseMax.Where(x => vSliceCimBaseMax.IndexOf(x) > ret.IBCimMax));
            if (vSliceCimBaseMin.Count == 0)
            {
                vSliceCimBaseMin = vSliceCimBaseMax;
            }

            ret.DBCimMin = vSliceCimBaseMin.Min();
            ret.IBCimMin = Array.IndexOf(CimBaseValues, ret.DBCimMin);

            ret.IBFreqMin = ret.IBCimMin;
            ret.DBFreqMin = BaseValues.Freq.ElementAt(ret.IBFreqMin);

            ret.DBPhaMax = vSlicePhaBaseMax.Max();
            ret.IBPhaMax = Array.IndexOf(PhaseBaseValues, ret.DBPhaMax);

            ret.IBPhaFreq = ret.IBPhaMax;
            ret.DBPhaFreq = BaseValues.Freq.ElementAt(ret.IBPhaFreq);

            /////
            /////
            ret.DMCimMax = vSliceCimMeasMax.Max();
            ret.IMCimMax = Array.IndexOf(CimMeasValues, ret.DMCimMax);

            ret.IMFreqMax = ret.IMCimMax;
            ret.DMFreqMax = MeasValues.Freq.ElementAt(ret.IMFreqMax);

            var vSliceCimMeasMin = new ObservableCollection<double>(vSliceCimMeasMax.Where(x => vSliceCimMeasMax.IndexOf(x) > ret.IMCimMax));
            if (vSliceCimMeasMin.Count == 0)
            {
                vSliceCimMeasMin = vSliceCimMeasMax;
            }

            ret.DMCimMin = vSliceCimMeasMin.Min();
            ret.IMCimMin = Array.IndexOf(CimMeasValues, ret.DMCimMin);

            ret.IMFreqMin = ret.IMCimMin;
            ret.DMFreqMin = MeasValues.Freq.ElementAt(ret.IMFreqMin);           

            ret.DMPhaMax = vSlicePhaMeasMax.Max();
            ret.IMPhaMax = Array.IndexOf(PhaseMeasValues, ret.DMPhaMax);

            ret.IMPhaFreq = ret.IMPhaMax;
            ret.DMPhaFreq = MeasValues.Freq.ElementAt(ret.IMPhaFreq);

            return ret;
        }
    }

    public class Measurement
    {
        public string Name { get; set; }
        public ObservableCollection<double> Freq { get; set; }
        public ObservableCollection<double> Z { get; set; }
        public ObservableCollection<double> Zre { get; set; }
        public ObservableCollection<double> Zim { get; set; }
        public ObservableCollection<double> Cre { get; set; }
        public ObservableCollection<double> Cim { get; set; }
        public ObservableCollection<double> Phase { get; set; }

        public Measurement()
        {
            Freq = new ObservableCollection<double>();
            Z = new ObservableCollection<double>();
            Zre = new ObservableCollection<double>();
            Zim = new ObservableCollection<double>();
            Cre = new ObservableCollection<double>();
            Cim = new ObservableCollection<double>();
            Phase = new ObservableCollection<double>();
        }
    }

    public class CalResult
    {
        public double FreqBase { get; set; }
        public int FreqBasePos { get; set; }
        public double FreqMeas { get; set; }
        public int FreqMeasPos { get; set; }
        public double SValuePeakMax { get; set; }
        public double SValuePeakMin { get; set; }
        public double SValuePhaseMin { get; set; }
        public bool UsePhase { get; set; }
        public bool Error { get; set; }
    }

    public class ResultList
    {
        public double DBFreqMin { get; set; }
        public int IBFreqMin { get; set; }
        public double DBFreqMax { get; set; }
        public int IBFreqMax { get; set; }
        public double DBCimMin { get; set; }
        public int IBCimMin { get; set; }
        public double DBCimMax { get; set; }
        public int IBCimMax { get; set; }
        public double DBPhaFreq { get; set; }
        public int IBPhaFreq { get; set; }
        public double DBPhaMax { get; set; }
        public int IBPhaMax { get; set; }

        public double DMFreqMin { get; set; }
        public int IMFreqMin { get; set; }
        public double DMFreqMax { get; set; }
        public int IMFreqMax { get; set; }
        public double DMCimMin { get; set; }
        public int IMCimMin { get; set; }
        public double DMCimMax { get; set; }
        public int IMCimMax { get; set; }
        public double DMPhaFreq { get; set; }
        public int IMPhaFreq { get; set; }
        public double DMPhaMax { get; set; }
        public int IMPhaMax { get; set; }
    }
}
