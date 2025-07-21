using System.Collections.Generic;

namespace vFalcon.Services.Interfaces
{
    public interface IArtccService
    {
        IEnumerable<string> GetArtccs();
        void BuildArtccFile(string inputPath, string outputPath);
    }
}
