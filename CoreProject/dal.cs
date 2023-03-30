using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace CoreProject
{
    public class mechanic
    {
        [Key]
        public string MechanicId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool Active { get; set; }
        public string Image { get; set; }
        public virtual ICollection<services> servicelink { get; set; }
    }
    public class services
    {
        [Key]
        public string ServiceId { get; set; }
        public string slno { get; set; }
        [ForeignKey("servicelink")]
        public string MechanicId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public virtual mechanic servicelink { get; set; }
    }
    public class MechanicServices
    {
        [Required(ErrorMessage = "Please enter MechanicId")]
        public string MechanicId { get; set; }
        [Required(ErrorMessage = "Please enter Name")]
        public string Name { get; set; }
        public string Address { get; set; }
        public bool Active { get; set; }
        public string Image { get; set; }
        [Required(ErrorMessage = "Please enter ServiceId")]
        public string ServiceId { get; set; }
        [Required(ErrorMessage = "Please enter slno")]
        public string slno { get; set; }
        public string Description { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
    }

}
