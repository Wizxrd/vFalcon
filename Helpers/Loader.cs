using System.IO;

namespace vFalcon.Helpers
{
    public class Loader
    {

        public static string GetAppDirectory()
        {
            try
            {
#if DEBUG
                var baseDir = AppContext.BaseDirectory;
                var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                return solutionDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
#else
        var exeDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parent = Directory.GetParent(exeDir)?.FullName ?? exeDir;
        return parent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
#endif
            }
            catch (Exception e)
            {
                Logger.Error("GetAppDirectory", e.ToString());
                return string.Empty;
            }
        }

        public static string LoadFile(string folderPath, string fileName)
        {
            try
            {
                string folderDir;
#if DEBUG
                string binDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string solutionDir = Directory.GetParent(binDir).FullName;
                folderDir = Path.Combine(solutionDir, folderPath);
#else
        string exeDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string parent = Directory.GetParent(exeDir)?.FullName ?? exeDir;
        folderDir = Path.Combine(parent, folderPath);
#endif
                return Path.Combine(folderDir, fileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Load", ex.ToString());
                return string.Empty;
            }
        }

        public static string LoadFolder(string folderPath)
        {
            try
            {
                string folderDir;
#if DEBUG
                string binDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string solutionDir = Directory.GetParent(binDir).FullName;
                folderDir = Path.Combine(solutionDir, folderPath);
#else
        string exeDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string parent = Directory.GetParent(exeDir)?.FullName ?? exeDir;
        folderDir = Path.Combine(parent, folderPath);
#endif
                if (!Directory.Exists(folderDir)) Directory.CreateDirectory(folderDir);
                return folderDir;
            }
            catch (Exception ex)
            {
                Logger.Error("Load", ex.ToString());
                return string.Empty;
            }
        }
    }
}
