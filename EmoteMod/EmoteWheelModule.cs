using Celeste.Mod.CelesteNet.Client.Components;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using Celeste.Mod.CelesteNet.Client;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;

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

            //if (Client == null || !Client.IsReady)
            //    goto End;

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
                if (EmoteModMain.Settings.EmoteWheelBinding.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.EmoteWheelBinding.Buttons[0]))
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
                }

                //(CelesteNetEmoteComponent)CelesteNetClientModule.Instance?.Context.Components[typeof(CelesteNetEmoteComponent)]


                //if (EmoteModMain.Settings.EmoteWheel || activatedWithButton)
                //{
                //    Wheel.Shown = JoystickEmoteWheel.Value.LengthSquared() >= 0.36f;
                //    int selected = Wheel.Selected;
                //    if (Wheel.Shown && selected != -1 && ButtonEmoteSend.Pressed)
                //    {
                //        Send(selected, Player);
                //    }
                //}
                //EmoteModMain.echo(activatedWithButton.ToString());

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
        // celestenetUpdateEmoteWheel
        //public static void celestenetUpdateEmoteWheel(Action<CelesteNetEmoteComponent, GameTime> orig, CelesteNetEmoteComponent self, GameTime gametime)
        //{
        //    if (self.Wheel != null && Wheel.Shown && self.Wheel.Shown)
        //    {
        //        self.Wheel.Shown = false;
        //    }
        //    orig(self, gametime);
        //}



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
		internal static void Load()
		{
            On.Celeste.Input.Initialize += Input_Initialize;
            // CHANGE!!!!!!!!
            On.Celeste.Player.Update += Player_Update;

            celestenetUpdateEmoteWheelHook = new ILHook(typeof(CelesteNetEmoteComponent).GetMethod("Update"), celestenetUpdateEmoteWheel);

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

		internal static void Unload()
		{
			On.Celeste.Input.Initialize -= Input_Initialize;
			On.Celeste.Player.Update -= Player_Update;

            celestenetUpdateEmoteWheelHook.Dispose();
        }
	}
}
