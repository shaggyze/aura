using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class NonaScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_nona");
		SetRace(10001);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 21, eye: 7, eyeColor: 54, lip: 43);

		NPC.ColorA = 0x0;
		NPC.ColorB = 0x0;
		NPC.ColorC = 0x0;		

		EquipItem(Pocket.Face, 0xF3C, 0x6BC05D, 0xB4588C, 0xFAC259);
		EquipItem(Pocket.Hair, 0xC24, 0x6BADCA, 0x6BADCA, 0x6BADCA);
		EquipItem(Pocket.Armor, 0x3E2E, 0xA69D8B, 0x2B2626, 0x6E724E);
		EquipItem(Pocket.RightHand2, 0x9E22, 0x808080, 0x959595, 0x3C4155);

		SetLocation(region: 52, x: 30778, y: 42871);

		SetDirection(123);
		SetStand("chapter4/human/female/anim/female_c4_npc_wildwoman_talk");
	}
}
