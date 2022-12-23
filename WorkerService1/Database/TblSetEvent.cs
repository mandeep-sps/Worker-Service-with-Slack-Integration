using System;
using System.Collections.Generic;

namespace WorkerService1.Database
{
    public partial class TblSetEvent
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Descrption { get; set; } = null!;
        public DateTime Time { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }

        public virtual TblUserRegsitration User { get; set; } = null!;
    }
}
