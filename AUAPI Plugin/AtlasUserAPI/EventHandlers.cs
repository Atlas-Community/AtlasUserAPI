using EXILED;
using System;

namespace AtlasUserAPI
{
    class EventHandlers
    {
        private Plugin plugin;
        public EventHandlers(Plugin pl)
        {
            plugin = pl;
        }

        public void playerJoin(PlayerJoinEvent ev)
        {
            ReferenceHub player = ev.Player;
            String steamID = ev.Player.characterClassManager.UserId;

            string role;
            if (Plugin.role.TryGetValue(steamID, out role))
            {
                if (ServerStatic.GetPermissionsHandler()._groups.ContainsKey(role))
                    EXILED.Extensions.Player.SetRank(player, ServerStatic.GetPermissionsHandler()._groups[role].BadgeText, ServerStatic.GetPermissionsHandler()._groups[role].BadgeColor, true, role);
                else
                    Log.Error("Le role \"" + role + "\" n'existe pas dans la configuration (Utilisateur: " + player.nicknameSync.MyNick + ").");
                Plugin.role.Remove(steamID);
            }
        }
    }
}