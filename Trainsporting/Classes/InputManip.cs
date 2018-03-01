using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;

using static Trainsporting.Game;

namespace Trainsporting.Classes
{
    static public class InputManip
    {
        static public void  CheckKeyPresses()
        {
            keyboardState = OpenTK.Input.Keyboard.GetState();

            if (KeyPress(Key.LShift))
            {
                int branch0Index = tracks.IndexOf(branches[0]);
                int branch1Index = tracks.IndexOf(branches[1]);
                if (train.branchSetting == 1)
                {
                    train.branchSetting = 0;
                    for (int i = TRACK_COLORING_OFFSET; i < TRACK_COLORING_OFFSET + NUMBER_OF_TRACKS_COLORED; i++)
                    {
                        tracks[branch0Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic3.png"];
                        tracks[branch1Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic2.png"];
                    }
                }
                else
                {
                    train.branchSetting = 1;
                    for (int i = TRACK_COLORING_OFFSET; i < TRACK_COLORING_OFFSET + NUMBER_OF_TRACKS_COLORED; i++)
                    {
                        tracks[branch0Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic2.png"];
                        tracks[branch1Index + i].Model.TextureID = textures[Global.TEXTURES_RELATIVE_PATH + "basic3.png"];
                    }
                }
            }

            // Store current state for next comparison;
            lastKeyboardState = keyboardState;
        }

        static bool KeyPress(Key key)
        {
            return (keyboardState[key] && (keyboardState[key] != lastKeyboardState[key]));
        }
    }
}
