using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;

namespace BEProject3
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Target : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Texture2D Image { get; set; }
        
        public static Vector2 position;
        Vector2 origin;
        Vector2 window;
        Vector2 prevPosition;
        float speed = 200f;

        public Target(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            position = new Vector2(0,0);
            origin = new Vector2(0,0);
            window = new Vector2(Game1.ScreenWidth - 32, Game1.ScreenHeight - 32);
            Map.targetPath.Enqueue(Map.WorldToMap(position));
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            prevPosition = position;
            float distance = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 move = GetMove(distance);
            Vector2 newPosition = position + move;
            //if (!Map.Collides(newPosition))
            {
                prevPosition = position;
                position = Vector2.Clamp(newPosition, Vector2.Zero, window);
                if (Map.WorldToMap(prevPosition) != Map.WorldToMap(position))
                {
                    Map.targetPath.Enqueue(Map.WorldToMap(position));
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            sBatch.Draw(Image, position,null,Color.White,0.0f,origin,1.0f,SpriteEffects.None,0);
            base.Draw(gameTime);
        }

        private Vector2 GetMove(float distance)
        {
            Vector2 move = Vector2.Zero;
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Up))
                move.Y = -1;
            if (keyboard.IsKeyDown(Keys.Down))
                move.Y = 1;
            if (keyboard.IsKeyDown(Keys.Right))
                move.X = 1;
            if (keyboard.IsKeyDown(Keys.Left))
                move.X = -1;
            return move * distance;
        }
    }
}