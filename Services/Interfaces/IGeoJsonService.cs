using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Services.Interfaces
{
    public interface IGeoJsonService
    {
        JObject ConvertLineStringsToPolygon(string sourceFile, string artccId);
        void CombineGeoJsonFiles(string[] inputFilePaths, string outputFilePath);
        void CleanGeoJson(string inputFile, string outputFile);
    }
}
