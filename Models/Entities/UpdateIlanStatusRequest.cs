using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBGoreWebApp.Models.Entities
{
    public class UpdateIlanStatusRequest
{
    public int Id { get; set; }
    public string? Status { get; set; }
}

}