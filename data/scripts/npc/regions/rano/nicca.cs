using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class NiccaScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_nicca");
		SetRace(10002);
		SetBody(height: 1.3f, fat: 0.99f, upper: 1.41f, lower: 1.05f);
		SetFace(skin: 19, eye: 5, eyeColor: 160, lip: 16);

		NPC.ColorA = 0x0;
		NPC.ColorB = 0x0;
		NPC.ColorC = 0x0;		

		EquipItem(Pocket.Face, 0x1324, 0xFA9C70, 0x566D16, 0x8C7D00);
		EquipItem(Pocket.Hair, 0x135B, 0xFFCB864E, 0xFFCB864E, 0xFFCB864E);
		EquipItem(Pocket.Armor, 0x32E7, 0xFFFED9D1, 0xFF694549, 0xFF884444);
		EquipItem(Pocket.Shoe, 0x429A, 0xFF804040, 0xFFC6825E, 0xFFF9E6A2);

		SetLocation(region: 3001, x: 165786, y: 170346);

		SetDirection(153);
		SetStand("human/male/anim/male_stand_Tarlach_anguish");
	}
}
