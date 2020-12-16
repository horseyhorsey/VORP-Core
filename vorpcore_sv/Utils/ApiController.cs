﻿using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using vorpcore_sv.Scripts;

namespace vorpcore_sv.Utils
{
    class ApiController : BaseScript
    {
        public delegate Dictionary<string, dynamic> auxDelegate(int source);
        public delegate Dictionary<string, Dictionary<string, dynamic>> getUsersSource();
        public ApiController()
        {
            EventHandlers["vorp:getCharacter"] += new Action<int, dynamic>(getCharacter);
            EventHandlers["vorp:addMoney"] += new Action<int, int, double>(addMoney);
            EventHandlers["vorp:removeMoney"] += new Action<int, int, double>(removeMoney);

            EventHandlers["vorp:addXp"] += new Action<int, int>(addXp);
            EventHandlers["vorp:removeXp"] += new Action<int, int>(removeXp);

            EventHandlers["vorp:setJob"] += new Action<int, string>(setJob);
            EventHandlers["vorp:setGroup"] += new Action<int, string>(setGroup);
            EventHandlers["getCore"] += new Action<CallbackDelegate>((cb) =>
            {
                Dictionary<string, dynamic> corefunctions = new Dictionary<string, dynamic>
                {
                    ["getUser"] = new auxDelegate(getUser),
                    ["maxCharacters"] = LoadConfig.Config["MaxCharacters"].ToObject<int>(),
                    ["addRpcCallback"] = new Action<string, CallbackDelegate>((name, callback) =>
                     {
                         try
                         {
                             LogManager.WriteLog($"Vorp Core: {name} function callback registered!", "success");

                             Callbacks.ServerCallBacks[name] = callback;
                         }
                         catch (Exception e)
                         {
                             Debug.WriteLine(e.Message);
                         }
                     }),
                    ["getUsers"] = new getUsersSource(getConnectedUsers),
                    ["sendLog"] = new Action<string, string>((msg, type) => { LogManager.WriteLog(msg, type); })
                };
                cb.Invoke(corefunctions);
            });
        }
        public Dictionary<string, dynamic> getUser(int source)
        {
            string steam = "steam:" + Players[source].Identifiers["steam"];
            if (LoadUsers._users.ContainsKey(steam))
            {
                return LoadUsers._users[steam].GetUser();
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, Dictionary<string, dynamic>> getConnectedUsers()
        {
            Dictionary<string, Dictionary<string, dynamic>> UsersDictionary = new Dictionary<string, Dictionary<string, dynamic>>();
            foreach(Player player in Players)
            {
                string steam = "steam:"+player.Identifiers["steam"];
                if (LoadUsers._users.ContainsKey(steam) && !UsersDictionary.ContainsKey(steam))
                {
                    UsersDictionary.Add(steam, LoadUsers._users[steam].GetUser());
                }
            }
            return UsersDictionary;
        }

        private void removeMoney(int handle, int typeCash, double quantity)
        {

            Player player = Players[handle];
            string sid = "steam:" + player.Identifiers["steam"];

            if (LoadUsers._users.ContainsKey(sid))
            {
                LoadUsers._users[sid].GetUsedCharacter().removeCurrency(typeCash, quantity);

                JObject nuipost = new JObject();
                nuipost.Add("type", "ui");
                nuipost.Add("action", "update");
                nuipost.Add("moneyquanty", LoadUsers._users[sid].GetUsedCharacter().Money);
                nuipost.Add("goldquanty", LoadUsers._users[sid].GetUsedCharacter().Gold);
                nuipost.Add("rolquanty", LoadUsers._users[sid].GetUsedCharacter().Rol);
                nuipost.Add("xp", LoadUsers._users[sid].GetUsedCharacter().Xp);
                nuipost.Add("serverId", handle);

                player.TriggerEvent("vorp:updateUi", nuipost.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning removeMoney: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }

           

        }

        private void addMoney(int handle, int typeCash, double quantity)
        {
            var player = Players[handle];
            string sid = ("steam:" + player.Identifiers["steam"]);

            if (LoadUsers._users.ContainsKey(sid))
            {
                LoadUsers._users[sid].GetUsedCharacter().addCurrency(typeCash, quantity);

                JObject nuipost = new JObject();
                nuipost.Add("type", "ui");
                nuipost.Add("action", "update");
                nuipost.Add("moneyquanty", LoadUsers._users[sid].GetUsedCharacter().Money);
                nuipost.Add("goldquanty", LoadUsers._users[sid].GetUsedCharacter().Gold);
                nuipost.Add("rolquanty", LoadUsers._users[sid].GetUsedCharacter().Rol);
                nuipost.Add("xp", LoadUsers._users[sid].GetUsedCharacter().Xp);
                nuipost.Add("serverId", handle);

                player.TriggerEvent("vorp:updateUi", nuipost.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning addMoney: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void addXp(int handle, int quantity)
        {

            var player = Players[handle];

            string sid = ("steam:" + player.Identifiers["steam"]);

            if (LoadUsers._users.ContainsKey(sid))
            {
                LoadUsers._users[sid].GetUsedCharacter().addXp(quantity);

                JObject nuipost = new JObject();
                nuipost.Add("type", "ui");
                nuipost.Add("action", "setxp");
                nuipost.Add("xp", LoadUsers._users[sid].GetUsedCharacter().Xp);

                player.TriggerEvent("vorp:updateUi", nuipost.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning addXp: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void removeXp(int handle, int quantity)
        {
            var player = Players[handle];
            string sid = ("steam:" + player.Identifiers["steam"]);

            if (LoadUsers._users.ContainsKey(sid))
            {
                LoadUsers._users[sid].GetUsedCharacter().removeXp(quantity);

                JObject nuipost = new JObject();
                nuipost.Add("type", "ui");
                nuipost.Add("action", "setxp");
                nuipost.Add("xp", LoadUsers._users[sid].GetUsedCharacter().Xp);

                player.TriggerEvent("vorp:updateUi", nuipost.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning removeXp: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void setJob(int handle, string job)
        {
            var player = Players[handle];
            string sid = ("steam:" + player.Identifiers["steam"]);

            if (LoadUsers._users.ContainsKey(sid))
            {
                LoadUsers._users[sid].GetUsedCharacter().setJob(job);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning setJob: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void setGroup(int handle, string group)
        {
            var player = Players[handle];

            string sid = ("steam:" + player.Identifiers["steam"]);

            if (LoadUsers._users.ContainsKey(sid))
            {
                LoadUsers._users[sid].GetUsedCharacter().setGroup(group);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning setGroup: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void getCharacter(int handle, dynamic cb)
        {
            var player = Players[handle];

            string sid = ("steam:" + player.Identifiers["steam"]);

            if (!LoadUsers._users.ContainsKey(sid))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: User not found!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                cb(LoadUsers._users[sid].GetUsedCharacter().getCharacter());
            }
        }
    }
}
