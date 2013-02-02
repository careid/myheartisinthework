using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HeartGame
{
    public class WinState : GameState
    {
        public PlayState Play { get; set;}
        public SpriteBatch SpriteBatch { get; set; }

        public WinState(Game1 game, GameStateManager GSM, PlayState play) :
            base(game, "PlayState", GSM)
        {
            Play = play;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void OnEnter()
        {
            IsInitialized = true;
            base.OnEnter();
        }

        public override void Update(GameTime gameTime)
        {

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                GeometricPrimitive.ExitGame = true;
                Game.Exit();
            }

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                StateManager.SwitchState("PlayState");
            }

            base.Update(gameTime);
        }

        public override void Render(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(new Color(10, 10, 10));
            SpriteBatch.Begin();
            Drawer2D.DrawRect(SpriteBatch, new Rectangle(5, 5, Game.GraphicsDevice.Viewport.Width - 5, Game.GraphicsDevice.Viewport.Height - 5), Color.Gray, 3);

            string winString = "Red Team Wins!";

            if(Play.hospitals[0].Score < Play.hospitals[1].Score)
            {
                winString = "Blue Team Wins!";
            }

            Drawer2D.DrawStrokedText(SpriteBatch, winString, Drawer2D.DefaultFont, new Vector2(300, 200), Color.White, Color.Black);
            Drawer2D.DrawStrokedText(SpriteBatch, "Red Team: $" + Play.hospitals[0].Score + " (" + Play.hospitals[0].NumPatients + " patients )", Drawer2D.DefaultFont, new Vector2(300, 300), Color.White, Color.Red);
            Drawer2D.DrawStrokedText(SpriteBatch, "Blue Team: $" + Play.hospitals[1].Score + " (" + Play.hospitals[1].NumPatients + " patients )", Drawer2D.DefaultFont, new Vector2(300, 330), Color.White, Color.Blue);
            Drawer2D.DrawStrokedText(SpriteBatch, "Press Space To Play Again!", Drawer2D.DefaultFont, new Vector2(300, 400), Color.White, Color.Black);


            SpriteBatch.End();
            
            base.Render(gameTime);
        }

    }
}
