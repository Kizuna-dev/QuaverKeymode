﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.GameState;
using Quaver.Config;
using Quaver.Database;
using Quaver.Gameplay;
using Quaver.Logging;
using Quaver.Main;
using Quaver.QuaFile;

namespace Quaver.Input
{
    internal class GameplayInputManager : IInputManager
    {
        /// <summary>
        ///     The current State
        /// </summary>
        public State CurrentState { get; set; } = State.PlayScreen;

        /// <summary>
        ///     The current Keyboard State
        /// </summary>
        public KeyboardState KeyboardState { get; set; }

        /// <summary>
        ///     TRhe current Mouse State
        /// </summary>
        public MouseState MouseState { get; set; }

        /// <summary>
        ///     All of the lane keys mapped to a list
        /// </summary>
        private List<Keys> LaneKeys { get; } = new List<Keys>()
        {
            Configuration.KeyMania1,
            Configuration.KeyMania2,
            Configuration.KeyMania3,
            Configuration.KeyMania4
        };

        /// <summary>
        ///     Keeps track of unique key presses
        /// </summary>
        private bool[] LaneKeyDown { get; set; } = new bool[4];

        /// <summary>
        ///     Keeps track of whether or not the song intro was skipped.
        /// </summary>
        private bool IntroSkipped { get; set; }

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput(Qua qua, bool skippable)
        {
            // Set the current state of the keyboard.
            KeyboardState = Keyboard.GetState();

            // Set the current mouse state.
            MouseState = Mouse.GetState();

            // Check Mania Key Presses
            HandleManiaKeyPresses();

            // Check Skip Song Input
            SkipSong(qua, skippable);

            // Check import beatmaps
            ImportBeatmaps();
        }

        /// <summary>
        ///     Handles what happens when a mania key is pressed and released.
        /// </summary>
        private void HandleManiaKeyPresses()
        {
            // Update Lane Keys Receptor
            for (var i = 0; i < LaneKeys.Count; i++)
            {
                //Lane Key Press
                if (KeyboardState.IsKeyDown(LaneKeys[i]) && !LaneKeyDown[i])
                {
                    LaneKeyDown[i] = true;
                    NoteManager.Input(i,true);
                    Playfield.UpdateReceptor(i, true);
                    GameBase.LoadedSkin.Hit.Play();
                }
                //Lane Key Release
                else if (KeyboardState.IsKeyUp(LaneKeys[i]) && LaneKeyDown[i])
                {
                    LaneKeyDown[i] = false;
                    NoteManager.Input(i, false);
                    Playfield.UpdateReceptor(i, false);
                }
            }
        }

        /// <summary>
        ///     Detects if the song intro is skippable and will skip if the player decides to.
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="currentSongTime"></param>
        private void SkipSong(Qua qua, bool skippable)
        {
            if (skippable && KeyboardState.IsKeyDown(Configuration.KeySkipIntro) && !IntroSkipped)
            {
                IntroSkipped = true;

                Console.WriteLine("[GAMEPLAY STATE] Song has been successfully skipped to 3 seconds before the first HitObject.");

                // Pause the song temporarily.
                GameBase.SelectedBeatmap.Song.Pause();

                // Skip to 3 seconds before the notes start
                GameBase.SelectedBeatmap.Song.SkipTo(qua.HitObjects[0].StartTime - 3000);

                // Resume the song
                GameBase.SelectedBeatmap.Song.Resume();
            }
        }

        /// <summary>
        ///     Checks if the beatmap import queue is ready, and imports then if the user decides to.
        /// </summary>
        private void ImportBeatmaps()
        {
            // TODO: This is a beatmap import and sync test, eventually add this to its own game state
            if (KeyboardState.IsKeyDown(Keys.F5) && GameBase.ImportQueueReady)
            {
                GameBase.ImportQueueReady = false;

                // Asynchronously load and set the GameBase beatmaps and visible ones.
                Task.Run(async () =>
                {
                    await GameBase.LoadAndSetBeatmaps();
                    GameBase.VisibleBeatmaps = GameBase.Beatmaps;
                });
            }
        }
    }
}
