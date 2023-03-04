using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_When.Models
{
    public class Settings
    {
        [Key] public int ID { get; set; }
        public DateTime LastDocOpenDate { get; set; }
        public TimeSpan DocTimeout { get; set; }
        public bool IsFirstLaunch { get; set; }
    }
}
