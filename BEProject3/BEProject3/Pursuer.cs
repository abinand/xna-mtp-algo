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
using DGui;
namespace BEProject3
{
    public enum PursuerStatus
    {
        exploring,
        tracking
    }
   
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Pursuer : Microsoft.Xna.Framework.DrawableGameComponent
    {
      
        public Texture2D Image { get; set; }
        Vector2 position;
        Vector2 tilePosition;
        float rotation = 0.0f;
        Vector2 origin;
        Vector2 destination;
        public static bool algorithmStopped;
        Map map;
        public static PursuerStatus status;
        AStar pathFinder;
        int lookahead = 4;
        Vector2 vfBest;
        bool hasPath=false;
        Stack<Vector2> waypoints;

        int alpha = 25;
        float speed = 200f;
        float threshold = 0.5f;

        public Pursuer(Game game)
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
            position = Map.CenterOfTile(new Vector2(20,15));
            origin = new Vector2(16, 16);
            status = PursuerStatus.exploring;
            pathFinder = new AStar();
            destination = new Vector2();
            waypoints = new Stack<Vector2>();
            algorithmStopped = true;
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            tilePosition = Map.WorldToMap(position-origin);
            // TODO: Add your update code here
            

           
            if (!algorithmStopped)
            {
                if (!hasPath)
                {
                    pathFinder.ExecuteSearch(lookahead, tilePosition, Map.WorldToMap(Target.position), out waypoints);
                    if (waypoints.Count != 0)
                    {
                        hasPath = true;
                        destination = waypoints.Pop();
                    }
                }
                else
                {
                    if (destination == tilePosition)
                    {
                        try
                        {
                            destination = waypoints.Pop();
                        }
                        catch (InvalidOperationException ex)
                        {
                            Debug.Write("complete");
                        }
                        ExecuteMoveToTile(gameTime, destination);
                    }
                    else
                    {
                        ExecuteMoveToTile(gameTime, destination);
                    }
                    if (waypoints.Count == 0 || Map.IsBarrier(destination))
                        hasPath = false;
                }
                vfBest = FindBest();
                if (threshold > ThresholdTest(vfBest))
                {
                    status = PursuerStatus.tracking;
                }
                else
                {
                    status = PursuerStatus.exploring;
                }
                DeleteTrajectoryTill(vfBest);
            }
            base.Update(gameTime);
        }

        private void DeleteTrajectoryTill(Vector2 vfBest)
        {
            int cut = Map.targetPath.ToList().IndexOf(vfBest);
            for (int i = 0; i < cut; i++)
            {
                Map.targetPath.Dequeue();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            sBatch.Draw(Image, position, null, Color.White,rotation, origin, 1.0f, SpriteEffects.None, 0);
            sBatch.DrawString(Game1.font,"Status:", new Vector2(845, 500), Color.Black);
            if (status == PursuerStatus.exploring)
            {
                sBatch.DrawString(Game1.font, "Exploring", new Vector2(925, 500), Color.SeaGreen);
            }
            else
            {
                sBatch.DrawString(Game1.font, "Tracking", new Vector2(925, 500), Color.CornflowerBlue);
            }
            sBatch.DrawString(Game1.font, "Best position(vf):" + vfBest.ToString(), new Vector2(845, 600), Color.Red);
           sBatch.DrawString(Game1.font, "Static Obstacles(0-150)  ", new Vector2(845, 15), Color.Black);
           sBatch.DrawString(Game1.font, "Dynamic Obstacles(0-9)  ", new Vector2(845, 85), Color.Black);
           sBatch.DrawString(Game1.font, "Algorithm ", new Vector2(845, 153), Color.Black);
            base.Draw(gameTime);
        }

        private Vector2 FindBest()
        {
            Vector2 bestPosition = Map.targetPath.Peek();
            foreach (Vector2 point in Map.targetPath)
            {
                if (Delta(point) < Delta(bestPosition))
                {
                    if (!Map.IsBarrier(point))
                    {
                        bestPosition = point;
                    }
                }
            }
            return bestPosition;
        }

        private int Delta(Vector2 position)
        {
            int manX = (int)Math.Abs(this.tilePosition.X - position.X);
            int manY = (int)Math.Abs(this.tilePosition.Y - position.Y);
            List<Vector2> queue = Map.targetPath.ToList<Vector2>();
            int delta = (alpha * (manX + manY)) + (int)((Map.targetPath.Count() - queue.IndexOf(position)) / 0.5);
            return delta;
        }

        private float ThresholdTest(Vector2 position)
        {
            int manX = (int)Math.Abs(this.tilePosition.X - position.X);
            int manY = (int)Math.Abs(this.tilePosition.Y - position.Y);
            List<Vector2> queue = Map.targetPath.ToList<Vector2>();
            float delta = (int)((alpha * (manX + manY)) / (Map.targetPath.Count));
            return delta;
        }

        private void ExecuteMoveToTile(GameTime gameTime, Vector2 destination)
        {
            destination = Map.CenterOfTile(destination);
            float distance = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 move = Vector2.Subtract(destination, position);
            if (move != Vector2.Zero)
            {
                float distanceToTarget = move.Length();
                distance = Math.Min(distance, distanceToTarget);
                move.Normalize();
                move *= distance;
            }
            position += move;
            rotation = (float)Math.Atan2(move.Y, move.X);
        }
    }
}