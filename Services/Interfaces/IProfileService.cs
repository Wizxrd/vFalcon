using System.Collections.Generic;
using System.Threading.Tasks;
using vFalcon.Models;

namespace vFalcon.Services.Interfaces
{
    public interface IProfileService
    {
        List<Profile> LoadProfiles();
        Task New(string artcc, string name);
        Task Rename(string oldName, string newName);
        Task Copy(Profile profile);
        void Export(Profile profile);
        Task Delete(Profile profile);
        Task Import(string file);
        Task Save(Profile profile);
        Task SaveAs(Profile profile, string name);
    }
}