using System;

namespace LineStatusServer.Models
{
    public class LineShift
    {
        public int ID { get; set; }
        public string LineCode { get; set; }
        public int WorkShiftID { get; set; }
    }

    public class LineShiftDTO: LineShift
    {
        public int ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
