using System.Collections.Generic;
using System.Threading.Tasks;
using vFalcon.Models;

namespace vFalcon.Services.Interfaces
{
    public interface IProfileService
    {
        public Task<List<Profile>> LoadProfiles();
        public Task New(string name, string artccId, string facilityId, string displayType);
        public Task<bool> Import();
        public Task Rename(string oldName, string newName);
        public Task Copy(Profile profile);
        public void Export(Profile profile);
        public Task Delete(Profile profile);
        public void Save(Profile profile);
        public Task SaveAsync(Profile profile);
        public Task SaveAs(Profile profile, string name);
    }
}