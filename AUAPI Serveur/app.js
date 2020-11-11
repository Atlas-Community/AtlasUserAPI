///////////////////////////////////////////
// AtlasUserAPI par Antt0n (t.me/Antt0n) //
// Créer pour le projet Atlas Community  //
// Version 1.0                           //
///////////////////////////////////////////

const fs = require("fs");
const https = require("https");
const mongoose = require('mongoose');
const bodyParser = require("body-parser"); 
const express = require("express"); 
const Ddos = require("ddos");
const ddos = new Ddos({burst:15, limit:35});
const helmet = require('helmet');
const CryptoJS = require("crypto-js");
const chalk = require("chalk");
const config = require("config");

const appName = config.get("app.name");
const debug = config.get("app.debug");
const accessToken =  CryptoJS.SHA256(config.get("app.passkey")).toString(CryptoJS.enc.Base64);
const usersConfig = config.get("users");
var usersToken = {};
for(var pos in usersConfig) usersToken[pos] = CryptoJS.SHA256(usersConfig[pos]).toString(CryptoJS.enc.Base64);
const webserver = config.get("webserver");
const database = config.get("database");
const mongoURL = "mongodb://" + database.user + ":" + database.password + "@" + database.host + ":" + database.port + "/" + database.db;

const WEBServer = express();
WEBServer.use(bodyParser.urlencoded({ extended: false }));
WEBServer.use(bodyParser.json());
WEBServer.set('trust proxy', true)
WEBServer.use(ddos.express);
WEBServer.use(helmet());
WEBServer.use((req, res, next) => {
    if (!req.headers.authorization || req.headers.authorization != accessToken || !req.headers.user || !usersToken.hasOwnProperty(req.headers.user) || !req.headers.usertoken || usersToken[req.headers.user] != req.headers.usertoken) {
        if (debug) console.log(head + error("Tentative de connexion à l'API avec erreur dans l'authentification (Adresse IP: "+ req.ip +")."));
        return res.status(403).json({success: false, code: "AUTH", message: "AccessToken requit.", methode : req.method});
    }
    next();
});
var Routeur = express.Router();

const head = chalk.yellowBright(appName + ": ");
const error = chalk.bold.red;
const warning = chalk.yellow;
const info = chalk.cyan;

///////////////////////////////////////////

async function databaseInit() {

    function exit(err) {
        console.log(head + error("Erreur durant l'établissement de la connexion (" + err["message"] + ")."))
        process.exit();
    }

    await mongoose.connect(mongoURL, { useNewUrlParser: true, useUnifiedTopology: true, serverSelectionTimeoutMS: 3000 })
        .then(() => console.log(head + "Connexion à la base de donnée avec succès."))
        .catch(err => exit(err));

}

if (debug) {
    var db = mongoose.connection;
    db.on('error', console.error.bind(console, 'Erreur avec MongoDB: '));
}

///////////////////////////////////////////

const User = mongoose.model('User', mongoose.Schema({
    steamid64: String,
    ip: String,
    role: String,
    first_login: String,
    last_login: String
    })
, "users");

const Banlist = mongoose.model('Banlist', mongoose.Schema({
    steamid64: String,
    ip: String,
    bannedAt: Number,
    bannedUntil: Number,
    by: String,
    reason: String
    })
, "banlist");

///////////////////////////////////////////
// Route par défaut

Routeur.route("/")
.all((req,res) => { 
    res.json({success: false, code: "MISSARG", message: "Il manque des arguments.", methode : req.method});
    console.log(head + warning("Connexion à l'API sans arguments (Informations: "+ req.headers.user +"/"+ req.ip +")."));
});

///////////////////////////////////////////
// Route /user|/:user_id]

Routeur.route("/user")
.get((req,res) => { 
    User.find()
    .then(user => res.json(user))
    .catch(err => res.send(err));
    if (debug) console.log(head + info("Liste des utilisateurs demandé (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})
.post((req,res) => {
    steamid64 = req.body.steamid64;
    ip = req.body.ip;
    role = req.body.role;
    login_date = req.body.login_date; 

    if (!steamid64 || !ip || !login_date) {
        res.send({message : "Erreur: il manque une entrée."})
    } else {

        var user = new User();

        user.steamid64 = steamid64;
        user.ip = ip;
        user.role = role;
        user.first_login = login_date;
        user.last_login = login_date;

        user.save()
        .then(() => res.send({success: true, message: "Le nouvel utilisateur a été ajouté avec succès."}))
        .catch(err => res.send({success:false, message: 'Error' + err}));

        if (debug) console.log(head + info("Nouvel utilisateur enregistré (Informations: "+ req.headers.user +"/"+ req.ip +")."));
    }
})
.purge((req,res) => {
    if (debug) {
        db.dropCollection("users")
        .then(() => res.send({success:true, message: "La liste des utilisateurs est maintenant purgé."}))
        .catch(err => res.send({success:true, message: "Erreur: " + err}));
    }
});
 
Routeur.route("/user/:user_id")
.get((req,res) => { 
    User.findById(req.params.user_id)
    .then(user => res.json(user))
    .catch(err => res.send(err));

    if (debug) console.log(head + info("Information d'un utilisateur demandé (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})
.patch((req,res) => { 

    steamid64 = req.body.steamid64;
    ip = req.body.ip;
    role = req.body.role;
    first_login = req.body.first_login; 
    last_login = req.body.last_login;

    var user = Object();

    if (steamid64) {
        user.steamid64 = steamid64;
    }
    if (ip) {
        user.ip = ip;
    }
    if (role) {
        user.role = role;
    }
    if (first_login) {
        user.first_login = first_login;
    }
    if (last_login) {
        user.last_login = last_login;
    }
    
    User.updateOne({_id: req.params.user_id}, user)
    .then(() => res.send({success: true, message: "Mise à jour de l'utilisateur " + req.params.user_id + " avec succès."}))
    .catch(err => res.send({success: false, message: "Erreur: " + err}));           

    if (debug) console.log(head + info("Modification d'un utilisateur (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})
.delete((req,res) => { 
 
    User.deleteOne({_id: req.params.user_id})
    .then(() => res.json({success: true, message: "Utilisateur supprimé."}))
    .catch(err => res.send({success: false, message: "Erreur: " + err}));   

    if (debug) console.log(head + info("Suppression d'un utilisateur (Informations: "+ req.headers.user +"/"+ req.ip +")."));
});

///////////////////////////////////////////
// Route /api/banlist

Routeur.route("/banlist")
.get((req,res) => { 
    Banlist.find()
    .then(user => res.json(user))
    .catch(err => res.send(err));
    if (debug) console.log(head + info("Liste des bannis demandé (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})
.post((req,res) => {

    steamid64 = req.body.steamid64;
    ip = req.body.ip;
    bannedAt = req.body.bannedAt;
    bannedUntil = req.body.bannedUntil;
    by = req.body.by;
    reason = req.body.reason;

    if (( !steamid64 && !ip ) || !bannedAt || !bannedUntil || !by) {
        res.send({success: false, message : "Erreur: il manque une entrée."})
    } else {

        var user = new Banlist();

        user.steamid64 = steamid64;
        user.ip = ip;
        user.bannedAt = bannedAt;
        user.bannedUntil = bannedUntil;
        user.by = by;

        if (reason) {
            user.reason = reason;
        }

        user.save()
        .then(() => res.send({success: true, message: "Le nouveau bannissement a été ajouté avec succès."}))
        .catch(err => res.send({success:false, message: 'Error' + err}));

        if (debug) console.log(head + info("Nouveau bannissement enregistré (Informations: "+ req.headers.user +"/"+ req.ip +")."));
    }
})
.purge((req,res) => {
    if (debug) {
        db.dropCollection("banlist")
        .then(() => res.send({success:true, message: "La liste des bannis est maintenant purgé."}))
        .catch(err => res.send({success:true, message: "Erreur: " + err}));
    }
});
 
Routeur.route("/banlist/:user_id")
.get((req,res) => { 
    Banlist.findById(req.params.user_id)
    .then(user => res.json(user))
    .catch(err => res.send(err));

    if (debug) console.log(head + info("Information d'un bannissement demandé (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})
.patch((req,res) => { 

    steamid64 = req.body.steamid64;
    ip = req.body.ip;
    bannedAt = req.body.bannedAt;
    bannedUntil = req.body.bannedUntil;
    by = req.body.by;
    reason = req.body.reason;

    var user = Object();

    if (steamid64) {
        user.steamid64 = steamid64;
    }
    if (ip) {
        user.ip = ip;
    }
    if (bannedAt) {
        user.bannedAt = bannedAt;
    }
    if (bannedUntil) {
        user.bannedUntil = bannedUntil;
    }
    if (by) {
        user.by = by;
    }
    if (reason) {
        user.reason = reason;
    }
    
    Banlist.updateOne({_id: req.params.user_id}, user)
    .then(() => res.send({success: true, message: "Mise à jour du bannissement " + req.params.user_id + " avec succès."}))
    .catch(err => res.send({success: false, message: "Erreur: " + err}));           

    if (debug) console.log(head + info("Modification d'un bannissement (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})
.delete((req,res) => { 

    Banlist.deleteOne({_id: req.params.user_id})
    .then(() => res.json({success: true, message: "Bannissement supprimé."}))
    .catch(err => res.send({success: false, message: "Erreur: " + err}));   

    if (debug) console.log(head + info("Suppression d'un utilisateur (Informations: "+ req.headers.user +"/"+ req.ip +")."));

});

///////////////////////////////////////////
// Route /api/login-scpsl

Routeur.route("/api/login-scpsl")
.post((req,res) => {

    steamid64 = req.body.steamid64;
    ip = req.body.ip;

    function UserCheck(req, res) {

        date_ob = new Date(Date.now());
        day = date_ob.getDate();
        month = date_ob.getMonth();
        year = date_ob.getFullYear();
        hours = date_ob.getHours();
        minutes = date_ob.getMinutes();
        seconds = date_ob.getSeconds();

        login_date = day+"/"+month+"/"+year+"-"+hours+":"+minutes+":"+seconds;
    
        if (!steamid64 || !ip) {
            res.send({success: false, code: "MISSARG", message: "Erreur: il manque une entrée."})
            console.log(req.body);
        } else {


            User.find({steamid64: steamid64})
            .then(user => {
                
                if (user.length == 0) {
    
                    var user = new User();
                
                    user.steamid64 = steamid64;
                    user.ip = ip;
                    user.first_login = login_date;
                    user.last_login = login_date;
                
                    user.save()
                    .then(() => { if (debug) console.log(head + warning("Nouvel utilisateur enregistré (Informations: " + req.headers.user +"/"+ req.ip +").")) })
                    .catch(err => { res.json({success: false, code: "ADDUSER", message: "Erreur: " + err})});
    
    
                } else {
    
                    if (user.length > 1) {
                        
                        for ( var i = 1; i < user.length; i++) {
                            User.deleteOne({_id: user[i]._id})
                            .then(() => { if (debug) console.log(head + info("Supression d'un utilisateur résiduel (Informations: "+ req.headers.user +"/"+ req.ip +").")) })
                            .catch(err => console.log(head + error("Erreur: " + err)));
                        }
    
                    }
    
                    var update = new Object();
    
                    update.ip = ip;
                    update.last_login = login_date;
    
                    User.updateMany({steamid64: steamid64}, update)
                    .then(() => { if (debug) console.log(head + warning("Utilisateur modifié (Informations: " + req.headers.user +"/"+ req.ip +").")) })
                    .catch(err => { res.json({success: false, code: "UPDATEUSER", message: "Erreur: " + err})});  
                            
                }
            })
            .catch(err => console.log(head + error("Erreur: " + err)));      

        }
    }

    function Return(req, res) {

        init = new Object();

        Banlist.find({ $or: [ { ip: ip }, { steamid64: steamid64 } ] })
        .then(result => {

            var ActualDate = ((new Date().getTime() * 10000) + 621355968000000000); // - (new Date().getTimezoneOffset() * 600000000);
            
            init.isBanned = false;
            for (entry in result) {
                if (result[entry].bannedUntil > ActualDate) {
                    init.isBanned = true;
                    break;
                } 
            }

            User.find({steamid64: steamid64})
            .then(user => {

                var role = user.map(({ role }) => role).toString();

                if (role)  {
                    init.role = role;
                }

                init.success = true;
                
                res.json(init);

                if (debug) console.log(head + info("Réponse de connexion aux serveurs SCP:SL envoyé (Informations: "+ req.headers.user +"/"+ req.ip +")."));

            })
            .catch(err => console.log({success: false, code: "CHECKUSER", message: "Erreur: " + err}))

        })
        .catch(err => res.json({success: false, code: "CHECKBAN", message: "Erreur: " + err}))

    }

    function app(req, res) {
        UserCheck(req, res);
        Return(req, res);

    }

    app(req, res);
    
});

///////////////////////////////////////////
// Route /api/discord/banlist

Routeur.route("/api/discord/banlist")
.post((req,res) => {

    steamid64 = req.body.steamid64;
    ip = req.body.ip;
    duration = req.body.duration;
    by = req.body.by;
    reason = req.body.reason;

    if (( !steamid64 && !ip ) || !duration || !reason || !by) {
        res.send({message : "Erreur: il manque une entrée."})
    } else {

        var bannedAt = ((new Date().getTime() * 10000) + 621355968000000000);// - (new Date().getTimezoneOffset() * 600000000);
        var bannedUntil = (((new Date().getTime() + (duration * 60000)) * 10000) + 621355968000000000);// - (new Date().getTimezoneOffset() * 600000000);

        var user = new Banlist();

        user.steamid64 = steamid64;
        user.ip = ip;
        user.bannedAt = bannedAt;
        user.bannedUntil = bannedUntil;
        user.by = by;
        user.reason = "BAN DISCORD: " + reason;

        user.save()
        .then(() => res.json({success: true, message: "Le nouveau bannissement a été ajouté avec succès."}))
        .catch(err => res.json({success:false, message: 'Error' + err}));

        if (debug) console.log(head + info("Nouveau bannissement enregistré (Informations: "+ req.headers.user +"/"+ req.ip +")."));
    }
});
Routeur.route("/api/discord/banlist/:user_steamid")
.get((req,res) => { 
    Banlist.find({steamid64: req.params.user_steamid})
    .then(user => res.json(user))
    .catch(err => res.json(err));

    if (debug) console.log(head + info("Information d'un bannissement demandé (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})

///////////////////////////////////////////
// Route /api/discord/lookup

Routeur.route("/api/discord/lookup/:user_arg")
.get((req,res) => { 
    var profile = Object();
    User.find({ $or: [ { ip: req.params.user_arg }, { steamid64: req.params.user_arg } ] })
    .then(user => {

        profile.user = user;

        Banlist.find({ $or: [ { ip: req.params.user_arg }, { steamid64: req.params.user_arg } ] })
        .then(banlist => {
            profile.banlist = banlist;
            
            res.json({success: true, user: profile.user, banlist: profile.banlist})

        })
        .catch(err => res.json(err)); 
    })
    .catch(err => res.json(err));

    if (debug) console.log(head + info("Information d'un joueur demandé (Informations: "+ req.headers.user +"/"+ req.ip +")."));
})

///////////////////////////////////////////

WEBServer.use(Routeur);

///////////////////////////////////////////

async function app() {
    await console.log(head + "Démarrage de l'API en cours.\nCréer par Antt0n (t.me/Antt0n).")
    if (debug) await console.log(head + warning("L'API va démarrer en mode DEBUG.\n") + head + "Voici l'URL de connexion à MongoDB utilisé : " + info(mongoURL) + ".")
    await console.log(head + "Voici le token d'authentification : " + info(accessToken) +".");
    await console.log(head + "Voici la liste des utilisateur pouvant accéder à l'API:")
    for (var pos in usersToken) console.log("- Utilisateur: " + info(pos) + ", Token: " + info(usersToken[pos]) + ".");
    await databaseInit();
    await WEBServer.listen(webserver.port, webserver.hostname, function(){
        console.log(head + "Serveur disponible depuis http://"+ webserver.hostname +":"+webserver.port+"."); 
    });
}

app();