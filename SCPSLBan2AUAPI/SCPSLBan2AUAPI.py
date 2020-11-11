# Script pour convertir les fichiers de bans SCP:SL vers des JSON compatibles avec AtlasUserAPI.
# Créer par Antt0n (t.me/Antt0n)

import json 
from datetime import datetime

# STEAMID64

BanFile_SID64 = "UserIdBans.txt"
BanFileJSON_SID64 = BanFile_SID64 + ".json"
BanArray_SID64 = []

with open(BanFile_SID64, encoding="utf8") as fp:
    line = fp.readline()
    while line:
        ban_info = line.strip().split(";")
        ban_obj = {
            "steamid64": ban_info[1],
            "bannedAt": ban_info[5],
            "bannedUntil": ban_info[2],
            "by": ban_info[4],
            "reason": "SCPSLBan2API:" + ban_info[3]
        }
        BanArray_SID64.append(ban_obj)
        line = fp.readline()

with open(BanFileJSON_SID64,'w',encoding="utf-8") as json_file:
        json.dump(BanArray_SID64, json_file)

# IP

BanFile_IP = "IpBans.txt"
BanFileJSON_IP = BanFile_IP + ".json"
BanArray_IP = []

with open(BanFile_IP, encoding="utf8") as fp:
    line = fp.readline()
    while line:
        ban_info = line.strip().split(";")
        ban_obj = {
            "ip": ban_info[1],
            "bannedAt": ban_info[5],
            "bannedUntil": ban_info[2],
            "by": ban_info[4],
            "reason": "SCPSLBan2API:" + ban_info[3]
        }
        BanArray_IP.append(ban_obj)
        line = fp.readline()

with open(BanFileJSON_IP,'w',encoding="utf-8") as json_file:
        json.dump(BanArray_IP, json_file)