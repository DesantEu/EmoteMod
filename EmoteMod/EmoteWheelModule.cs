using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.EmoteMod
{
    public static class EmoteWheelModule
	{

		public static VirtualJoystick JoystickEmoteWheel;
		public static VirtualButton ButtonEmoteSend;

        public static EmoteWheel Wheel;


		private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
		{
            orig(self);

            Player Player = self;

            //if (Client == null || !Client.IsReady)
            //    goto End;

            if (!(Engine.Scene is Level level))
                goto End;

            //if (Player == null || Player.Scene != level)
            //    Player = level.Tracker.GetEntity<Player>();

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
            if (!level.Paused && EmoteModMain.Settings.EmoteWheel && !Player.Dead && Engine.TimeRate > 0.05f)
            {
                Wheel.Shown = JoystickEmoteWheel.Value.LengthSquared() >= 0.36f;
                int selected = Wheel.Selected;
                if (Wheel.Shown && selected != -1 && ButtonEmoteSend.Pressed)
                {
                    Wheel.Shown = false;
                    Wheel.Selected = -1;
                }
            }
            else
            {
                Wheel.Shown = false;
                Wheel.Selected = -1;
            }

            End:
            return;
            //if (Wheel?.Shown ?? false)
            //    Context.Main.StateUpdated |= Context.Main.ForceIdle.Add("EmoteWheel");
            //else
            //    Context.Main.StateUpdated |= Context.Main.ForceIdle.Remove("EmoteWheel");
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
		}
		internal static void Load()
		{
            On.Celeste.Input.Initialize += Input_Initialize;
            // CHANGE!!!!!!!!
            On.Celeste.Player.Update += Player_Update;
        }

		internal static void Unload()
		{
			On.Celeste.Input.Initialize -= Input_Initialize;
			On.Celeste.Player.Update -= Player_Update;

		}
	}
}
