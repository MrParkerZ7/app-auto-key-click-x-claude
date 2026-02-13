using System.IO;

namespace AutoClickKey.Services;

public interface IFileSystem
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    void CreateDirectory(string path);

    string ReadAllText(string path);

    void WriteAllText(string path, string content);

    void DeleteFile(string path);

    string[] GetFiles(string path, string searchPattern);
}

public class FileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

    public void DeleteFile(string path) => File.Delete(path);

    public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
}
