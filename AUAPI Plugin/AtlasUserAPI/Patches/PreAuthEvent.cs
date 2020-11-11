using EXILED;
using Cryptography;
using GameCore;
using Harmony;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror.LiteNetLib4Mirror;
using System;
using System.Collections.Generic;
using AtlasUserAPI.JSON;
using System.IO;
using System.Linq;

namespace AtlasUserAPI.Patches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest), typeof(ConnectionRequest))]
    public class PreAuthEventByAtlas
    {
		public static bool Prefix(ref ConnectionRequest request)
		{
			try
			{
				HandleConnection(request);
				return false;
			}
			catch (Exception exception)
			{
				EXILED.Log.Error($"PreAuthEvent error: {exception}");
				return true;
			}
		}
		private static void HandleConnection(ConnectionRequest request)
        {
			NetDataWriter rejectData = new NetDataWriter();
			try
			{
				byte result1;
				byte result2;
				if (!request.Data.TryGetByte(out result1) || !request.Data.TryGetByte(out result2) || result1 != CustomNetworkManager.Major || result2 != CustomNetworkManager.Minor)
				{
					rejectData.Reset();
					rejectData.Put(3);
					request.Reject(rejectData);
				}
				else
				{
					if (CustomLiteNetLib4MirrorTransport.IpRateLimiting)
					{
						if (CustomLiteNetLib4MirrorTransport.IpRateLimit.Contains(request.RemoteEndPoint.Address.ToString()))
						{
							ServerConsole.AddLog(string.Format("Connexion entrante à partir de l'IP {0} rejetée en raison d'un dépassement du taux limite.", request.RemoteEndPoint));
							ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("Connexion entrante à partir de l'IP {0} rejetée en raison d'un dépassement du taux limite.", request.RemoteEndPoint), ServerLogs.ServerLogType.RateLimit);
							rejectData.Reset();
							rejectData.Put(12);
							request.Reject(rejectData);
							return;
						}
						CustomLiteNetLib4MirrorTransport.IpRateLimit.Add(request.RemoteEndPoint.Address.ToString());
					}					

					string result3;
					if (!request.Data.TryGetString(out result3) || result3 == string.Empty)
					{
						rejectData.Reset();
						rejectData.Put(5);
						request.Reject(rejectData);
					}
					else
					{	
						ulong result4;
						byte result5;
						string result6;
						byte[] result7;
						if (!request.Data.TryGetULong(out result4) || !request.Data.TryGetByte(out result5) || !request.Data.TryGetString(out result6) || !request.Data.TryGetBytesWithLength(out result7))
						{
							rejectData.Reset();
							rejectData.Put(4);
							request.Reject(rejectData);
						}
						else
						{
							CentralAuthPreauthFlags flags = (CentralAuthPreauthFlags)result5;
							try
							{
								String steamID = result3;
								Login LoginJSON = new Login();
								LoginJSON.Steamid64 = steamID;
								LoginJSON.Ip = request.RemoteEndPoint.Address.ToString();
								String JSON = Serialize.ToJson(LoginJSON);
								String JsonResponse = Methods.Post(Plugin.LoginURL, JSON);

								try
								{
									JSON.Success.SuccessResponseJSON APIResponse = AtlasUserAPI.JSON.Success.SuccessResponseJSON.FromJson(JsonResponse);

									if (!ECDSA.VerifyBytes(string.Format("{0};{1};{2};{3}", result3, result5, result6, result4), result7, ServerConsole.PublicKey))
									{
										ServerConsole.AddLog(string.Format("Joueur avec l'IP {0} a envoyé un jeton de préauthentification avec une signature numérique non valide.", request.RemoteEndPoint));
										rejectData.Reset();
										rejectData.Put(2);
										request.Reject(rejectData);
									}
									else if (TimeBehaviour.CurrentUnixTimestamp > result4)
									{
										ServerConsole.AddLog(string.Format("Joueur avec l'IP {0} a envoyé un jeton de préauthentification périmé.", request.RemoteEndPoint));
										ServerConsole.AddLog("Assurez-vous que l'heure et le fuseau horaire définis sur le serveur sont corrects. Nous recommandons de synchroniser l'heure.");
										rejectData.Reset();
										rejectData.Put(11);
										request.Reject(rejectData);
									}
									else
									{
										if (CustomLiteNetLib4MirrorTransport.UserRateLimiting)
										{
											if (CustomLiteNetLib4MirrorTransport.UserRateLimit.Contains(result3))
											{
												ServerConsole.AddLog(string.Format("Connexion entrante de {0} ({1}) rejetée en raison d'un dépassement du taux limite.", result3, request.RemoteEndPoint));
												ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("Connexion entrante à partir de l'IP {0} ({1}) rejetée en raison d'un dépassement du taux limite.", result3, request.RemoteEndPoint), ServerLogs.ServerLogType.RateLimit);
												rejectData.Reset();
												rejectData.Put(12);
												request.Reject(rejectData);
												return;
											}
											CustomLiteNetLib4MirrorTransport.UserRateLimit.Add(result3);
										}
										if (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreBans) || !ServerStatic.GetPermissionsHandler().IsVerified)
										{
											// API Check BAN.
											if (APIResponse.IsBanned)
											{
												ServerConsole.AddLog(string.Format("Le joueur {0} a essayé de se connecter avec l'IP {1}, mais l'API répond qu'il est banni.", result3, request.RemoteEndPoint));
												ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("Le joueur {0} a essayé de se connecter avec l'IP {1}, mais l'API répond qu'il est banni.", result3, request.RemoteEndPoint), ServerLogs.ServerLogType.ConnectionUpdate);

												rejectData.Reset();
												rejectData.Put(6);
												request.Reject(rejectData);
												return;
											}
										}
										if (flags.HasFlagFast(CentralAuthPreauthFlags.GloballyBanned) && !ServerStatic.GetPermissionsHandler().IsVerified)
										{
											bool useGlobalBans = CustomLiteNetLib4MirrorTransport.UseGlobalBans;
										}
										if ((!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreWhitelist) || !ServerStatic.GetPermissionsHandler().IsVerified) && !WhiteList.IsWhitelisted(result3))
										{
											ServerConsole.AddLog(string.Format("Le joueur {0} a essayé de joindre à partir de l'IP {1}, mais n'est pas sur la liste blanche.", result3, request.RemoteEndPoint));
											rejectData.Reset();
											rejectData.Put(7);
											request.Reject(rejectData);
										}
										else if (CustomLiteNetLib4MirrorTransport.Geoblocking != GeoblockingMode.None && (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreGeoblock) || !ServerStatic.GetPermissionsHandler().BanTeamBypassGeo) && (!CustomLiteNetLib4MirrorTransport.GeoblockIgnoreWhitelisted || !WhiteList.IsOnWhitelist(result3)) && (CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Whitelist && !CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(result6.ToUpper()) || CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Blacklist && CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(result6.ToUpper())))
										{
											ServerConsole.AddLog(string.Format("Le joueur {0} ({1}) a tenté de rejoindre depuis le pays bloqué {2}.", result3, request.RemoteEndPoint, result6.ToUpper()));
											rejectData.Reset();
											rejectData.Put(9);
											request.Reject(rejectData);
										}
										else
										{
											// API Role & Slots
											string role;
											if (Plugin.role.TryGetValue(steamID, out role))
												Plugin.role.Remove(steamID);
											if (!String.IsNullOrEmpty(APIResponse.Role))
											{
												Plugin.role.Add(steamID, APIResponse.Role);
											}
											else
											{
												if (ServerStatic.GetPermissionsHandler()._members.ContainsKey(steamID))
													ServerStatic.GetPermissionsHandler()._members.Remove(steamID);
											}

											int num = CustomNetworkManager.slots;
											if (flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) && ServerStatic.GetPermissionsHandler().BanTeamSlots)
												num = LiteNetLib4MirrorNetworkManager.singleton.maxConnections;
											else if (ConfigFile.ServerConfig.GetBool("use_reserved_slots", true))
												// API Slots
												if (!String.IsNullOrEmpty(APIResponse.Role))
												{
													List<string> RoleRSRead = File.ReadAllLines(Plugin.RoleRSFilePath).ToList();
													if (RoleRSRead.Contains(APIResponse.Role))
													{
														num = CustomNetworkManager.singleton.maxConnections;
													}
												}
											if (LiteNetLib4MirrorCore.Host.PeersCount < num)
											{
												if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
													CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(result3);
												else
													CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint, new PreauthItem(result3));
												bool allow = true;
												Events.InvokePreAuth(ref result3, request, ref allow);
												if (allow)
												{
													request.Accept();
													ServerConsole.AddLog(string.Format("Le joueur {0} est préauthentifié à partir de l'IP {1}.", result3, request.RemoteEndPoint));
													ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("{0} préauthentifié à partir de l'IP {1}.", result3, request.RemoteEndPoint), ServerLogs.ServerLogType.ConnectionUpdate);
												}
											}
											else
											{
												ServerConsole.AddLog(string.Format("Le joueur {0} ({1}) a essayé de se connecter, mais le serveur est plein.", result3, request.RemoteEndPoint));
												rejectData.Reset();
												rejectData.Put(1);
												request.Reject(rejectData);
											}
										}
									}
								}
								catch (Exception exception)
								{
									ServerConsole.AddLog(string.Format("Le joueur avec l'IP {0} a envoyé un jeton de préauthentification non valable. {1}", request.RemoteEndPoint, exception.Message));
									rejectData.Reset();
									rejectData.Put(2);
									request.Reject(rejectData);
								}
							}
							catch (Exception exception)
							{
								ServerConsole.AddLog(string.Format("Le joueur avec l'IP {0} a subi une erreur avec l'API. {1}", request.RemoteEndPoint, exception.Message));
								rejectData.Reset();
								rejectData.Put(2);
								request.Reject(rejectData);
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				ServerConsole.AddLog(string.Format("Joueur avec l'IP {0} n'a pas réussi à se préauthentifier : {1}", request.RemoteEndPoint, exception.Message));
				rejectData.Reset();
				rejectData.Put(4);
				request.Reject(rejectData);
			}
		}
    }
}
