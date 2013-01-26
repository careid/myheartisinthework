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
    public class GameState
    {
        public enum TransitionMode
        {
            Entering,
            Exiting,
            Running
        }

        public Game1 Game { get; set; }
        public string Name { get; set; }
        public GameStateManager StateManager { get; set; }
        public bool IsInitialized { get; set; }
        public float TransitionValue { get; set; }
        public TransitionMode Transitioning { get; set; }

        public GameState(Game1 game, string name, GameStateManager stateManager)
        {
            Game = game;
            Name = name;
            StateManager = stateManager;
            IsInitialized = false;
            TransitionValue = 0.0f;
            Transitioning = TransitionMode.Entering;
        }

        public virtual void OnEnter()
        {

        }

        public virtual void OnExit()
        {

        }


        public virtual void RenderUnitialized(GameTime gameTime)
        {

        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Render(GameTime gameTime)
        {

        }
    }
}
