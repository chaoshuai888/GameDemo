using System.IO;
using McpUnity.Unity;
using UnityEditor;
using UnityEngine;

namespace LawnDefense.EditorTools
{
    [InitializeOnLoad]
    public static class McpUnityAutoStart
    {
        private const string NpmExecutablePath = @"C:\Program Files\nodejs\npm.cmd";

        static McpUnityAutoStart()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall -= EnsureConfiguredAndStarted;
            EditorApplication.delayCall += EnsureConfiguredAndStarted;
        }

        [MenuItem("LawnDefense/MCP Unity/Ensure Auto Start")]
        public static void EnsureConfiguredAndStarted()
        {
            McpUnitySettings settings = McpUnitySettings.Instance;
            bool changed = false;

            if (!settings.AutoStartServer)
            {
                settings.AutoStartServer = true;
                changed = true;
            }

            if (File.Exists(NpmExecutablePath) && settings.NpmExecutablePath != NpmExecutablePath)
            {
                settings.NpmExecutablePath = NpmExecutablePath;
                changed = true;
            }

            if (changed)
            {
                settings.SaveSettings();
            }

            McpUnityServer server = McpUnityServer.Instance;
            if (server != null && settings.AutoStartServer && !server.IsListening)
            {
                server.StartServer();
            }
        }
    }
}
