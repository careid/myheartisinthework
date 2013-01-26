using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HeartGame
{
    public class GameStateManager
    {
        public Dictionary<string, GameState> States { get; set; }
        public Game1 Game { get; set; }
        private string CurrentState { get; set; }
        private string NextState { get; set; }
        public float TransitionSpeed { get; set; }

        public GameStateManager(Game1 game)
        {
            Game = game;
            States = new Dictionary<string, GameState>();
            CurrentState = "";
            NextState = "";
            TransitionSpeed = 1.0f;
        }

        public void SwitchState(string state)
        {
            NextState = state;
            States[NextState].OnEnter();
            States[NextState].TransitionValue = 0.0f;
            States[NextState].Transitioning = GameState.TransitionMode.Entering;

            if (CurrentState != "")
            {
                States[CurrentState].Transitioning = GameState.TransitionMode.Exiting;
                States[CurrentState].TransitionValue = 0.0f;
            }

        }

        private void TransitionComplete()
        {
            if (CurrentState != "")
            {
                States[CurrentState].OnExit();
                States[CurrentState].Transitioning = GameState.TransitionMode.Exiting;
            }

            CurrentState = NextState;
            States[CurrentState].Transitioning = GameState.TransitionMode.Running;
            NextState = "";
        }

        public void Update(GameTime time)
        {
            if (CurrentState != "" && States[CurrentState].IsInitialized)
            {
                States[CurrentState].Update(time);

                if(States[CurrentState].Transitioning != GameState.TransitionMode.Running)
                {
                    States[CurrentState].TransitionValue += (float)(TransitionSpeed * time.ElapsedGameTime.TotalSeconds); 
                }
            }

            if (NextState != "" && States[NextState].IsInitialized)
            {
                States[NextState].Update(time);
                if (States[NextState].Transitioning != GameState.TransitionMode.Running)
                {
                    States[NextState].TransitionValue += (float)(TransitionSpeed * time.ElapsedGameTime.TotalSeconds);
                    if (States[NextState].TransitionValue > 1.0)
                    {
                        TransitionComplete();
                    }
                }
            }
        }

        public void Render(GameTime time)
        {
            Game.GraphicsDevice.Clear(Color.Black);

            if (CurrentState != "" && States[CurrentState].IsInitialized)
            {
                States[CurrentState].Render(time);
            }
            else if(CurrentState != "" && !States[CurrentState].IsInitialized)
            {
                States[CurrentState].RenderUnitialized(time);
            }

            if (NextState != "" && States[NextState].IsInitialized)
            {
                States[NextState].Render(time);
            }
            else if (NextState != "" && !States[NextState].IsInitialized)
            {
                States[NextState].RenderUnitialized(time);
            }
        }
    }
}
