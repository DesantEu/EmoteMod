using Celeste.Mod.CelesteNet.Client.Components;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.EmoteMod
{
    public static class EmoteWheelModule 
	{

		public static VirtualJoystick JoystickEmoteWheel;
		public static VirtualButton ButtonEmoteSend;

        public static EmoteWheel Wheel;

        public static bool activatedWithButton;
        public static bool joystickMoved;

		private static ILHook celestenetUpdateEmoteWheelHook;
		internal static int onCNetWheelUpdate;

		private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
		{
            orig(self);

            Player Player = self;

            if (!(Engine.Scene is Level level))
                goto End;

            if (Player == null || Player.Scene != level)
                Player = level.Tracker.GetEntity<Player>();

            if (Wheel != null && Wheel.Scene != level)
            {
                Wheel.RemoveSelf();
                Wheel = null;
            }

            if (Player == null)
                goto End;

            if (Wheel == null)
                level.Add(Wheel = new EmoteWheel(Player));

            if (JoystickEmoteWheel == null)
                goto End;

            


            // TimeRate check is for Prologue Dash prompt freeze
            if (!level.Paused && !Player.Dead && Engine.TimeRate > 0.05f)
            {
                // cool key feture
                if (EmoteModMain.Settings.EmoteWheelBinding.Buttons.Count != 0 
                && MInput.GamePads[0].Pressed(EmoteModMain.Settings.EmoteWheelBinding.Buttons[0]))
                {
                    activatedWithButton = !activatedWithButton;
                }

                bool joystickActive = JoystickEmoteWheel.Value.LengthSquared() >= 0.36f;
                if (joystickActive && !joystickMoved && activatedWithButton) joystickMoved = true;

                // show
                Wheel.Shown = EmoteModMain.Settings.EmoteWheel && joystickActive
                   || activatedWithButton && !joystickMoved
                   || joystickMoved && joystickActive;


                if (!Wheel.Shown)
                {
                    activatedWithButton = false;
                    joystickMoved = false;
                }

				int selected = Wheel.Selected;
                if (Wheel.Shown && selected != -1 && ButtonEmoteSend.Pressed)
                {
                    Send(selected, Player);
                    // if you open it with button and press r3 it wont close so we do this
                    joystickMoved = true;
                }

            }
            else
            {
                Wheel.Shown = false;
                Wheel.Selected = -1;
                activatedWithButton = false;
                joystickMoved = false;
            }

            End:
            return;
            //if (Wheel?.Shown ?? false)
            //    Context.Main.StateUpdated |= Context.Main.ForceIdle.Add("EmoteWheel");
            //else
            //    Context.Main.StateUpdated |= Context.Main.ForceIdle.Remove("EmoteWheel");
        }



        public static void Send(int index, Player player)
		{
			string[] emotes = 
            {
				EmoteModMain.Settings.emote0,
				EmoteModMain.Settings.emote1,
				EmoteModMain.Settings.emote2,
				EmoteModMain.Settings.emote3,
				EmoteModMain.Settings.emote4,
				EmoteModMain.Settings.emote5,
				EmoteModMain.Settings.emote6,
				EmoteModMain.Settings.emote7,
				EmoteModMain.Settings.emote8,
				EmoteModMain.Settings.emote9
			};
            if (0 <= index && index < emotes.Length)
                EmoteModule.Emote(emotes[index], false, player);
		}

		private static void Input_Initialize(On.Celeste.Input.orig_Initialize orig)
		{
            orig();

			JoystickEmoteWheel = new VirtualJoystick(true,
				new VirtualJoystick.PadRightStick(Input.Gamepad, 0.2f)
			);
			ButtonEmoteSend = new VirtualButton(
				new VirtualButton.KeyboardKey(Keys.Q),
				new VirtualButton.PadButton(Input.Gamepad, Buttons.RightStick)
			);

            activatedWithButton = false;
		}

        private static void celestenetUpdateEmoteWheel(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // yea
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.36f)))
            {
                cursor.EmitDelegate<Func<float>>(() => Wheel?.Shown ?? true ? 100f : 1f);
                cursor.Emit(OpCodes.Mul);
            }
        }

		private static void OnHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player)
		{
			orig(self, player);
			Wheel?.TimeRateSkip.Add(self.IsFake ? "EmptySpaceHeart" : "HeartGem");
			if (self.IsFake && Wheel != null)
				Wheel.timeSkipForcedDelay = 10f;
		}

		private static void OnHeartGemEndCutscene(On.Celeste.HeartGem.orig_EndCutscene orig, HeartGem self)
		{
			orig(self);
			Wheel?.TimeRateSkip.Remove("HeartGem");
		}

		private static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
		{
			PlayerDeadBody pdb = orig(self, direction, evenIfInvincible, registerDeathInStats);
			if (pdb != null && Wheel != null)
			{
				Wheel.TimeRateSkip.Add("PlayerDead");
				Wheel.Shown = false;
				Wheel.ForceSetTimeRate = true;
                activatedWithButton = false;
			}
			return pdb;
		}

		internal static void Load()
		{
            On.Celeste.Input.Initialize += Input_Initialize;
            // CHANGE!!!!!!!!
            On.Celeste.Player.Update += Player_Update;

			On.Celeste.HeartGem.Collect += OnHeartGemCollect;
			On.Celeste.HeartGem.EndCutscene += OnHeartGemEndCutscene;
			On.Celeste.Player.Die += OnPlayerDie;

			celestenetUpdateEmoteWheelHook = new ILHook(typeof(CelesteNetEmoteComponent).GetMethod("Update"), celestenetUpdateEmoteWheel);

        }


		internal static void Unload()
		{
			On.Celeste.Input.Initialize -= Input_Initialize;
			On.Celeste.Player.Update -= Player_Update;

			On.Celeste.HeartGem.Collect -= OnHeartGemCollect;
			On.Celeste.HeartGem.EndCutscene -= OnHeartGemEndCutscene;
			On.Celeste.Player.Die -= OnPlayerDie;

			celestenetUpdateEmoteWheelHook.Dispose();
        }
	}
}
