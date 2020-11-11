using EXILED;
using System;
using System.IO;
using System.Collections.Generic;
using Harmony;

namespace AtlasUserAPI
{
    public class Plugin : EXILED.Plugin
    {
        private HarmonyInstance instance;
        private static int patchFixer;
        public static bool Enabled;
        public static string LoginURL;
        public static string BanURL;
        public static string TokenAPI;
        public static string UserAPI;
        public static string UserTokenAPI;
        public static string RoleRSFilePath;
        public static Dictionary<string, string> role = new Dictionary<string, string>();
        private static EventHandlers EventHandler;

        public override void OnEnable()
        {
            try
            {
                // Désactivation du PATCH PreAuth d'EXILED
                EventPlugin.PreAuthEventPatchDisable = true;

                Enabled = Config.GetBool("atlasuserapi_enable", true);
                LoginURL = Config.GetString("atlasuserapi_loginurl", "");
                BanURL = Config.GetString("atlasuserapi_banurl", "");
                TokenAPI = Config.GetString("atlasuserapi_token", "");
                UserAPI = Config.GetString("atlasuserapi_user", "");
                UserTokenAPI = Config.GetString("atlasuserapi_usertoken", "");

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string pluginPath = Path.Combine(appData, "Plugins");
                string path = Path.Combine(pluginPath, "AtlasUserAPI");
                string RoleRSFileName = Path.Combine(path, "Role-RS.txt");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(RoleRSFileName))
                    File.Create(RoleRSFileName).Close();
                RoleRSFilePath = RoleRSFileName;

                if (string.IsNullOrWhiteSpace(BanURL) || string.IsNullOrWhiteSpace(LoginURL) || string.IsNullOrWhiteSpace(TokenAPI) || string.IsNullOrWhiteSpace(UserAPI) || string.IsNullOrWhiteSpace(UserTokenAPI) || !Enabled)
                {
                    Log.Info("Le plugin est désactivé, ou il manque des entrées dans la configuration.");
                    return;
                }

                EventHandler = new EventHandlers(this);
                Events.PlayerJoinEvent += EventHandler.playerJoin;

                try
                {
                    patchFixer++;
                    instance = HarmonyInstance.Create($"atlasuserapi.patches{patchFixer}");
                    instance.PatchAll();
                    Log.Info("Le PATCH vient d'avoir lieu avec succès.");
                    Log.Info("Plugin chargé et démarré.");
                }
                catch (Exception exception)
                {
                    Log.Error($"Erreur durant le PATCH: {exception}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Erreur durant le démarrage du plugin: {e}");
            }
        }

        public override void OnDisable()
        {
            Events.PlayerJoinEvent -= EventHandler.playerJoin;

            instance.UnpatchAll();
        }

        public override void OnReload() { }

        public override string getName { get; } = "AtlasUserAPI";
       
    }

}
