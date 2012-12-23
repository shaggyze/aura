using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class PencastScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_pencast");
		SetRace(10002);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 15, eye: 0, eyeColor: 126, lip: 0);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		

		EquipItem(Pocket.Face, 0x133A, 0xF49C33, 0x737171, 0xDD7785);
		EquipItem(Pocket.Hair, 0x100D, 0xADAAA5, 0xADAAA5, 0xADAAA5);
		EquipItem(Pocket.Armor, 0x3C1D, 0x808080, 0x808080, 0x808080);
		EquipItem(Pocket.Shoe, 0x4299, 0xEFE3B5, 0x660033, 0x808080);
		EquipItem(Pocket.Head, 0x4747, 0x808080, 0x4BA2E8, 0x8B2B2B);
		EquipItem(Pocket.RightHand1, 0x9D62, 0x808080, 0x808080, 0x156245);

		SetLocation(region: 401, x: 87883, y: 82244);

		SetDirection(255);
		SetStand("human/male/anim/male_natural_stand_npc_Duncan");
	}
}
