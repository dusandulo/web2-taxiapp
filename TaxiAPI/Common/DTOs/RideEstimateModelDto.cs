using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class RideEstimateDto
    {
        [Required]
        public string StartAddress { get; set; } = string.Empty;

        [Required]
        public string EndAddress { get; set; } = string.Empty;
    }
}
