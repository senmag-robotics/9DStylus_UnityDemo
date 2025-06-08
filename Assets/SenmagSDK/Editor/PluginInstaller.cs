using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class PluginInstaller
{
    static PluginInstaller()
    {
       /* string sourcePath = "Assets/SenmagSDK/Editor/Plugins/FTD2XX64.dll";
        string destDir = "Assets/Plugins/x86_64";
        string destPath = Path.Combine(destDir, "FTD2XX64.dll");

        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        if (!File.Exists(destPath))
        {
            File.Copy(sourcePath, destPath);
            AssetDatabase.Refresh();
            Debug.Log("Senmag SDK: Copied FTDI DLL to Plugins folder.");
        }



        sourcePath = "Assets/SenmagSDK/Core/Scripts/DeviceInterfaces/FTDI_D2XX/D2XXWrapper/x64/Debug/D2XXWrapper.dll";
        destDir = "Assets/Plugins/x86_64";
        destPath = Path.Combine(destDir, "D2XXWrapper.dll");

        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        if (!File.Exists(destPath))
        {
            File.Copy(sourcePath, destPath);
            AssetDatabase.Refresh();
           // Debug.Log("Senmag SDK: Copied FTDI DLL to Plugins folder.");
        }*/
    }
}
