using System.ComponentModel.DataAnnotations;

namespace NanobionicsTestVirusDesktop.Models
{
    public class DataMeasure
    {
        [Key]
        public int IdDataMeasure { get; set; }
        public string Name { get; set; }
        public string Values { get; set; }
        public int Type { get; set; }
        public int PointsCount { get; set; }
        public double? FrequencyValue { get; set; }
        public int? FrequencyPos { get; set; }
        public int IdFileMeasure { get; set; }
        public virtual FileMeasure FileMeasure { get; set; }
    }
}
