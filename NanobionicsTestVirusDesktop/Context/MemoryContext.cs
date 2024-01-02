using NanobionicsTestVirusDesktop.Models;
using System;
using System.Collections.ObjectModel;

namespace NanobionicsTestVirusDesktop.Context
{
    class MemoryContext
    {
        public ObservableCollection<FileMeasure> FileMeasures { get; set; }
        public ObservableCollection<DataMeasure> DataMeasures1 { get; set; }
        public ObservableCollection<DataMeasure> DataMeasures2 { get; set; }

        public MemoryContext()
        {
            FileMeasures = new()
            {
                new FileMeasure { IdFileMeasure = 1, Name = "Measure 1", ContentValue = "Measures list 1 not implement", DataMeasures = DataMeasures1, LoadedAt = DateTime.Now },
                new FileMeasure { IdFileMeasure = 2, Name = "Measure 2", ContentValue = "Measures list 2 not implement", DataMeasures = DataMeasures2, LoadedAt = DateTime.Now }
            };

            DataMeasures1 = new()
            {
                new DataMeasure
                {
                    IdFileMeasure = 1,
                    Name = "Base 1",
                    Values = "{f = 10, v = 5.5}",
                    Type = 5, FrequencyValue = 5,
                    FrequencyPos = 10,
                    IdDataMeasure = 1
                },
                new DataMeasure
                {
                    IdFileMeasure = 2,
                    Name = "Meas 1",
                    Values = "{f = 10, v = 5.5}",
                    Type = 5,
                    FrequencyValue = 5,
                    FrequencyPos = 10,
                    IdDataMeasure = 1
                }
            };

            DataMeasures2 = new()
            {
                new DataMeasure
                {
                    IdFileMeasure = 3,
                    Name = "Base 2",
                    Values = "{f = 10, v = 5.5}",
                    Type = 5,
                    FrequencyValue = 5,
                    FrequencyPos = 10,
                    IdDataMeasure = 1
                },
                new DataMeasure
                {
                    IdFileMeasure = 4,
                    Name = "Meas 2",
                    Values = "{f = 10, v = 5.5}",
                    Type = 5,
                    FrequencyValue = 5,
                    FrequencyPos = 10,
                    IdDataMeasure = 1
                }
            };
        }
    }
}
