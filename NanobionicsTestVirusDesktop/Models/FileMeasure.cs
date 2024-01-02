using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace NanobionicsTestVirusDesktop.Models
{
    public class FileMeasure
    {
        [Key]
        public int IdFileMeasure { get; set; }
        public string Name { get; set; }
        public string ContentValue { get; set; }
        public double CutOffMax { get; set; }
        public double CutOffMin { get; set; }
        public double FreqCutMin { get; set; }
        public double FreqCutMax { get; set; }
        public DateTime LoadedAt { get; set; }

        public virtual ICollection<DataMeasure>
            DataMeasures
        { get; set; } = new ObservableCollection<DataMeasure>();
    }
}
