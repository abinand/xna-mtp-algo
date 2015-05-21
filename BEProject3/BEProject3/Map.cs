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
using System.Xml;
using System.Diagnostics;
using DGui;

namespace BEProject3
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Map : Microsoft.Xna.Framework.DrawableGameComponent  //Design of the environment
    {
        int buttonclick=0;                                           
        String s;
        Game1 g;                                                  
        String url = "../../../Content/back2.xml";
        static HashSet<Vector2> barriers,temp;       // hashset datatype to hold positions of static and dynamic obstacles                              
        static Vector2 tileSize;                     //size of the tile( static obstacle) 
        protected Random random;                     //random object to generate random number
        int[] direction;                             
        float[] rotation;
        int count,grid,mcount;
        Vector2 v;

        //Declaration of UI objects
        DGuiManager _guiManager;                     
        DForm _form;
        DTextBox _textBox,_textBox1;
        DComboBox _algotype;
        DButton _button,_button1,_pauseplay;
        
        
        public static Queue<Vector2> targetPath;         //Queue to store path of target
        public Vector2[] motionbarriers;                 // to hold position of dynamic obstacles

        public Texture2D Image { get; set; }                  
        public Texture2D Image_m { get; set; }
        Vector2 origin = new Vector2(0,0);

        public Map(Game1 game)
            : base(game)
        {
            g = game;
            // initilization of objects
            barriers = new HashSet<Vector2>();
            temp = new HashSet<Vector2>();
            tileSize = new Vector2(28f, 28f);                   //tile size set to 28x28 pixel size
            direction = new int[10];
            rotation = new float[10];
            motionbarriers = new Vector2[10];
            v = new Vector2();
            s = "";
            
        }

        
        
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
          /*try 
               {
                   XmlTextReader reader = new XmlTextReader(url);
                   while (reader.Read()) 
                   {
                       switch (reader.NodeType) 
                       {
                           case XmlNodeType.Element:
                               if (reader.HasAttributes) 
                               {
                                   reader.MoveToAttribute(0);
                                   if (reader.Name == "tileId")
                                    {
                                        reader.MoveToAttribute(1);
                                        int x = int.Parse(reader.Value);
                                        reader.MoveToAttribute(2);
                                        int y = int.Parse(reader.Value);
                                        barriers.Add(new Vector2(x,y));
                                    }
                                }
                               break;
                           default:
                               break;
                        }
                                                       
                       }
                   } 
               catch (XmlException e) 
               {
                   Debug.Write("An error occured:"+e);
              }*/


          targetPath = new Queue<Vector2>();
          random = new Random();
         
         
            base.Initialize();

        }

        protected override void LoadContent()
        {
            base.LoadContent();
            SpriteBatch s = new SpriteBatch(GraphicsDevice); //to draw in the screen
            _guiManager = new DGuiManager(g,s);              //creation of form
            _form = new DForm(_guiManager, "GuiTest", null);
            _form.Size = new Vector2(415,640);
            _form.Position = new Vector2(835,1);
            _form.Initialize();
            _guiManager.AddControl(_form);
            DLayoutFlow layout = new DLayoutFlow(1 , 6, DLayoutFlow.DLayoutFlowStyle.Vertically);//creation of layout used
            
            layout.CellPadding=40;
            layout.Position = new Vector2(10, 40);
            
            _textBox = new DTextBox(_guiManager);   //adding textbox for specifying static obstacles
             layout.Add(_textBox);
            _textBox.Initialize();
            _textBox.Text = "150";
            _form.AddPanel(_textBox);
            _textBox1 = new DTextBox(_guiManager);  //adding textbox for specifying dynamic obstacles
            layout.Add(_textBox1);
            _textBox1.Initialize();
            _textBox1.Text = "9";
            _form.AddPanel(_textBox1);
            _algotype = new DComboBox(_guiManager);

            layout.Add(_algotype);                    //adding combo box for type of algorithm
            _algotype.Initialize();                 
            _algotype.AddItem("AStar", null);
            _algotype.AddItem("DYNAMIC TAO-MTP", null);
            _algotype.Text = "DYNAMIC TAO-MTP";
            _form.AddPanel(_algotype);
           
            _button = new DButton(_guiManager);      //addition of buttons for reset,clear and pause/play
            layout.Add(_button);
            _button.Text = "RESET";
            _button.OnClick+=new DButtonEventHandler(_button_OnClick);
            _button.Initialize();
            _form.AddPanel(_button);
            _button1 = new DButton(_guiManager);
            
            layout.Add(_button1);
            _button1.Text = "CLEAR";
            _button1.OnClick += new DButtonEventHandler(_button1_OnClick);
            _button.Initialize();
            _form.AddPanel(_button1);

            _pauseplay = new DButton(_guiManager);
            layout.Add(_pauseplay);
            _pauseplay.Text = ">||";
            _pauseplay.OnClick += new DButtonEventHandler(_pauseplay_OnClick);
            _pauseplay.Initialize();
            _form.AddPanel(_pauseplay);
            //grid =int.Parse(_textBox.Text);
            
           
            
        }

        void _button_OnClick(GameTime gameTime)         //event handler for reset buttonclick
        {
           

            if (int.Parse(_textBox.Text) <= 150 && int.Parse(_textBox.Text) >= 0 && int.Parse(_textBox1.Text) <= 9 && int.Parse(_textBox1.Text) >= 0)
            {
               
                grid = int.Parse(_textBox.Text);    //setting number of static obstacles
                mcount = int.Parse(_textBox1.Text); //setting number of dynamic obstacles
                AddBarriers();                      //function call
                AddMotionBarriers();
                buttonclick = 1;                    //flag set
            }
           
        }
        void _button1_OnClick(GameTime gameTime)    //event handler for clear buttonclick
        {
            
            buttonclick = 0;   //flag reset
            grid = 0;           
            mcount = 0;
            Draw(gameTime);
            
            
            

        }
        void _pauseplay_OnClick(GameTime gameTime)  //event handler for pause/play button
        {
            
            //checking combo box value(algorithm type)
            if (_algotype.Text.ToString().Equals("AStar"))
            {
                AStar.type = AlgorithmType.Astar;
                
            }
            if (_algotype.Text.ToString().Equals("DYNAMIC TAO-MTP"))
            {
                AStar.type = AlgorithmType.DynamicTAOMTP;

            }
            
            // pause/play 
            if(Pursuer.algorithmStopped ^= true)
                _pauseplay.Text = ">||";
            else
                _pauseplay.Text = "||";
                //AddBarriers();
            //AddMotionBarriers();


        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < motionbarriers.Count(); i++)    //sets direction for dynamic obstacles in random 
            {
                if (direction[i] == -1)                        
                 {
                label3:
                    int ran = random.Next(0, 4);                
                 
                    switch (ran)
                    {
                        case 0:
                            if (!barriers.Contains(new Vector2(motionbarriers[i].X - 1, motionbarriers[i].Y)))
                            {
                                direction[i] = 0;
                                rotation[i] = 0.0f;
                            }
                            else goto label3;
                            break;
                        case 1:
                            if (!barriers.Contains(new Vector2(motionbarriers[i].X + 1, motionbarriers[i].Y)))
                            {
                                direction[i] = 1;
                                rotation[i] = 0.0f;
                            }
                            else goto label3;
                            break;

                        case 2:
                            if (!barriers.Contains(new Vector2(motionbarriers[i].X, motionbarriers[i].Y + 1)))
                            {

                                direction[i] = 2;
                                rotation[i] = 1.5f;
                            }
                            else goto label3;
                            break;

                        case 3: if (!barriers.Contains(new Vector2(motionbarriers[i].X, motionbarriers[i].Y - 1)))
                            {
                                rotation[i] = 1.5f;
                                direction[i] = 3;
                            }
                            else goto label3;
                            break;
                    }
                }
            }

            //motion of dynamic obstacles in the set direction
            
            count++;
            if (count % 10 == 0&&_pauseplay.Text.Equals("||"))
            for (int i = 0; i < motionbarriers.Count(); i++)
                {
                    switch (direction[i])
                    {
                        case 0: motionbarriers[i].X -= 1f;
                            if (barriers.Contains(new Vector2((float)Math.Truncate(motionbarriers[i].X - 1), (float)Math.Truncate(motionbarriers[i].Y))))
                            {
                                direction[i] = -1;
                                rotation[i] = 0.0f;
                            }
                            break;
                        case 1:
                            motionbarriers[i].X += 1f;
                            if (barriers.Contains(new Vector2((float)Math.Truncate(motionbarriers[i].X + 1), (float)Math.Truncate(motionbarriers[i].Y))))
                            {
                                direction[i] = -1;
                                rotation[i] = 0.0f;
                            }

                            break;
                        case 2: motionbarriers[i].Y += 1f;

                            if (barriers.Contains(new Vector2((float)Math.Truncate(motionbarriers[i].X), (float)Math.Truncate(motionbarriers[i].Y + 1))))
                            {
                                direction[i] = -1;
                                rotation[i] = 0.0f;
                            }
                            break;

                        case 3: motionbarriers[i].Y -= 1f;

                            if (barriers.Contains(new Vector2((float)Math.Truncate(motionbarriers[i].X), (float)Math.Truncate(motionbarriers[i].Y - 1))))
                            {
                                direction[i] = -1;
                                rotation[i] = 0.0f;
                            }
                            break;
                    }
                }

            if (count == 10)        //reset direction if path does not exist
            {
                count = 0;
                barriers.Clear();
                Vector2[] arr;
                arr = temp.ToArray<Vector2>();

                foreach (Vector2 v in arr)
                    barriers.Add(v);
                for (int i = 0; i < motionbarriers.Count(); i++)
                    barriers.Add(motionbarriers[i]);
            }
            
            _guiManager.Update(gameTime);

        }

        public override void Draw(GameTime gameTime)
        {
            _guiManager.Draw(gameTime);
            if (buttonclick==1)
            {
                
                g.spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));  
                
                foreach (Vector2 point in temp)
                {
                    //draw static obstacles
                    g.spriteBatch.Draw(Image, MapToWorld(point), Color.White);

                }
                

                for (int i = 0; i < motionbarriers.Count(); i++)
                {
                    //draw dynamic obstacles
                    g.spriteBatch.Draw(Image_m, CenterOfTile(motionbarriers[i]), null, Color.White, rotation[i], new Vector2(16, 16), 1.0f, SpriteEffects.None, 1.0f);
                    

                }
            }

            
            
            
            try
            {
                //display queue length
                g.spriteBatch.DrawString(Game1.font, "Queue length: " + targetPath.ToArray().Length.ToString(), new Vector2(845, 550), Color.Red);
            }
            catch (InvalidOperationException) { }
            base.Draw(gameTime);
        }

        private void AddBarriers() //adding vector2 type to hashset for static obstacle
        {

            temp.Clear();
          
            
                //temp.Add(new Vector2(random.Next(1, 24), random.Next(1, 19)));
                
               // s = "100";
               // grid =int.Parse(_textBox.Text);
               //grid = random.Next(100, 150);
               
                if(grid!=0)
                for (int i = 0; i <=grid; i++)  
                {
                label1:
                    int x = random.Next(1, 24);//set x position to each static obstacle randomly
                    int y = random.Next(1, 19);//set y position to each static obstacle randomly
                    v = new Vector2(x, y);
                    //condition to add to hashset temp holding static obstacle position
                    if (!temp.Contains(v) && (!temp.Contains(new Vector2(v.X + 1, v.Y + 1)) && !temp.Contains(new Vector2(v.X - 1, v.Y + 1)) && !temp.Contains(new Vector2(v.X + 1, v.Y - 1)) && !temp.Contains(new Vector2(v.X - 1, v.Y - 1))))
                    {
                        temp.Add(v);
                    }
                    else goto label1;
                }

                for (int i = -1; i <= 21; i++)          //boundary blocks
                {
                    temp.Add(new Vector2(-1, i));
                    temp.Add(new Vector2(25, i));
                }
                for (int i = -1; i <= 26; i++)         //boundry blocks
                {
                    temp.Add(new Vector2(i, -1));
                    temp.Add(new Vector2(i, 20));
                }
            

        }
        private void AddMotionBarriers()   //adding vector2 type to array for dynamic obstacle
        {
           for(int i=0;i<motionbarriers.Count();i++)   //initialize
           {
               motionbarriers[i].X=-2;
               motionbarriers[i].Y = -2;
           }
            
             
           
            for (int i = 0; i <mcount; i++)
            {
            label2:
                float x = (float)random.Next(1, 24);      //set x position to each dynamic obstacle randomly
                float y = (float)random.Next(1, 19);      //set y position to each dynamic obstacle randomly

                v = new Vector2(x, y);
                                                          //condition to add position to hashset 
                if (!temp.Contains(v) && !motionbarriers.Contains(v))
                {
                    motionbarriers[i] = v;
                    direction[i] = -1;
                }
                else goto label2;
                             //merging static and dynamic obstacles into single hashset
                barriers.Clear();
                Vector2[] arr;
                arr = temp.ToArray<Vector2>();

                foreach (Vector2 point in arr)
                    barriers.Add(point);
                for (int j = 0; j < motionbarriers.Count(); j++)
                    barriers.Add(motionbarriers[j]);
            }
        }

        public static Vector2 WorldToMap(Vector2 position) //conversion from world(pixel) co ordinates to map coordinates
        {
            position.X = (int)(position.X / 32); // width and height of each tile is considered 32 pixels
            position.Y = (int)(position.Y / 32);
            return position;
        }

        public static Vector2 MapToWorld(Vector2 position)//conversion from map co ordinates to world(pixel) coordinates
        {
            position.X = (position.X * 32) ;
            position.Y = (position.Y * 32) ;
            return position;
        }

        public static bool Collides(Vector2 target)   //checking for collision
        {
            Vector2 temp;
            foreach (Vector2 point in barriers)
            {
                temp = MapToWorld(point);
                if (temp.X + tileSize.X > target.X &&
                temp.X < target.X + tileSize.X &&
                temp.Y + tileSize.Y > target.Y &&
                temp.Y < target.Y + tileSize.Y)
                    return true;
            }
            return false;
        }

        public static IEnumerable<Vector2> OpenNodes(Vector2 pointLoc)
        {
            Vector2 temp;
            if (!barriers.Contains(temp = new Vector2(pointLoc.X, pointLoc.Y + 1)))
                yield return temp;
            if (!barriers.Contains(temp = new Vector2(pointLoc.X + 1, pointLoc.Y)))
                yield return temp;
            /* 
                diagonals
             
               if (!barriers.Contains(temp = new Vector2(pointLoc.X + 1, pointLoc.Y + 1)))
                 yield return temp;
               if (!barriers.Contains(temp = new Vector2(pointLoc.X - 1, pointLoc.Y - 1)))
                 yield return temp;
              if (!barriers.Contains(temp = new Vector2(pointLoc.X - 1, pointLoc.Y + 1)))
                 yield return temp;
             if (!barriers.Contains(temp = new Vector2(pointLoc.X + 1, pointLoc.Y - 1)))
                yield return temp; 
             */
             
            if (!barriers.Contains(temp = new Vector2(pointLoc.X - 1, pointLoc.Y)))
                yield return temp;
            if (!barriers.Contains(temp = new Vector2(pointLoc.X, pointLoc.Y - 1)))
                yield return temp;
            

        }

        public static bool IsBarrier(Vector2 point)
        {
            if(barriers.Contains(point))
                return true;
            else
                return false;
        }

        public static Vector2 CenterOfTile(Vector2 tile)
        {
            return MapToWorld(tile) + new Vector2(16, 16);
        }
    }
}