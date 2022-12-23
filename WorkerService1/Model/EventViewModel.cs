using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerService1.Database;

namespace WorkerService1.Model
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string UserEmail { get; set; } 

        public string UserName{ get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime Time { get; set; }
        public string channelId { get; set; }

    }
}
