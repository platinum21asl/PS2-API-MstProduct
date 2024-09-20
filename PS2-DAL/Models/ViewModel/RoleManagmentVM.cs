using Microsoft.AspNetCore.Mvc.Rendering;
using PS2_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS2_DAL.Models.ViewModel
{
    public class RoleManagmentVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }
    }
}
