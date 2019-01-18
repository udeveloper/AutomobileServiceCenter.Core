using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WebMVC.Areas.Configuration.Models.MasterDataViewModels
{
    public class MasterDataKeyViewModel
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public bool IsActive { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
