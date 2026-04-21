using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using PackageManager.Alpm.Pacfile;

namespace PackageManager;

/// <summary>
/// Manages storage locations for Shelly only storage. This is to be used to support features that don't exist
/// inside pacman.
/// </summary>
/// <param name="configPath"></param>
public class ShellyDatastore(string configPath = "/var/lib/shelly")
{
    private const string PacfileStoragePath = "pacfiles.d";

    public Task<string> GetPacfileStoragePath()
    {
        var path = Path.Combine(configPath, PacfileStoragePath);
        Directory.CreateDirectory(path);
        return Task.FromResult(path);
    }
}