﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using World.Network;

namespace World
{
	class Program
	{
		static void Main(string[] args)
		{
			WorldServer.Instance.Run(args);
		}
	}
}