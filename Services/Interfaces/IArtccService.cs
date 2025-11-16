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
        public Task<Artcc?> GetArtcc(string artccId);
        public IEnumerable<string>? GetStarsPositions(Artcc artcc, string facilityId);
        public IEnumerable<string>? GetArtccPositions(Artcc artcc);
    }
}
