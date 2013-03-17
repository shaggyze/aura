﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using Aura.Data;
using Aura.Shared.Util;
using Aura.World.Network;
using Aura.World.Util;
using Aura.World.World;
using Aura.World.Events;

namespace Aura.World.Scripting
{
	public partial class BaseScript : IDisposable
	{
		public string ScriptPath { get; set; }
		public string ScriptName { get; set; }

		public bool Disposed { get; protected set; }

		public virtual void OnLoad()
		{
		}

		public virtual void OnLoadDone()
		{
		}

		/// <inheritdoc/>
		/// <summary>
		/// Cleans up after the NPC (In case of reloading)
		/// Every derived class should call base.Dispose()
		/// </summary>
		public virtual void Dispose()
		{
			this.Disposed = true;
		}

		// Built in methods
		// ------------------------------------------------------------------

		/// <summary>
		/// Shortcut for drops, using region names.
		/// </summary>
		protected void SpawnProp(uint propClass, string region, uint x, uint y, uint area, float scale, float direction, PropAction action, uint dropType)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			this.SpawnProp(propClass, regionId, x, y, area, scale, direction, action, dropType);
		}

		/// <summary>
		/// Shortcut for drops, using region ids.
		/// </summary>
		protected void SpawnProp(uint propClass, uint region, uint x, uint y, uint area, float scale, float direction, PropAction action, uint dropType)
		{
			var prop = this.SpawnProp(propClass, region, x, y, area, scale, direction);
			this.DefineProp(prop.Id, region, x, y, action, dropType);
		}

		/// <summary>
		/// Shortcut for warps, using region names.
		/// </summary>
		protected void SpawnProp(uint propClass, string region, uint x, uint y, uint area, float scale, float direction, PropAction action, string tregion, uint tx, uint ty)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			uint tregionId = MabiData.MapDb.TryGetRegionId(tregion);
			this.SpawnProp(propClass, regionId, x, y, area, scale, direction, action, tregionId, tx, ty);
		}

		/// <summary>
		/// Shortcut for warps, using region ids.
		/// </summary>
		protected void SpawnProp(uint propClass, uint region, uint x, uint y, uint area, float scale, float direction, PropAction action, uint tregion, uint tx, uint ty)
		{
			MabiPropFunc behavior = (client, creature, prop) => { client.Warp(tregion, tx, ty); };
			this.SpawnProp(propClass, region, x, y, area, scale, direction, behavior);
		}

		/// <summary>
		/// Spawns prop with the specified behavior, using the region name.
		/// </summary>
		protected void SpawnProp(uint propClass, string region, uint x, uint y, uint area, float scale, float direction, MabiPropFunc behavior)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			this.SpawnProp(propClass, regionId, x, y, area, scale, direction, behavior);
		}

		/// <summary>
		/// Spawns prop with the specified behavior, using the region id.
		/// </summary>
		protected void SpawnProp(uint propClass, uint region, uint x, uint y, uint area, float scale, float direction, MabiPropFunc behavior)
		{
			var prop = this.SpawnProp(propClass, region, x, y, area, scale, direction);
			this.DefineProp(prop, behavior);
		}

		/// <summary>
		/// Simple prop spawning without behavior, using region name.
		/// Without "area" props are not hit or touchable.
		/// </summary>
		/// <returns>New prop</returns>
		protected MabiProp SpawnProp(uint propClass, string region, uint x, uint y, uint area = 0, float scale = 1f, float direction = 1f)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			return this.SpawnProp(propClass, regionId, x, y, area, scale, direction);
		}

		/// <summary>
		/// Simple prop spawning without behavior.
		/// Without "area" props are not hit or touchable.
		/// </summary>
		/// <returns>New prop</returns>
		protected MabiProp SpawnProp(uint propClass, uint region, uint x, uint y, uint area = 0, float scale = 1f, float direction = 1f)
		{
			var prop = new MabiProp(region, area);
			prop.Info.Class = propClass;
			prop.Info.X = x;
			prop.Info.Y = y;
			prop.Info.Scale = scale;
			prop.Info.Direction = direction;

			WorldManager.Instance.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Shortcut for drops using a region name.
		/// </summary>
		protected void DefineProp(ulong propId, string region, uint x, uint y, PropAction action, uint dropType)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			this.DefineProp(propId, regionId, x, y, action, dropType);
		}

		/// <summary>
		/// Shortcut for drops using a region id.
		/// </summary>
		protected void DefineProp(ulong propId, uint region, uint x, uint y, PropAction action, uint dropType)
		{
			MabiPropFunc behavior = (client, creature, prop) =>
			{
				if (Rnd() > WorldConf.PropDropRate)
					return;

				var di = MabiData.PropDropDb.Find(dropType);
				if (di == null)
				{
					Logger.Warning("Unknown prop drop type '{0}'.", dropType);
					return;
				}

				var dii = di.GetRndItem(RandomProvider.Get());
				var item = new MabiItem(dii.ItemClass);
				item.Info.Amount = dii.Amount > 1 ? (ushort)this.Rnd(1, dii.Amount) : (ushort)1;
				WorldManager.Instance.CreatureDropItem(prop, new ItemEventArgs(item));
			};
			this.DefineProp(propId, region, x, y, behavior);
		}

		/// <summary>
		/// Shortcut for warps using a region name.
		/// </summary>
		protected void DefineProp(ulong propId, string region, uint x, uint y, PropAction action, string tregion, uint tx, uint ty)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			uint tregionId = MabiData.MapDb.TryGetRegionId(tregion);
			this.DefineProp(propId, regionId, x, y, action, tregionId, tx, ty);
		}

		/// <summary>
		/// Shortcut for warps using a region id.
		/// </summary>
		protected void DefineProp(ulong propId, uint region, uint x, uint y, PropAction action, uint tregion, uint tx, uint ty)
		{
			MabiPropFunc behavior = (client, creature, prop) => { client.Warp(tregion, tx, ty); };
			this.DefineProp(propId, region, x, y, behavior);
		}

		protected void DefineProp(ulong propId, string region, uint x, uint y, MabiPropFunc behavior = null)
		{
			uint regionId = MabiData.MapDb.TryGetRegionId(region);
			this.DefineProp(propId, regionId, x, y, behavior);
		}

		/// <summary>
		/// Adds a behavior for the prop with the given id. Since this is for
		/// client side props we also need a location that can be checked later on.
		/// </summary>
		protected void DefineProp(ulong propId, uint region, uint x, uint y, MabiPropFunc behavior = null)
		{
			this.DefineProp(new MabiProp(propId, region, x, y), behavior);
		}

		/// <summary>
		/// Adds the given prop and behavior to the behavior list.
		/// </summary>
		protected void DefineProp(MabiProp prop, MabiPropFunc behavior = null)
		{
			if (behavior != null)
				WorldManager.Instance.SetPropBehavior(new MabiPropBehavior(prop, behavior));
		}

		/// <summary>
		/// "Redirect" to WorldManager.Instance.SpawnCreature.
		/// </summary>
		protected void Spawn(uint race, uint amount, uint region, uint x, uint y, uint radius = 0, bool effect = false)
		{
			WorldManager.Instance.SpawnCreature(race, amount, region, x, y, radius, effect);
		}

		/// <summary>
		/// "Redirect" to WorldManager.Instance.SpawnCreature.
		/// </summary>
		protected void Spawn(uint race, uint amount, uint region, MabiVertex pos, uint radius = 0, bool effect = false)
		{
			WorldManager.Instance.SpawnCreature(race, amount, region, pos, radius, effect);
		}

		/// <summary>
		/// Returns random int between from and too (inclusive).
		/// </summary>
		protected int Rnd(int from, int to)
		{
			return RandomProvider.Get().Next(from, to);
		}

		/// <summary>
		/// Returns random double between 0.0 and 1.0.
		/// </summary>
		protected double Rnd()
		{
			return RandomProvider.Get().NextDouble();
		}

		protected void Notice(WorldClient client, string msg, NoticeType type = NoticeType.MiddleTop)
		{
			if (client == null)
				return;

			client.Send(PacketCreator.Notice(msg, type));
		}

		protected void Broadcast(string msg, NoticeType type = NoticeType.Top)
		{
			WorldManager.Instance.Broadcast(PacketCreator.Notice(msg, type), SendTargets.All);
		}

		protected void AddHook(string npc, string hook, ScriptHook func)
		{
			ScriptManager.Instance.AddHook(npc, hook, func);
		}
	}

	public enum PropAction { None, Warp, Drop }
}
