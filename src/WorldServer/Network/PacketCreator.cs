﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using Common.World;
using Common.Network;
using System.Collections.Generic;
using Common.Constants;

namespace World.Network
{
	public enum MsgBoxTitle { NOTICE, INFO, WARNING, CONFIRM }
	public enum MsgBoxButtons { NONE, CLOSE, OK_CANCEL, YES_NO_CANCEL }
	public enum MsgBoxAlign { LEFT, CENTER }
	public enum NoticeType { TOP = 1, TOP_RED, MIDDLE_TOP, MIDDLE, LEFT, TOP_GREEN, MIDDLE_SYSTEM, SYSTEM, MIDDLE_LOWER }

	/// <summary>
	/// Packet creator for often used packets
	/// </summary>
	public static class PacketCreator
	{
		public static MabiPacket SystemMessage(MabiCreature target, string from, string message)
		{
			var p = new MabiPacket(0x526C, target.Id);

			p.PutByte(0);
			p.PutString(from);
			p.PutString(message);
			p.PutByte(1);
			p.PutSInt(-32640);
			p.PutInt(0);
			p.PutByte(0);

			return p;
		}

		public static MabiPacket SystemMessage(MabiCreature target, string message)
		{
			return PacketCreator.SystemMessage(target, "<SYSTEM>", message);
		}

		public static MabiPacket ServerMessage(MabiCreature target, string message)
		{
			return PacketCreator.SystemMessage(target, "<SERVER>", message);
		}

		public static MabiPacket CombatMessage(MabiCreature target, string message)
		{
			return PacketCreator.SystemMessage(target, "<COMBAT>", message);
		}

		public static MabiPacket MsgBox(MabiCreature target, string message, MsgBoxTitle title = MsgBoxTitle.NOTICE, MsgBoxButtons buttons = MsgBoxButtons.CLOSE, MsgBoxAlign align = MsgBoxAlign.CENTER)
		{
			var p = new MabiPacket(0x526F, target.Id);

			p.PutString(message);
			p.PutByte((byte)title);
			p.PutByte((byte)buttons);
			p.PutByte((byte)align);

			return p;
		}

		public static MabiPacket MsgBox(MabiCreature target, string message, string title, MsgBoxButtons buttons = MsgBoxButtons.CLOSE, MsgBoxAlign align = MsgBoxAlign.CENTER)
		{
			var p = new MabiPacket(0x526F, target.Id);

			p.PutString(message);
			p.PutString(title);
			p.PutByte((byte)buttons);
			p.PutByte((byte)align);

			return p;
		}

		public static MabiPacket Notice(MabiCreature target, string message, NoticeType type = NoticeType.MIDDLE, uint duration = 0)
		{
			var p = new MabiPacket(0x526D, target.Id);

			p.PutByte((byte)type);
			p.PutString(message);
			if (duration > 0)
			{
				p.PutInt(duration);
			}

			return p;
		}

		public static MabiPacket EntitiesAppear(List<MabiEntity> entities)
		{
			var p = new MabiPacket(0x5334, 0x3000000000000000);

			p.PutShort((ushort)entities.Count);
			foreach (var entity in entities)
			{
				var data = new MabiPacket(0, 0);
				entity.AddEntityData(data);
				byte[] dataBytes = data.Build(false);

				p.PutShort(entity.DataType);
				p.PutInt((uint)dataBytes.Length);
				p.PutBin(dataBytes);
			}

			return p;
		}

		public static MabiPacket EntityAppears(MabiEntity entity)
		{
			uint op = 0x520C;
			if (entity is MabiItem)
				op = 0x5211;

			var p = new MabiPacket(op, 0x3000000000000000);
			entity.AddEntityData(p);
			return p;
		}

		public static MabiPacket EntitiesLeave(List<MabiEntity> entities)
		{
			var p = new MabiPacket(0x5335, 0x3000000000000000);

			p.PutShort((ushort)entities.Count);
			foreach (var entity in entities)
			{
				p.PutShort(entity.DataType);
				p.PutLong(entity.Id);
			}

			return p;
		}

		public static MabiPacket EntityLeaves(MabiEntity entity)
		{
			uint op = 0x520D;
			if (entity is MabiItem)
				op = 0x5212;

			var p = new MabiPacket(op, 0x3000000000000000);
			p.PutLong(entity.Id);
			p.PutByte(0);

			return p;
		}

		public static MabiPacket EnterRegionPermission(MabiEntity entity, bool permission = true)
		{
			var p = new MabiPacket(0x6597, 0x1000000000000001);
			var pos = entity.GetPosition();

			p.PutLong(entity.Id);
			if (permission)
			{
				p.PutByte(1);
				p.PutInt(entity.Region);
				p.PutInt(pos.X);
				p.PutInt(pos.Y);
			}
			else
			{
				p.PutByte(0);
			}

			return p;
		}

		public static MabiPacket ItemInfo(MabiCreature creature, MabiItem item)
		{
			var p = new MabiPacket(0x59E0, creature.Id);
			item.AddPrivateEntityData(p);
			return p;
		}

		public static MabiPacket ItemRemove(MabiCreature creature, MabiItem item)
		{
			var p = new MabiPacket(0x59E1, creature.Id);
			p.PutLong(item.Id);
			p.PutByte(item.Info.Pocket);
			return p;
		}

		public static MabiPacket ItemAmount(MabiCreature creature, MabiItem item)
		{
			var p = new MabiPacket(0x59EA, creature.Id);
			p.PutLong(item.Id);
			p.PutShort(item.Info.Bundle);
			p.PutByte(2);
			return p;
		}
	}
}