using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class AgnesScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_agnes");
		SetRace(10001);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 17, eye: 3, eyeColor: 47, lip: 1);

		NPC.ColorA = 0x0;
		NPC.ColorB = 0x0;
		NPC.ColorC = 0x0;		

		EquipItem(Pocket.Face, 0xF3C, 0xCD8320, 0xF0097B, 0x6C676B);
		EquipItem(Pocket.Hair, 0xBE1, 0xB5562, 0xB5562, 0xB5562);
		EquipItem(Pocket.Armor, 0x3AE8, 0xFFFCE8, 0xA5C261, 0x275A49);
		EquipItem(Pocket.Shoe, 0x4290, 0x227775, 0x2F2F, 0x576D8D);

		SetLocation(region: 52, x: 42538, y: 46984);

		SetDirection(244);
		SetStand("human/female/anim/female_natural_stand_npc_01");
	}
}
