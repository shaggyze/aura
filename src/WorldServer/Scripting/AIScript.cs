﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Collections.Generic;
using System.Threading;
using Common.Tools;
using Common.World;
using World.World;
using System;

namespace World.Scripting
{
	public enum AIState { Any, Idle, Dead, Noticed, Aggro }
	public enum AIAction
	{
		// Walk to a random spot inside the defined radius.
		// intVal1 = Radius
		WalkRandom,

		// intVal1 = X
		// intVal2 = Y
		Run,

		// Say the specified line
		// strVal1 = Message
		// TODO: Need more options, for more random texts.
		Say,

		Attack,

		// Don't do anything for the specified amount of beats
		// intVal1 = Beats
		Wait,
	}

	public class AIElement
	{
		public AIState State;
		public AIAction Action;

		public int IntVal1, IntVal2;
		public string StrVal1;

		public int WaitBeats;
		public int Cooldown;

		/// <summary>
		/// AI elements are summaries of an actions with parameters, and a state
		/// in which they can be chosen to be added to the stack, to be executed.
		/// </summary>
		/// <param name="state">State in which this action can be chosen.</param>
		/// <param name="action">Action to be done.</param>
		/// <param name="intVal1">Int parameter for this action.</param>
		/// <param name="strVal1">String parameter for this action.</param>
		/// <param name="cooldown">Beats till this action can be used again.</param>
		public AIElement(AIState state, AIAction action, int intVal1 = 0, int intVal2 = 0, string strVal1 = null, int cooldown = 0)
		{
			this.State = state;
			this.Action = action;
			this.IntVal1 = intVal1;
			this.IntVal2 = intVal2;
			this.StrVal1 = strVal1;
			this.Cooldown = cooldown;
		}
	}

	public class AIDCooldown
	{
		public AIAction Action;
		public int Beats, BeatCount;

		/// <summary>
		/// Information holder, for how many beats we have to wait,
		/// till the action can be used again. BeatCount must be updated
		/// once a beat.
		/// </summary>
		/// <param name="action">Action this delay is for</param>
		/// <param name="beats">Number of beats to wait</param>
		public AIDCooldown(AIAction action, int beats)
		{
			this.Action = action;
			this.Beats = beats;
		}
	}

	public class AIScript : BaseScript
	{
		protected List<AIElement> Elements = new List<AIElement>();
		protected List<AIElement> Stack = new List<AIElement>();
		protected List<AIDCooldown> Cooldowns = new List<AIDCooldown>();

		public MabiCreature Creature;

		private Timer _heartbeatTimer;
		private int _heartbeat = 50;//ms

		private AIState _prevState;

		/// <summary>
		/// To be executed when the script is added to the creature.
		/// </summary>
		public override void OnLoad()
		{
			_heartbeatTimer = new Timer(this.OnHearbeat, null, 5000, _heartbeat);
			this.Definition();
		}

		/// <summary>
		/// Executed from OnLoad. Should be overriden by AI scripts,
		/// to define actions.
		/// </summary>
		public virtual void Definition()
		{
		}

		/// <summary>
		/// Adds AI element to this script, that may be chosen to be executed.
		/// </summary>
		/// <param name="state">State in which this action can be chosen.</param>
		/// <param name="action">Action to be done.</param>
		/// <param name="intVal1">Int parameter for this action.</param>
		/// <param name="strVal1">String parameter for this action.</param>
		/// <param name="cooldown">Ms till this action can be used again.</param>
		public void Define(AIState state, AIAction action, int intVal1 = 0, int intVal2 = 0, string strVal1 = null, int cooldown = 0)
		{
			this.Elements.Add(new AIElement(state, action, intVal1, intVal2, strVal1, this.GetBeats(cooldown)));
		}

		/// <summary>
		/// Returns number of beats, that are required till
		/// the specified amount of milliseconds passed.
		/// </summary>
		/// <param name="ms"></param>
		/// <returns></returns>
		public int GetBeats(int ms)
		{
			return (ms / _heartbeat);
		}

		/// <summary>
		/// Called by the intern timer every 500ms.
		/// </summary>
		/// <param name="stateobj"></param>
		public void OnHearbeat(object stateobj)
		{
			// TODO: try, catch, logging

			// Stop if there are no actions or no creature
			if (this.Creature == null && this.Elements.Count < 1)
				return;

			// Stop if there are no characters in range
			var inSight = WorldManager.Instance.GetPlayersInRange(this.Creature, 2600);
			if (inSight.Count < 1)
				return;

			// Stop if creature is unable to do anything
			if (this.Creature.IsDead() || this.Creature.IsStunned())
			{
				// TODO: Empty stack?
				// TODO: Stun should later be a state.
				return;
			}

			var rand = RandomProvider.Get();

			var inRange = WorldManager.Instance.GetPlayersInRange(this.Creature, 1500); // TODO: Use race default
			if (inRange.Count > 0 /* && AutoAggro && Ready && etc */)
			{
				//this.Creature.Target = inRange[rand.Next(inRange.Count)];
			}

			// Decide what state we're in
			var curState = AIState.Idle;
			if (this.Creature.Target != null)
			{
				if (this.Creature.Target.IsDead() || !WorldManager.InRange(this.Creature, this.Creature.Target, 2900))
				{
					this.Stack.Clear();
					this.Creature.Target = null;
				}
				else
				{
					curState = AIState.Aggro;
				}
			}

			// Update cooldown stack
			this.Cooldowns.RemoveAll(a => a.BeatCount++ >= a.Beats);

			// No stack? Gotta fill it!
			if (this.Stack.Count < 1)
			{
				// Decide what to do
				var elements = this.Elements.FindAll(a => a.State == curState || a.State == AIState.Any);
				if (elements.Count > 0)
				{
					// Find an element that can be used
					AIElement element = null;
					for (int i = 0; element == null && i < elements.Count; ++i)
					{
						var tmpElement = elements[rand.Next(elements.Count)];

						// Check for cooldown
						if (!this.Cooldowns.Exists(a => a.Action == tmpElement.Action))
							element = tmpElement;
					}

					// Add the element if one was found
					if (element != null)
					{
						switch (element.Action)
						{
							default:
								this.Stack.Add(element);
								break;
						}
					}
				}
			}

			// Handle stack
			if (this.Stack.Count > 0)
			{
				var element = this.Stack[0];

				switch (element.Action)
				{
					case AIAction.WalkRandom:
						{
							this.Stack.RemoveAt(0);

							var pos = this.Creature.GetPosition();
							var x = (uint)(pos.X + rand.Next(-element.IntVal1, element.IntVal1 + 1));
							var y = (uint)(pos.Y + rand.Next(-element.IntVal1, element.IntVal1 + 1));
							var dest = new MabiVertex(x, y);

							this.Creature.StartMove(dest, true);

							WorldManager.Instance.CreatureMove(this.Creature, pos, dest, true);
							break;
						}

					case AIAction.Run:
						{
							this.Stack.RemoveAt(0);

							var to = new MabiVertex((uint)element.IntVal1, (uint)element.IntVal2);

							if (!this.Creature.IsDestination(to))
							{
								var from = this.Creature.StartMove(to, false);
								WorldManager.Instance.CreatureMove(this.Creature, from, to, false);
							}
							break;
						}

					case AIAction.Attack:
						{
							var attackResult = MabiCombat.Attack(this.Creature, this.Creature.Target);
							if (attackResult == AttackResult.OutOfRange)
							{
								var targetPos = this.Creature.Target.GetPosition();
								this.Stack.Insert(0, new AIElement(AIState.Aggro, AIAction.Run, intVal1: (int)targetPos.X, intVal2: (int)targetPos.Y));
							}
							else if (attackResult == AttackResult.Okay)
							{
								this.Stack.RemoveAt(0);
							}
							break;
						}

					case AIAction.Say:
						{
							this.Stack.RemoveAt(0);
							WorldManager.Instance.CreatureTalk(this.Creature, element.StrVal1);
							break;
						}

					case AIAction.Wait:
						{
							if (element.WaitBeats++ < element.IntVal1)
								return;

							this.Stack.RemoveAt(0);
							break;
						}
				}

				if (element.Cooldown > 0)
				{
					this.Cooldowns.Add(new AIDCooldown(element.Action, element.Cooldown));
				}
			}

			_prevState = curState;
		}
	}
}