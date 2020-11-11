using SanctionSystem.BanJSON;
using EXILED;
using EXILED.Extensions;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SanctionSystem
{
	class EventHandlers
	{
		private Plugin plugin;
		public EventHandlers(Plugin pl)
		{
			plugin = pl;
		}

		public void RemoteAdminCommandEvent(ref RACommandEvent ev)
		{
			string[] args = ev.Command.Split(' ');
			ReferenceHub sender = ev.Sender.SenderId == "SERVER CONSOLE" || ev.Sender.SenderId == "GAME CONSOLE" ? Player.GetPlayer(PlayerManager.localPlayer) : Player.GetPlayer(ev.Sender.SenderId);

			switch (args[0].ToLower())
			{
				case "oban":
					ev.Allow = false;
					if (!sender.CheckPermission("oban.use"))
					{
						ev.Sender.RaReply("ExiledPermissions#No permission.", true, true, string.Empty);
						return;
					}
					if (args.Length == 1)
					{
						ev.Sender.RaReply("OBAN#Commandes:", true, true, string.Empty);
						ev.Sender.RaReply("#- OBAN - Afficher la liste des commandes.", true, true, string.Empty);
						ev.Sender.RaReply("#- OBAN STEAMID [SteamID@steam] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
						ev.Sender.RaReply("#- OBAN IP [IP] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
						ev.Sender.RaReply("#- OBAN USER [SteamID@steam] [IP] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
						return;
					}
					else if (args.Length > 1)
					{
						switch (args[1].ToLower())
						{
							case "steamid":
								if (args.Length > 3)
								{
									string Steamid64 = args[2];
									long BannedAt = TimeBehaviour.CurrentTimestamp();
									double Expire = (double.TryParse(args[3], out double x)) ? x : -1;
									string By = sender.characterClassManager.UserId;
									string Reason = (args.Length > 3) ? string.Join(" ", args.Skip(4)) : string.Empty;

									Ban BanJSON = new Ban();
									BanJSON.Steamid64 = Steamid64;
									BanJSON.BannedAt = BannedAt;
									BanJSON.BannedUntil = DateTime.UtcNow.AddMinutes((double)Expire).Ticks;
									BanJSON.By = By;
									if (!String.IsNullOrEmpty(Reason))
										BanJSON.Reason = Reason;

									String JSON = Serialize.ToJson(BanJSON);
									String JsonResponse = Methods.Post(Plugin.BanURL, JSON);
									try
									{
										JSON.Success.SuccessResponseJSON json = SanctionSystem.JSON.Success.SuccessResponseJSON.FromJson(JsonResponse);

										string response = "\n" +
											"SteamID: " + Steamid64 + "\n" +
											"Est banni pour : " + Expire + " minutes \n" +
											"Par: " + By + " / " + sender.nicknameSync.MyNick;
										ev.Sender.RaReply(args[0].ToUpper() + "#" + response, true, true, "");
									}
									catch (Exception e)
									{
										JSON.Error.ErrorResponseJSON json = SanctionSystem.JSON.Error.ErrorResponseJSON.FromJson(JsonResponse);

										if (!String.IsNullOrEmpty(json.Code))
											ev.Sender.RaReply("Erreur durant le processus d'API (Code d'erreur répondu par l'API: " + json.Code, true, true, string.Empty);
										else
											ev.Sender.RaReply("Erreur durant le processus d'API (Code d'erreur plugin: " + e, true, true, string.Empty);
									}
								}
								else
								{
									ev.Sender.RaReply("#- OBAN STEAMID [SteamID@steam] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
								}
								break;
							case "ip":
								if (args.Length > 3)
								{
									string IP = args[2];
									long BannedAt = TimeBehaviour.CurrentTimestamp();
									double Expire = (double.TryParse(args[3], out double x)) ? x : -1;
									string By = sender.characterClassManager.UserId;
									string Reason = (args.Length > 3) ? string.Join(" ", args.Skip(4)) : string.Empty;

									Ban BanJSON = new Ban();
									BanJSON.Ip = IP;
									BanJSON.BannedAt = BannedAt;
									BanJSON.BannedUntil = DateTime.UtcNow.AddMinutes((double)Expire).Ticks;
									BanJSON.By = By;
									if (!String.IsNullOrEmpty(Reason))
										BanJSON.Reason = Reason;

									String JSON = Serialize.ToJson(BanJSON);
									String JsonResponse = Methods.Post(Plugin.BanURL, JSON);
									try
									{
										JSON.Success.SuccessResponseJSON json = SanctionSystem.JSON.Success.SuccessResponseJSON.FromJson(JsonResponse);

										string response = "\n" +
											"IP: " + IP + "\n" +
											"Est banni pour : " + Expire + " minutes \n" +
											"Par: " + By + " / " + sender.nicknameSync.MyNick;
										ev.Sender.RaReply(args[0].ToUpper() + "#" + response, true, true, "");
									}
									catch (Exception e)
									{
										JSON.Error.ErrorResponseJSON json = SanctionSystem.JSON.Error.ErrorResponseJSON.FromJson(JsonResponse);

										if (!String.IsNullOrEmpty(json.Code))
											ev.Sender.RaReply("Erreur durant le processus d'API (Code d'erreur répondu par l'API: " + json.Code, true, true, string.Empty);
										else
											ev.Sender.RaReply("Erreur durant le processus d'API (Code d'erreur plugin: " + e, true, true, string.Empty);
									}
								}
								else
								{
									ev.Sender.RaReply("#- OBAN IP [IP] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
								}
								break;
							case "user":
								if (args.Length > 4)
								{
									string Steamid64 = args[2];
									string IP = args[3];
									long BannedAt = TimeBehaviour.CurrentTimestamp();
									double Expire = (double.TryParse(args[4], out double x)) ? x : -1;
									string By = sender.characterClassManager.UserId;
									string Reason = (args.Length > 4) ? string.Join(" ", args.Skip(5)) : string.Empty;

									Ban BanJSON = new Ban();
									BanJSON.Steamid64 = Steamid64;
									BanJSON.Ip = IP;
									BanJSON.BannedAt = BannedAt;
									BanJSON.BannedUntil = DateTime.UtcNow.AddMinutes((double)Expire).Ticks;
									BanJSON.By = By;
									if (!String.IsNullOrEmpty(Reason))
										BanJSON.Reason = Reason;

									String JSON = Serialize.ToJson(BanJSON);
									String JsonResponse = Methods.Post(Plugin.BanURL, JSON);
									try
									{
										JSON.Success.SuccessResponseJSON json = SanctionSystem.JSON.Success.SuccessResponseJSON.FromJson(JsonResponse);

										string response = "\n" +
											"SteamID: " + Steamid64 + "\n" +
											"IP: " + IP + "\n" +
											"Est banni pour : " + Expire + " minutes \n" +
											"Par: " + By + " / " + sender.nicknameSync.MyNick;
										ev.Sender.RaReply(args[0].ToUpper() + "#" + response, true, true, "");
									}
									catch (Exception e)
									{
										JSON.Error.ErrorResponseJSON json = SanctionSystem.JSON.Error.ErrorResponseJSON.FromJson(JsonResponse);

										if (!String.IsNullOrEmpty(json.Code))
											ev.Sender.RaReply("Erreur durant le processus d'API (Code d'erreur répondu par l'API: " + json.Code, true, true, string.Empty);
										else
											ev.Sender.RaReply("Erreur durant le processus d'API (Code d'erreur plugin: " + e, true, true, string.Empty);
									}
								}
								else
								{
									ev.Sender.RaReply("#- OBAN USER [SteamID@steam] [IP] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
								}
								break;
							default:
								ev.Sender.RaReply("OBAN#Commandes:", true, true, string.Empty);
								ev.Sender.RaReply("#- OBAN - Afficher la liste des commandes.", true, true, string.Empty);
								ev.Sender.RaReply("#- OBAN STEAMID [SteamID@steam] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
								ev.Sender.RaReply("#- OBAN IP [IP] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
								ev.Sender.RaReply("#- OBAN USER [SteamID@steam] [IP] [MINUTES] (RAISON OPTIONNELLE)", true, true, string.Empty);
								return;
						}
					}
					break;
			}
		}
		public void playerBan(PlayerBanEvent ev)
		{
			ev.Allow = false;
			GameObject player = ev.BannedPlayer.characterClassManager.gameObject;

			if (ev.Duration == 0)
			{
				ServerConsole.Disconnect(player, ev.FullMessage);
			}
			else
			{
				ServerConsole.Disconnect(player, ev.FullMessage);

				Ban BanJSON = new Ban();
				BanJSON.Steamid64 = ev.BannedPlayer.characterClassManager.UserId;
				BanJSON.Ip = ev.BannedPlayer.queryProcessor._ipAddress;
				BanJSON.BannedAt = TimeBehaviour.CurrentTimestamp();
				BanJSON.BannedUntil = DateTime.UtcNow.AddMinutes((double)ev.Duration).Ticks;
				BanJSON.By = ev.Issuer.characterClassManager.UserId;
				if (!String.IsNullOrEmpty(ev.Reason))
					BanJSON.Reason = ev.Reason;

				String JSON = Serialize.ToJson(BanJSON);
				String JsonResponse = Methods.Post(Plugin.BanURL, JSON);

				try
				{
					JSON.Success.SuccessResponseJSON json = SanctionSystem.JSON.Success.SuccessResponseJSON.FromJson(JsonResponse);
				}
				catch (Exception e)
				{
					JSON.Error.ErrorResponseJSON json = SanctionSystem.JSON.Error.ErrorResponseJSON.FromJson(JsonResponse);

					if (!String.IsNullOrEmpty(json.Code))
						Log.Error("Erreur durant le processus d'API (Code d'erreur répondu par l'API: " + json.Code);
					else
						Log.Error("Erreur durant le processus d'API (Code d'erreur plugin: " + e);
				}
			}
		}
	}
}