using System;
using EXILED;

namespace SanctionSystem
{
    public class Plugin : EXILED.Plugin
    {
        public static bool Enabled;
        public static string BanURL;
        public static string TokenAPI;
        public static string UserAPI;
        public static string UserTokenAPI;
        private static EventHandlers EventHandler;

        public override void OnEnable()
        {
            try
            {
                Enabled = Config.GetBool("atlasuserapi_enable", true);
                BanURL = Config.GetString("atlasuserapi_banurl", "");
                TokenAPI = Config.GetString("atlasuserapi_token", "");
                UserAPI = Config.GetString("atlasuserapi_user", "");
                UserTokenAPI = Config.GetString("atlasuserapi_usertoken", "");

                if (string.IsNullOrWhiteSpace(TokenAPI) || string.IsNullOrWhiteSpace(UserAPI) || string.IsNullOrWhiteSpace(UserTokenAPI) || !Enabled)
                {
                    Log.Info("Le plugin est désactivé, ou il manque des entrées dans la configuration.");
                    return;
                }

                EventHandler = new EventHandlers(this);
                Events.RemoteAdminCommandEvent += EventHandler.RemoteAdminCommandEvent;
                Events.PlayerBanEvent += EventHandler.playerBan;

                Log.Info("Le plugin est maintenant activé.");
            }
            catch (Exception e)
            {
                Log.Error($"Erreur durant le démarrage du plugin: {e}");
            }
        }

        public override void OnDisable()
        {
            Events.RemoteAdminCommandEvent -= EventHandler.RemoteAdminCommandEvent;
            Events.PlayerBanEvent -= EventHandler.playerBan;
        }

        public override void OnReload() { }

        public override string getName => "SanctionSystem";
    }
}
