using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class MailBox_Housing_CuilinScript : Mailbox
{
	public override void OnLoad()
	{
		base.OnLoad();
		SetName("_MailBox_Housing_Cuilin");
		SetRace(990019);

		SetLocation(region: 60204, x: 53305, y: 51991);

		SetDirection(214);
	}
}
