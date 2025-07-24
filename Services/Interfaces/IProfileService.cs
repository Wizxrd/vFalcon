using System.Collections.Generic;
using System.Threading.Tasks;
using vFalcon.Models;

namespace vFalcon.Services.Interfaces
{
    public interface IProfileService
    {
        public List<Profile>? LoadProfiles();
        public Task Rename(string oldName, string newName);
        public Task Copy(Profile profile);
        public void Export(Profile profile);
        public Task Delete(Profile profile);
        public void Save(Profile profile);
        public Task SaveAs(Profile profile, string name);
    }
}