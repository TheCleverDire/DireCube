﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Collections.Generic;
using ClassicalSharp.Entities;

namespace ClassicalSharp.Commands
{

    /// <summary> Represents a client side action that optionally accepts arguments. </summary>
    public abstract class Command
    {
        public string Name;
        public string[] Help;
        public bool SingleplayerOnly;
        protected internal Game game;

        public abstract void Execute(string[] args);
    }

    public class CommandList : IGameComponent
    {

        const string prefix = "/client", prefixSpace = "/client ";

        public bool IsCommandPrefix(string input)
        {
            if (game.Server.IsSinglePlayer && Utils.CaselessStarts(input, "/"))
                return true;

            return Utils.CaselessStarts(input, prefixSpace)
                   || Utils.CaselessEq(input, prefix);
        }

        public bool IsCommandHackPrefix(string input)
        {
            return input.StartsWith(".");
        }

        protected Game game;
        public List<Command> RegisteredCommands = new List<Command>();

        void IGameComponent.Init(Game game)
        {
            this.game = game;
            Register(new GpuInfoCommand());
            Register(new HelpCommand());
            Register(new HacksCommand());
            Register(new RenderTypeCommand());
            Register(new ResolutionCommand());
            Register(new ModelCommand());
            Register(new CuboidCommand());
            Register(new TeleportCommand());
        }

        void IGameComponent.Ready(Game game)
        {
        }

        void IGameComponent.Reset(Game game)
        {
        }

        void IGameComponent.OnNewMap(Game game)
        {
        }

        void IGameComponent.OnNewMapLoaded(Game game)
        {
        }

        public void Register(Command command)
        {
            command.game = game;
            RegisteredCommands.Add(command);
        }

        public Command GetMatch(string cmdName)
        {
            Command match = null;
            for (int i = 0; i < RegisteredCommands.Count; i++)
            {
                Command cmd = RegisteredCommands[i];
                if (!Utils.CaselessStarts(cmd.Name, cmdName)) continue;

                if (match != null)
                {
                    game.Chat.Add("&e/client: Multiple commands found that start with: \"&f" + cmdName + "&e\".");
                    return null;
                }

                match = cmd;
            }

            if (match == null)
            {
                game.Chat.Add("&e/client: Unrecognised command: \"&f" + cmdName + "&e\".");
                game.Chat.Add("&e/client: Type &a/client &efor a list of commands.");
                return null;
            }

            if (match.SingleplayerOnly && !game.Server.IsSinglePlayer)
            {
                game.Chat.Add("&e/client: \"&f" + cmdName + "&e\" can only be used in singleplayer.");
                return null;
            }

            return match;
        }

        static char[] splitChar = new char[] {' '};

        public void Execute(string text)
        {
            if (Utils.CaselessStarts(text, prefixSpace))
            {
                // /client command args
                text = text.Substring(prefixSpace.Length);
            }
            else if (Utils.CaselessStarts(text, prefix))
            {
                // /clientcommand args
                text = text.Substring(prefix.Length);
            }
            else
            {
                // /command args
                text = text.Substring(1);
            }

            if (text.Length == 0)
            {
                // only / or /client
                game.Chat.Add("&eList of client commands:");
                PrintDefinedCommands(game);
                game.Chat.Add("&eTo see help for a command, type &a/client help [cmd name]");
                return;
            }

            string[] args = text.Split(splitChar);
            Command cmd = GetMatch(args[0]);
            if (cmd == null) return;
            cmd.Execute(args);
        }

        public void PrintDefinedCommands(Game game)
        {
            StringBuffer sb = new StringBuffer(Utils.StringLength);

            for (int i = 0; i < RegisteredCommands.Count; i++)
            {
                Command cmd = RegisteredCommands[i];
                string name = cmd.Name;

                if ((sb.Length + name.Length + 2) > sb.Capacity)
                {
                    game.Chat.Add(sb.ToString());
                    sb.Clear();
                }

                sb.Append(name);
                sb.Append(", ");
            }

            if (sb.Length > 0)
                game.Chat.Add(sb.ToString());
        }

        void IDisposable.Dispose()
        {
            RegisteredCommands.Clear();
        }
    }
}