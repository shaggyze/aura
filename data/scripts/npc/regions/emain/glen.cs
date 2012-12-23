using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class GlenScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_glen");
		SetRace(10002);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 15, eye: 132, eyeColor: 31, lip: 24);

		NPC.ColorA = 0x0;
		NPC.ColorB = 0x0;
		NPC.ColorC = 0x0;		

		EquipItem(Pocket.Face, 0x1324, 0x81D0C5, 0x80B662, 0x62A2);
		EquipItem(Pocket.Hair, 0x1773, 0xDBBA00, 0xDBBA00, 0xDBBA00);
		EquipItem(Pocket.Armor, 0x3AC4, 0xC5880B, 0xC3C3C3, 0x808080);
		EquipItem(Pocket.Shoe, 0x4293, 0x292B35, 0x808080, 0x808080);
		EquipItem(Pocket.Head, 0x473E, 0x9AB284, 0x808080, 0x808080);

		SetLocation(region: 52, x: 29029, y: 43267);

		SetDirection(214);
		SetStand("chapter3/human/male/anim/male_c3_npc_leymore");
	}
}
