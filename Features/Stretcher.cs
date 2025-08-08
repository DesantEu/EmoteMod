using System;
using MonoMod.Cil;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using System.Globalization;
using Celeste;

namespace EmoteMod.Features
{
    internal class Stretch
    {
        public static float x_stretch = 1;
        public static float y_stretch = 1;

        public static bool stretch_lock = false;

        private static Player player;

        public static void stretch_x(float x)
        {
            x_stretch = x;
        }
        public static void stretch_y(float y)
        {
            y_stretch = y;
        }

        internal static void lock_stretch()
        {
            stretch_lock = !stretch_lock;
            x_stretch = 1;
            y_stretch = 1;
        }

        public static void FindAndReplaceScale(ILContext il)
        {
            ILCursor cursor = new(il);

            while (cursor.TryGotoNext(MoveType.Before, instr => instr.Match(OpCodes.Stfld)))
            {
                string name = Convert.ToString(cursor.Next.Operand, CultureInfo.InvariantCulture);
                if (!name.Contains("Scale"))
                    continue;

                cursor.Index -= 2;
                if (cursor.Prev.OpCode == OpCodes.Ldc_R4)
                    cursor.EmitDelegate<Func<float, float>>((orig) => orig * x_stretch);
                cursor.Index++;
                if (cursor.Prev.OpCode == OpCodes.Ldc_R4)
                    cursor.EmitDelegate<Func<float, float>>((orig) => orig * y_stretch);
                cursor.Index++;
                cursor.Index++;
            }
        }

        public static void FindAndReplaceScaleXY(ILContext il)
        {
            ILCursor cursor = new(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldflda)))
            {
                string name = Convert.ToString(cursor.Prev.Operand, CultureInfo.InvariantCulture);
                if (!name.Contains("Scale"))
                    continue;
                int index = cursor.Index;

                cursor.GotoNext(MoveType.Before, instr => instr.MatchStfld<Vector2>("X"));
                cursor.EmitDelegate<Func<float, float>>((orig) => orig * x_stretch);
                cursor.Index = index;
                cursor.GotoNext(MoveType.Before, instr => instr.MatchStfld<Vector2>("Y"));
                cursor.EmitDelegate<Func<float, float>>((orig) => orig * y_stretch);
            }
        }
    }
}
