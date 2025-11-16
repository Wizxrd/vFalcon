using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Models;

namespace vFalcon.Services.Interfaces
{
    public interface IArtccService
    {
        public Task<Artcc>? LoadArtcc(string artccId);
    }
}
