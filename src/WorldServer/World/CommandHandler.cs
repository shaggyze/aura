﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aura.Data;
using Aura.Shared.Const;
using Aura.Shared.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.World.Events;
using Aura.World.Network;
using Aura.World.Player;
using Aura.World.Scripting;
using Aura.World.Util;
using CSScriptLibrary;
using Aura.World.World.Guilds;
using Aura.Shared.World;

namespace Aura.World.World
{
	public class CommandHandler
	{
		// Command definition / loading
		// ------------------------------------------------------------------
		public void Load()
		{
			this.AddCommand("where", Authority.Player, Command_where);
			this.AddCommand("gesture", "<gesture>", Authority.Player, Command_gesture);
			this.AddCommand("cutscene", "[name]", Authority.Player, Command_cutscene);
			this.AddCommand("evgsupport", "<elf|giant|none>", Authority.Player, Command_evgsupport);

			this.AddCommand("go", "<destination>", Authority.VIP, Command_go);
			this.AddCommand("shamala", "<race>", Authority.VIP, Command_shamala);
			this.AddCommand("setrace", "<race>", Authority.VIP, Command_setrace);
			this.AddCommand("guild", "<name>", Authority.VIP, Command_guild);

			this.AddCommand("gmcp", Authority.GameMaster, Command_gmcp);
			this.AddCommand("item", "<id|item_name> [<amount|[color1> <color2> <color3>]]", Authority.GameMaster, Command_item);
			this.AddCommand("iteminfo", "<item name>", Authority.GameMaster, Command_iteminfo);
			this.AddCommand("skillinfo", "<skill name>", Authority.GameMaster, Command_skillinfo);
			this.AddCommand("raceinfo", "<race name>", Authority.GameMaster, Command_raceinfo);
			this.AddCommand("warp", "<region> [x] [y]", Authority.GameMaster, Command_warp);
			this.AddCommand("jump", "<x> [y]", Authority.GameMaster, Command_jump);
			this.AddCommand("clean", Authority.GameMaster, Command_clean);
			this.AddCommand("statuseffect", Authority.GameMaster, Command_statuseffect);
			this.AddCommand("effect", "<id> {(b|i|s:parameter)|me}", Authority.GameMaster, Command_effect);
			this.AddCommand("skill", "<id> [rank=F]", Authority.GameMaster, Command_skill);
			this.AddCommand("spawn", "<race id> [amount]", Authority.GameMaster, Command_spawn);
			this.AddCommand("prop", "<class>", Authority.GameMaster, Command_prop);
			this.AddCommand("ritem", Authority.GameMaster, Command_randomitem);
			this.AddCommand("who", "[region]", Authority.GameMaster, Command_who);
			this.AddCommand("weather", "<clear|cloudy|rain|storm>", Authority.GameMaster, Command_weather);
			this.AddCommand("title", "<title id> <usable>", Authority.GameMaster, Command_title);
			this.AddCommand("grandmaster", "<talent id (0 - 16)>", Authority.GameMaster, Command_grandmaster);
			this.AddCommand("over9000", "", Authority.GameMaster, Command_over9k);
			this.AddCommand("motion", "<category> <motion>", Authority.GameMaster, Command_motion);
			this.AddCommand("broadcast", "<notice>", Authority.GameMaster, Command_broadcast);
			this.AddCommand("inv", "<add|rm> <id> <amount>", Authority.GameMaster, Command_inv);

			this.AddCommand("test", Authority.Admin, Command_test);
			this.AddCommand("reloadscripts", Authority.Admin, Command_reloadscripts);
			this.AddCommand("reloaddata", Authority.Admin, Command_reloaddata);
			this.AddCommand("reloadconf", Authority.Admin, Command_reloadconf);
			this.AddCommand("addcard", "<pet|character> <card id> <character>", Authority.Admin, Command_addcard);

			// Commands issued from the GMCP
			this.AddCommand("set_inventory", "/c [/p:<pocket>]", Authority.GameMaster, Command_set_inventory);

			// Aliases
			this.AddAlias("reloadscripts", "reloadnpcs", "rs");
			this.AddAlias("item", "drop");
			this.AddAlias("iteminfo", "ii");
			this.AddAlias("skillinfo", "si");
			this.AddAlias("raceinfo", "ri");
			this.AddAlias("statuseffect", "se");
			this.AddAlias("test", "t");
			this.AddAlias("broadcast", "broadcastw");

			// Load script commands
			var commandsPath = Path.Combine(WorldConf.ScriptPath, "command");
			var scriptPaths = Directory.GetFiles(commandsPath);
			foreach (var path in scriptPaths)
			{
				try
				{
					this.AddCommand(CSScript.Load(path).CreateObject("*") as Command);
				}
				catch (Exception ex)
				{
					Logger.Exception(ex, "Script: " + path);
					continue;
				}

			}
		}

		// ------------------------------------------------------------------

		public static readonly CommandHandler Instance = new CommandHandler();
		static CommandHandler() { }
		private CommandHandler() { }

		private static Dictionary<string, Command> _commands = new Dictionary<string, Command>();

		public List<string> GetAllCommandsForAuth(byte auth)
		{
			List<string> ret = new List<string>();
			foreach (KeyValuePair<string, Command> kvp in _commands)
			{
				if (kvp.Value.Auth <= auth)
					ret.Add(kvp.Key);
			}
			ret.Sort();
			return ret;
		}

		public void AddCommand(string name, byte authority, CommandFunc func)
		{
			this.AddCommand(name, null, authority, func);
		}

		public void AddCommand(string name, string parameters, byte authority, CommandFunc func)
		{
			this.AddCommand(new Command(name, parameters, authority, func));
		}

		public void AddCommand(Command command)
		{
			_commands.Add(command.Name, command);
		}

		public void AddAlias(string command, params string[] aliases)
		{
			if (!_commands.ContainsKey(command))
				return;

			foreach (var alias in aliases)
				_commands.Add(alias, _commands[command]);
		}

		/// <summary>
		/// Returns true if a command was used.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature"></param>
		/// <param name="args"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		public bool Handle(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var command = _commands.FirstOrDefault(a => a.Key == args[0]).Value;
			if (command != null)
			{
				if (client.Account.Authority >= command.Auth)
				{
					try
					{
						var result = command.Func(client, creature, args, msg);
						if (result == CommandResult.WrongParameter)
						{
							Send.ServerMessage(client, creature, Localization.Get("gm.usage"), args[0], command.Parameters); // Usage: {0} {1}
						}

						if (result == CommandResult.Okay && WorldConf.CommandLogger)
						{
							ChatLogger.Command(creature.Name, args[0].Substring(1), msg);
						}
					}
					catch (Exception ex)
					{
						Send.ServerMessage(client, creature, Localization.Get("gm.error")); // Error while executing command.
						Logger.Exception(ex, "Unable to execute command '" + args[0] + "'. Message: '" + msg + "'", true);
					}

					return true;
				}

				Send.ServerMessage(client, creature, Localization.Get("gm.unknown")); // Unknown command.
				return true;
			}

			return false;
		}

		// Commands
		// ------------------------------------------------------------------

		private CommandResult Command_where(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var pos = creature.GetPosition();
			var area = MabiData.RegionInfoDb.GetAreaId(creature.Region, pos.X, pos.Y);
			Send.ServerMessage(client, creature, Localization.Get("gm.where"), creature.Region, pos.X, pos.Y, area, creature.Direction); // Region: {0}, X: {1}, Y: {2}, Area: {3}, Direction: {4}

			return CommandResult.Okay;
		}

		private CommandResult Command_gmcp(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (client.Account.Authority < WorldConf.MinimumGMCP)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.gmcp_auth")); // You're not authorized to use the GMCP.
				return CommandResult.Fail;
			}

			client.Send(new MabiPacket(Op.GMCPOpen, creature.Id));
			//Send.ServerMessage(client, creature, Localization.Get("gm.gmcp_disabled"))); // The GMCP is currently disabled or not available.

			return CommandResult.Okay;
		}

		private CommandResult Command_who(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			uint region = 0;
			if (args.Length > 1 && !uint.TryParse(args[1], out region))
				return CommandResult.WrongParameter;

			Send.ServerMessage(client, creature, Localization.Get(region == 0 ? "gm.who" : "gm.who_region")); // Players online[ in region {0}]:

			var players = (region == 0 ? WorldManager.Instance.GetAllPlayers() : WorldManager.Instance.GetAllPlayersInRegion(region));
			if (players.Count() > 0)
			{
				foreach (var player in players)
				{
					var pos = player.GetPosition();
					Send.ServerMessage(client, creature, Localization.Get("gm.who_r"), player.Name, player.Region, pos.X, pos.Y); // {0} - Region: {1}, X: {2}, Y: {3}
				}
			}
			else
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.who_none")); // None.
			}

			return CommandResult.Okay;
		}

		private CommandResult Command_randomitem(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var rand = RandomProvider.Get();

			var itemInfo = MabiData.ItemDb.Entries.ElementAt(rand.Next(MabiData.ItemDb.Entries.Count)).Value;
			var color1 = "0x" + (rand.NextDouble() < 0.5 ? rand.Next(0, 255) : 0).ToString("X2") + rand.Next(0, 255).ToString("X2") + rand.Next(0, 255).ToString("X2") + rand.Next(0, 255).ToString("X2");
			var color2 = "0x" + (rand.NextDouble() < 0.5 ? rand.Next(0, 255) : 0).ToString("X2") + rand.Next(0, 255).ToString("X2") + rand.Next(0, 255).ToString("X2") + rand.Next(0, 255).ToString("X2");
			var color3 = "0x" + (rand.NextDouble() < 0.5 ? rand.Next(0, 255) : 0).ToString("X2") + rand.Next(0, 255).ToString("X2") + rand.Next(0, 255).ToString("X2") + rand.Next(0, 255).ToString("X2");
			var cmd = new string[] { "drop", itemInfo.Id.ToString(), color1, color2, color3 };

			return Command_item(client, creature, cmd, msg);
		}

		private CommandResult Command_item(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2 || args.Length == 4 || args.Length > 5)
				return CommandResult.WrongParameter;

			uint itemId = 0;
			if (!uint.TryParse(args[1], out itemId))
			{
				// Couldn't parse uint, must be a spawn by name

				var itemName = args[1].Replace('_', ' ');

				// Search exact name first
				var newItemInfo = MabiData.ItemDb.Find(itemName);
				if (newItemInfo == null)
				{
					// Not found? Look for the next best thing.
					var itemInfo = MabiData.ItemDb.FindAll(itemName);
					if (itemInfo.Count < 1)
					{
						Send.ServerMessage(client, creature, Localization.Get("gm.item_nores"), itemName); // Item '{0}' not found in database.
						return CommandResult.Fail;
					}

					// Find best match
					int score = 10000;
					foreach (var ii in itemInfo)
					{
						int iScore = ii.Name.LevenshteinDistance(itemName);
						if (iScore < score)
						{
							score = iScore;
							newItemInfo = ii;
						}
					}

					Send.ServerMessage(client, creature, Localization.Get("gm.item_mures"), itemName, newItemInfo.Name); // Item '{0}' not found, using next best result, '{1}'.
				}

				itemId = newItemInfo.Id;
			}

			var info = MabiData.ItemDb.Find(itemId);
			if (info == null)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.item_nores"), itemId); // Item '{0}' not found in database.
				return CommandResult.Fail;
			}

			var newItem = new MabiItem(itemId);
			newItem.Info.Amount = 1;

			if (args.Length == 3)
			{
				// command id amount

				if (!ushort.TryParse(args[2], out newItem.Info.Amount))
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.item_amount")); // Invalid amount.
					return CommandResult.Fail;
				}

				// Just in case
				if (newItem.Info.Amount > newItem.StackMax)
					newItem.Info.Amount = newItem.StackMax;
				if (newItem.StackType == BundleType.None)
					newItem.Info.Amount = 1;
			}
			else if (args.Length == 5)
			{
				// command id color1-3

				uint[] color = { 0, 0, 0 };

				for (int i = 0; i < 3; ++i)
				{
					if (args[i + 2].StartsWith("0x"))
					{
						// Color in hex
						if (!uint.TryParse(args[i + 2].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color[i]))
						{
							Send.ServerMessage(client, creature, Localization.Get("gm.item_hex"), args[i + 2]); // Invalid hex color '{0}'.
							return CommandResult.Fail;
						}
					}
					else if (uint.TryParse(args[i + 2], out color[i]))
					{
						// Mabi color
						color[i] += 0x10000000;
					}
					else
					{
						// Color by name
						switch (args[i + 2])
						{
							case "black": color[i] = 0xFF000000; break;
							case "white": color[i] = 0xFFFFFFFF; break;
							default:
								Send.ServerMessage(client, creature, Localization.Get("gm.item_color"), args[i + 2]); // Unknown color '{0}'.
								return CommandResult.Fail;
						}
					}
				}

				newItem.Info.ColorA = color[0];
				newItem.Info.ColorB = color[1];
				newItem.Info.ColorC = color[2];
			}

			if (args[0] == "drop")
			{
				// >drop
				var pos = client.Character.GetPosition();
				WorldManager.Instance.DropItem(newItem, client.Character.Region, (uint)pos.X, (uint)pos.Y);
			}
			else
			{
				// >item

				//newItem.Info.Pocket = (byte)Pocket.Temporary;
				//creature.Items.Add(newItem);
				creature.Inventory.Add(newItem, Pocket.Temporary);

				//Send.ItemInfo(client, creature, newItem);
			}

			return CommandResult.Okay;
		}

		private CommandResult Command_iteminfo(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var itemName = msg.Substring(args[0].Length + 2).Replace('_', ' ');

			var infos = MabiData.ItemDb.FindAll(itemName);
			if (infos.Count < 1)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.ii_none")); // No items found.
				return CommandResult.Fail;
			}

			for (int i = 0; i < infos.Count && i < 20; ++i)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.ii_for"), infos[i].Id, infos[i].Name); // {0}: {1}
			}

			Send.ServerMessage(client, creature, Localization.Get("gm.ii_res"), infos.Count); // Results: {0} (Max. 20 shown)

			return CommandResult.Okay;
		}

		private CommandResult Command_skillinfo(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var skillName = msg.Substring(args[0].Length + 2).Replace('_', ' ');

			var infos = MabiData.SkillDb.FindAll(skillName);
			if (infos.Count < 1)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.si_none")); // No skills found.
				return CommandResult.Fail;
			}

			for (int i = 0; i < infos.Count && i < 20; ++i)
			{
				var lastRank = infos[i].RankInfo.Last();
				Send.ServerMessage(client, creature, Localization.Get("gm.si_for"), infos[i].Id, infos[i].Name, (16 - lastRank.Rank).ToString("X")); // {0}: {1} (Max Rank: {2})
			}

			Send.ServerMessage(client, creature, Localization.Get("gm.si_res"), infos.Count); // Results: {0} (Max. 20 shown)

			return CommandResult.Okay;
		}

		private CommandResult Command_raceinfo(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var monsterName = msg.Substring(args[0].Length + 2).Replace('_', ' ');

			var infos = MabiData.RaceDb.FindAll(monsterName);
			if (infos.Count < 1)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.ri_none")); // No races found.
				return CommandResult.Fail;
			}

			for (int i = 0; i < infos.Count && i < 20; ++i)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.ri_for"), infos[i].Id, infos[i].Name); // {0}: {1}

				var drops = "";
				if (infos[i].Drops.Count < 1)
					drops = Localization.Get("gm.ri_no_drop"); // None.
				else
				{
					foreach (var drop in infos[i].Drops)
					{
						drops += string.Format(Localization.Get("gm.ri_for_drop"), drop.ItemId, (drop.Chance * 100));
					}
				}
				Send.ServerMessage(client, creature, Localization.Get("gm.ri_drops") + " " + drops.TrimEnd(','));
				Send.ServerMessage(client, creature, "----------");
			}

			Send.ServerMessage(client, creature, Localization.Get("gm.ri_res"), infos.Count); // Results: {0} (Max. 20 shown)

			return CommandResult.Okay;
		}

		private CommandResult Command_spawn(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			uint raceId = 0;
			if (!uint.TryParse(args[1], out raceId))
				return CommandResult.Fail;

			byte amount = 1;
			if (args.Length > 2)
			{
				if (!byte.TryParse(args[2], out amount))
					return CommandResult.Fail;

				if (amount < 1)
					amount = 1;
				else if (amount > 100)
					amount = 100;
			}

			uint radius = 300;
			radius += (uint)(amount / 10 * 50);

			WorldManager.Instance.SpawnCreature(raceId, amount, creature.Region, creature.GetPosition(), radius, true);

			return CommandResult.Okay;
		}

		private CommandResult Command_prop(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			uint propClass = 0;
			if (!uint.TryParse(args[1], out propClass))
				return CommandResult.Fail;

			var pos = creature.GetPosition();
			var area = MabiData.RegionInfoDb.GetAreaId(creature.Region, pos.X, pos.Y);

			var prop = new MabiProp(propClass, creature.Region, pos.X, pos.Y, creature.Direction);
			WorldManager.Instance.AddProp(prop);

			return CommandResult.Okay;
		}

		private CommandResult Command_set_inventory(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			if (args[1] != "/c")
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.si_param"), args[1]); // Unknown paramter '{0}'.
				return CommandResult.Fail;
			}

			byte pocket = 2;
			if (args.Length >= 3)
			{
				var match = Regex.Match(args[2], "/p:(?<id>[0-9]+)");
				if (!match.Success)
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.si_param_pocket"), args[2]); // Unknown paramter '{0}', please specify a pocket.
					return CommandResult.Fail;
				}
				if (!byte.TryParse(match.Groups["id"].Value, out pocket) || pocket > (byte)Pocket.Max - 1)
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.si_pocket")); // Invalid pocket.
					return CommandResult.Fail;
				}
			}

			var toRemove = new List<MabiItem>();
			foreach (var item in creature.Inventory.Items)
			{
				if (item.Info.Pocket == pocket)
					toRemove.Add(item);
			}
			foreach (var item in toRemove)
			{
				creature.Inventory.Remove(item);
			}

			Send.ServerMessage(client, creature, Localization.Get("gm.si_cleared"), ((Pocket)pocket), toRemove.Count); // Cleared pocket '{0}'. (Deleted items: {1})

			return CommandResult.Okay;
		}

		/// <summary>
		/// Warps creature to the specified region and coordinates.
		/// If coordinates are ommited, a random location will be used.
		/// </summary>
		private CommandResult Command_warp(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var pos = creature.GetPosition();

			uint region = 0;
			if (!uint.TryParse(args[1], out region))
			{
				var mapInfo = MabiData.RegionDb.Find(args[1]);
				if (mapInfo != null)
					region = mapInfo.Id;
				else
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.warp_region"), args[1]); // Region '{0}' not found.
					return CommandResult.Fail;
				}
			}

			uint x = pos.X, y = pos.Y;
			if (args.Length > 2 && !uint.TryParse(args[2], out x))
				return CommandResult.WrongParameter;
			if (args.Length > 3 && !uint.TryParse(args[3], out y))
				return CommandResult.WrongParameter;
			if (args.Length == 2)
			{
				// Random coordinates.
				var rndc = MabiData.RegionInfoDb.RandomCoord(region);
				x = rndc.X;
				y = rndc.Y;
			}

			client.Warp(region, x, y);

			return CommandResult.Okay;
		}

		/// <summary>
		/// Warps creature to the specified coordinates in the current region.
		/// If coordinates are ommited, a random location will be used.
		/// </summary>
		private CommandResult Command_jump(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var pos = creature.GetPosition();
			uint region = creature.Region;

			uint x = pos.X, y = pos.Y;
			if (args.Length > 1 && !uint.TryParse(args[1], out x))
				return CommandResult.WrongParameter;
			if (args.Length > 2 && !uint.TryParse(args[2], out y))
				return CommandResult.WrongParameter;
			if (args.Length == 1)
			{
				// Random coordinates.
				var rndc = MabiData.RegionInfoDb.RandomCoord(region);
				x = rndc.X;
				y = rndc.Y;
			}

			client.Warp(region, x, y);

			return CommandResult.Okay;
		}

		private CommandResult Command_go(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
			{
				Send.ServerMessage(client, creature,
					Localization.Get("gm.go_dest") + // Destinations:
					" Tir Chonaill, Dugald Isle, Dunbarton, Gairech, Bangor, Emain Macha, Taillteann, Nekojima, GM Island"
				);
				return CommandResult.WrongParameter;
			}

			uint region = 0, x = 0, y = 0;
			var destination = msg.Substring(args[0].Length + 1).Trim();

			if (destination.StartsWith("tir")) { region = 1; x = 12991; y = 38549; }
			else if (destination.StartsWith("dugald")) { region = 16; x = 23017; y = 61244; }
			else if (destination.StartsWith("dun")) { region = 14; x = 38001; y = 38802; }
			else if (destination.StartsWith("gairech")) { region = 30; x = 39295; y = 53279; }
			else if (destination.StartsWith("bangor")) { region = 31; x = 12904; y = 12200; }
			else if (destination.StartsWith("emain")) { region = 52; x = 39818; y = 41621; }
			else if (destination.StartsWith("tail")) { region = 300; x = 212749; y = 192720; }
			else if (destination.StartsWith("neko")) { region = 600; x = 114430; y = 79085; }
			else if (destination.StartsWith("gm")) { region = 22; x = 2500; y = 2500; }
			else
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.go_unk")); // Unknown destination.
				return CommandResult.Fail;
			}

			client.Warp(region, x, y);

			return CommandResult.Okay;
		}

		private CommandResult Command_motion(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 3)
				return CommandResult.WrongParameter;

			ushort category = 0, type = 0;
			if (!ushort.TryParse(args[1], out category) || !ushort.TryParse(args[2], out type))
				return CommandResult.WrongParameter;

			Send.UseMotion(creature, category, type);

			return CommandResult.Okay;
		}

		private CommandResult Command_gesture(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var info = MabiData.MotionDb.Find(args[1]);
			if (info == null)
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.gesture_unk")); // Unknown gesture.
				return CommandResult.Fail;
			}

			Send.UseMotion(creature, info.Category, info.Type);

			return CommandResult.Okay;
		}

		private CommandResult Command_setrace(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			ushort raceId = 0;
			if (!ushort.TryParse(args[1], out raceId))
				return CommandResult.Fail;

			creature.Race = raceId;

			Send.ServerMessage(client, creature, Localization.Get("gm.setrace")); // Your race has been changed. You'll be logged out to complete the process.

			client.Disconnect(3);

			return CommandResult.Okay;
		}

		private CommandResult Command_clean(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var items = WorldManager.Instance.GetItemsInRegion(creature.Region);
			foreach (var item in items)
			{
				WorldManager.Instance.RemoveItem(item);
			}

			return CommandResult.Okay;
		}

		private CommandResult Command_statuseffect(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			ulong[] val = new ulong[5] { 0, 0, 0, 0, 0 };
			MabiCreature target = creature;
			int hasTarget = 0;

			if (args.Length > 1 && !ulong.TryParse(args[1], out val[0]))
			{
				target = WorldManager.Instance.GetCharacterOrNpcByName(args[1]);
				if (target == null)
				{
					Send.ServerMessage(client, creature,
						"Target character not found or failed to convert first parameter.");
					return CommandResult.Okay; //Okay since we do a more specific error message.
				}
				hasTarget = 1;
			}

			for (int i = 1; i <= 5; ++i)
				if (args.Length > (i + hasTarget) && !ulong.TryParse(
					args[i + hasTarget], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val[i - 1]))
				{
					Send.ServerMessage(client, creature,
						"Failed to convert parameter.\nMake sure parameter is a numerical value.");
					return CommandResult.Okay; //Okay since we do a more specific error message.
				}

			target.Conditions.A = (CreatureConditionA)val[0];
			target.Conditions.B = (CreatureConditionB)val[1];
			target.Conditions.C = (CreatureConditionC)val[2];
			target.Conditions.D = (CreatureConditionD)val[3];
			//target.Conditions.E = (CreatureConditionE)val[4];

			Send.StatusEffectUpdate(target);

			return CommandResult.Okay;
		}

		private CommandResult Command_effect(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var p = new MabiPacket(Op.Effect, creature.Id);

			uint id;
			if (!uint.TryParse(args[1], out id))
				return CommandResult.WrongParameter;

			p.PutInt(id);

			for (int i = 2; i < args.Length; ++i)
			{
				var splitted = args[i].Split(':');

				if (splitted[0] == "me")
				{
					p.PutLong(creature.Id);
					continue;
				}

				if (splitted.Length < 2)
					continue;

				splitted[0] = splitted[0].Trim();
				splitted[1] = splitted[1].Trim();

				switch (splitted[0])
				{
					case "b":
						{
							byte val;
							if (!byte.TryParse(splitted[1], out val))
								return CommandResult.WrongParameter;
							p.PutByte(val);
							break;
						}
					case "i":
						{
							uint val;
							if (!uint.TryParse(splitted[1], out val))
								return CommandResult.WrongParameter;
							p.PutInt(val);
							break;
						}
					case "s":
						{
							p.PutString(splitted[1]);
							break;
						}
				}
			}

			WorldManager.Instance.Broadcast(p, SendTargets.Range, creature);

			return CommandResult.Okay;
		}

		private CommandResult Command_test(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			Send.ServerMessage(client, creature, "Nothing to see here, move along.");

			return CommandResult.Okay;
		}

		private CommandResult Command_skill(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			ushort skillId = 0;
			if (!ushort.TryParse(args[1], out skillId))
				return CommandResult.Fail;

			byte rank = 0xF;
			if (args.Length > 2 && !byte.TryParse(args[2], NumberStyles.HexNumber, null, out rank))
				return CommandResult.WrongParameter;

			if (rank >= 1 && rank <= 15)
				rank = (byte)(16 - rank);
			else if (rank >= 0x16 && rank <= 0x18)
				rank -= 6;
			else
				return CommandResult.WrongParameter;

			creature.Skills.Give((SkillConst)skillId, (SkillRank)rank);

			WorldManager.Instance.CreatureStatsUpdate(creature);

			return CommandResult.Okay;
		}

		// Due to appdomains, the server must be restarted to really reload NPCs.
		// Loading NPCs into a new appdomain would incur a large performance hit.
		// However, we still need to be able to reload NPCs. So we'll just add
		// new assemblies in, even though this causes a memory leak over time.
		// -- Xcelled
		private CommandResult Command_reloadscripts(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			Send.ServerMessage(client, creature, Localization.Get("gm.reloadscripts")); // Reloading NPCs...

			ServerUtil.LoadData(WorldConf.DataPath, DataLoad.Npcs, true);

			MabiData.QuestDb.Entries.Clear();
			WorldManager.Instance.RemoveAllNPCs();
			ScriptManager.Instance.LoadScripts();
			ScriptManager.Instance.LoadSpawns();

			Send.ServerMessage(client, creature, Localization.Get("gm.reload_done")); // done.

			return CommandResult.Okay;
		}

		private CommandResult Command_reloaddata(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			Send.ServerMessage(client, creature, Localization.Get("gm.reloaddata")); // Reloading data...

			ServerUtil.LoadData(WorldConf.DataPath, DataLoad.All, true);

			this.Command_reloadscripts(client, creature, null, null);

			return CommandResult.Okay;
		}

		private CommandResult Command_reloadconf(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			Send.ServerMessage(client, creature, Localization.Get("gm.reloadconf")); // Reloading conf...

			WorldConf.Load(null);

			Send.ServerMessage(client, creature, Localization.Get("gm.reload_done")); // done.

			return CommandResult.Okay;
		}

		private CommandResult Command_addcard(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 3)
				return CommandResult.WrongParameter;

			uint cardId = 0;
			if (!uint.TryParse(args[2], out cardId))
			{
				Send.ServerMessage(client, creature, Localization.Get("gm.addcard_id")); // Invalid card id.
				return CommandResult.Fail;
			}

			MabiCreature target = null;
			if (args.Length < 4)
				target = creature;
			else
			{
				var characterName = args[3];

				target = WorldManager.Instance.GetCharacterByName(characterName);
				if (target == null)
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.addcard_char"), characterName); // Character '{0}' not found.
					return CommandResult.Fail;
				}
			}

			if (target != null)
			{
				var type = args[1];

				uint race = 0;
				if (type == "pet")
				{
					race = cardId;
					cardId = Id.PetCardType;

					if (!MabiData.PetDb.Has(race))
					{
						Send.ServerMessage(client, creature, Localization.Get("gm.addcard_petcard"), race); // Unknown pet card ({0}).
						return CommandResult.Fail;
					}
				}
				else if (type != "character")
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.addcard_type")); // Invalid card type.
					return CommandResult.WrongParameter;
				}

				MabiDb.Instance.AddCard((target.Client as WorldClient).Account.Name, cardId, race);

				Send.ServerMessage(client, creature, Localization.Get("gm.addcard")); // Card added.
				return CommandResult.Okay;
			}

			return CommandResult.Fail;
		}

		private CommandResult Command_guild(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length != 2)
				return CommandResult.WrongParameter;

			GuildManager.CreateGuild(args[1], GuildType.Adventure, creature, new MabiCreature[] { });

			return CommandResult.Okay;
		}

		private CommandResult Command_shamala(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			uint race = 0;
			if (args.Length > 1 && !uint.TryParse(args[1], out race))
				return CommandResult.WrongParameter;

			if (race > 0)
			{
				var raceInfo = MabiData.RaceDb.Find(race);
				if (raceInfo == null)
				{
					Send.ServerMessage(client, creature, Localization.Get("gm.shamala_race")); // Race not found.
					return CommandResult.Fail;
				}

				creature.ShamalaRace = raceInfo;
				creature.Shamala = new ShamalaInfo() { Id = 1 };

				WorldManager.Instance.Broadcast(new MabiPacket(Op.ShamalaTransformation, creature.Id)
					.PutByte(1)                        // Sucess
					.PutInt(creature.Shamala.Id)
					.PutByte(1)                        // Show transformation effect
					.PutInt(creature.ShamalaRace.Id)
					.PutFloat(creature.Shamala.Size)
					.PutInt(creature.Shamala.Color1)
					.PutInt(creature.Shamala.Color2)
					.PutInt(creature.Shamala.Color3)
					.PutByte(0)
					.PutByte(0)
				, SendTargets.Range, creature);
				Send.ServerMessage(client, creature, Localization.Get("gm.shamala_trans")); // Transform~!
			}
			else if (creature.Shamala != null)
			{
				creature.Shamala = null;
				creature.ShamalaRace = null;

				WorldManager.Instance.Broadcast(new MabiPacket(Op.ShamalaTransformationEndR, creature.Id).PutBytes(1, 1), SendTargets.Range, creature);
				Send.ServerMessage(client, creature, Localization.Get("gm.shamala_end")); // Transformation ended.
			}
			else
			{
				return CommandResult.WrongParameter;
			}


			return CommandResult.Okay;
		}

		private CommandResult Command_weather(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			float weather = 0;
			switch (args[1])
			{
				case "clear": weather = 0.5f; break;
				case "cloudy": weather = 1.2f; break;
				case "rain": weather = 1.95f; break;
				case "storm": weather = 2f; break;
				default:
					return CommandResult.WrongParameter;
			}

			WeatherManager.Instance.SetWeather(creature.Region, weather);

			return CommandResult.Okay;
		}

		private CommandResult Command_title(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 3)
				return CommandResult.WrongParameter;

			var character = creature as MabiPC;
			var title = ushort.Parse(args[1]);
			var usable = bool.Parse(args[2]);

			character.GiveTitle(title, usable);
			return CommandResult.Okay;
		}

		private CommandResult Command_cutscene(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var name = "JG_nekojima_arrival";
			if (args.Length > 1)
				name = args[1];

			name = name.Replace(@"\_", " ");

			MabiCutscene scene = new MabiCutscene(creature, name);

			scene.AddActor("me", creature);

			scene.Play(client);

			return CommandResult.Okay;
		}

		private CommandResult Command_grandmaster(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var id = (TalentId)byte.Parse(args[1]);

			creature.Talents.Grandmaster = id;

			creature.Talents.SendUpdate();

			return CommandResult.Okay;
		}

		private CommandResult Command_over9k(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			var skills = Enum.GetValues(typeof(SkillConst));

			foreach (var skill in skills)
				creature.Skills.Give((SkillConst)skill, SkillRank.R1);

			WorldManager.Instance.CreatureStatsUpdate(creature);

			Send.ChannelNotice(NoticeType.TopRed, 20000, "{0}'S POWER IS OVER NINE THOUSAAAND!!!", creature.Name.ToUpper());

			return CommandResult.Okay;
		}

		private CommandResult Command_evgsupport(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			switch (args[1].ToLower().Trim())
			{
				case "elf":
					creature.EvGSupportRace = 1;
					break;

				case "giant":
					creature.EvGSupportRace = 2;
					break;

				case "none":
					creature.EvGSupportRace = 0;
					break;

				default:
					return CommandResult.WrongParameter;
			}

			Send.PvPInformation(creature);

			return CommandResult.Okay;
		}

		private CommandResult Command_broadcast(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 2)
				return CommandResult.WrongParameter;

			var notice = string.Join(" ", args, 1, args.Length - 1);

			if (args[1].EndsWith("w"))
				Send.WorldNotice(NoticeType.TopRed, 20000, notice);
			else
				Send.ChannelNotice(NoticeType.TopRed, 20000, notice);

			return CommandResult.Okay;
		}

		private CommandResult Command_inv(WorldClient client, MabiCreature creature, string[] args, string msg)
		{
			if (args.Length < 4)
				return CommandResult.WrongParameter;

			uint id, amount;
			if (!uint.TryParse(args[2], out id))
				return CommandResult.WrongParameter;
			if (!uint.TryParse(args[3], out amount))
				return CommandResult.WrongParameter;

			if (args[1] == "add")
				creature.Inventory.Add(uint.Parse(args[2]), uint.Parse(args[3]));
			else if (args[1] == "rm")
				creature.Inventory.Remove(uint.Parse(args[2]), uint.Parse(args[3]));
			else if (args[1] == "dbg")
				creature.Inventory.Debug();
			else
				return CommandResult.WrongParameter;

			return CommandResult.Okay;
		}
	}

	public enum CommandResult { Okay, WrongParameter, Fail }

	public delegate CommandResult CommandFunc(WorldClient client, MabiCreature creature, string[] args, string msg);

	public class Command
	{
		public string Name, Parameters;
		public byte Auth;
		public CommandFunc Func;

		public Command() { }

		public Command(string name, string parameters, byte authority, CommandFunc func)
		{
			this.Name = name;
			this.Parameters = parameters;
			this.Auth = authority;
			this.Func = func;
		}
	}
}
