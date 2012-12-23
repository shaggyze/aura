using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class Castle_patrol_01Script : NPCScript
{
	public override void OnLoad()
	{
		SetName("_castle_patrol_01");
		SetRace(10002);
		SetBody(height: 1.1f, fat: 1.1f, upper: 1.4f, lower: 1.1f);
		SetFace(skin: 17, eye: 104, eyeColor: 29, lip: 4);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		

		EquipItem(Pocket.Face, 0x1324, 0xF79825, 0xF69B2D, 0x3868AF);
		EquipItem(Pocket.Hair, 0xFB0, 0x544223, 0x544223, 0x544223);
		EquipItem(Pocket.Armor, 0x3C4D, 0x808080, 0x808080, 0x808080);
		EquipItem(Pocket.Head, 0x4754, 0x808080, 0x808080, 0x808080);
		EquipItem(Pocket.RightHand2, 0x9C4C, 0xB0B0B0, 0x47A8E5, 0x676661);

		SetLocation(region: 410, x: 23634, y: 8821);

		SetDirection(63);
		SetStand("");
	}
}
