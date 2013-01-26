using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HeartGame
{

    public class CommandQueue
    {
        Queue<string> commands;
        public CommandQueue()
        {
            commands = new Queue<string>();
        }

        public string Read()
        {
            lock (this)
            {
                if (commands.Count > 0)
                {
                    return commands.Dequeue();
                }
                else
                {
                    return "";
                }
            }
        }
        public void Write(string inp)
        {
            lock (this)
            {
                commands.Enqueue(inp);
            }
        }
    }

    public class PlayState : GameState
    {
        public Camera Camera { get; set; }
        public ComponentManager ComponentManager { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public Effect Shader { get; set; }
        public GameComponent dorf;
        public Texture2D SunMap { get; set; }
        public Texture2D AmbientMap { get; set; }
        public Texture2D TorchMap { get; set; }
        static System.Net.Sockets.TcpClient TC;
        protected StreamWriter SW;
        protected CommandQueue CQ;
        public LocatableComponent ground;
        public List<PhysicsComponent> dorfs = new List<PhysicsComponent>();
        public List<LocatableComponent> hospitals = new List<LocatableComponent>();

        private float rand()
        {
            return (float)RandomHelper.random.NextDouble();
        }

        public PlayState(Game1 game, GameStateManager GSM) :
            base(game, "PlayState", GSM)
        {
            Camera = new OrbitCamera(Game.GraphicsDevice, 0, 0, 0.001f, new Vector3(0, 0, 0), new Vector3(-10, 10, 0), (float)Math.PI * 0.25f, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);
            ComponentManager = new ComponentManager();
            ComponentManager.RootComponent = new LocatableComponent(ComponentManager, "root", null, Matrix.Identity, Vector3.Zero, Vector3.Zero);

            /*
            // Networking shit
            TC = new System.Net.Sockets.TcpClient();
            TC.Connect("172.24.8.194", 3001);

            SW = new StreamWriter(TC.GetStream());
            //request dwarf count from server
            SW.WriteLine("add dwarf");
            SW.Flush();
            CQ = new CommandQueue();
            Thread t = new Thread(new ParameterizedThreadStart(runListener));
            t.Start(CQ);
            */

            for (int i = 0; i < 4; i++)
            {
                dorf = new Person("person", new Vector3(rand() * 10 - 5, rand() * 10 - 5, rand() * 10 - 5), ComponentManager, Game.Content, Game.GraphicsDevice, "dorfdorf");
                //EntityFactory.GenerateWalker(new Vector3(rand() * 10 - 5, rand() * 10 - 5, rand() * 10 - 5), ComponentManager, Game.Content, Game.GraphicsDevice, "dorfdorf");
                ((PhysicsComponent)dorf).Velocity = new Vector3(rand() * 2f - 1f, rand() * 2f - 1f, rand() * 2f - 1f);
                ((PhysicsComponent)dorf).HasMoved = true;
                dorfs.Add((PhysicsComponent)dorf);

                if (i == 0)
                {
                    VelocityController velocityController = new VelocityController((PhysicsComponent)dorf);
                    velocityController.IsTracking = true;
                    velocityController.WASDControl = true;
                }

            }


            

            Vector3 boundingBoxPos = Camera.Position + new Vector3(0, -15, 0);
            Vector3 boundingBoxExtents = new Vector3(200, 5, 200);
            Vector3 boundingBoxMin = boundingBoxPos - boundingBoxExtents * 0.5f;
            Vector3 boundingBoxMax = boundingBoxPos + boundingBoxExtents * 0.5f;

            ground = (LocatableComponent)EntityFactory.GenerateBlankBox(new BoundingBox(boundingBoxMin, boundingBoxMax), ComponentManager, Game.Content, Game.GraphicsDevice, "brown");
            LocatableComponent hospital1 
                 = (LocatableComponent)EntityFactory
                .GenerateBlankBox(new BoundingBox(new Vector3(-1, -2.1f, -11),
                                                  new Vector3(2, -0.9f, -8)),                                   
                                                  ComponentManager,
                                                  Game.Content,
                                                  Game.GraphicsDevice, 
                                                  "hospital");
            hospitals.Add(hospital1);

            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Shader = Game.Content.Load<Effect>("Hargraves");

            SunMap = Game.Content.Load<Texture2D>("sungradient");
            AmbientMap = Game.Content.Load<Texture2D>("ambientgradient");
            TorchMap = Game.Content.Load<Texture2D>("torchgradient");
        }

        static void runListener(object Obj)
        {
            CommandQueue CQ = (CommandQueue)Obj;
            StreamReader SR = new StreamReader(TC.GetStream());
            while (true)
            {
                string line = SR.ReadLine();
                CQ.Write(line);
            }
        }

        public override void OnEnter()
        {
            IsInitialized = true;
            base.OnEnter();
        }

        public override void Render(GameTime gameTime)
        {

            Game.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

   
            Shader.Parameters["xFogColor"].SetValue(new Vector3(0, 0, 0));
            Shader.Parameters["Clipping"].SetValue(false);
            Shader.Parameters["xView"].SetValue(Camera.ViewMatrix);

            Shader.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);
            Shader.CurrentTechnique = Shader.Techniques["Textured"];

            Shader.Parameters["xSunGradient"].SetValue(SunMap);
            Shader.Parameters["xAmbientGradient"].SetValue(AmbientMap);
            Shader.Parameters["xTorchGradient"].SetValue(TorchMap);

            Shader.Parameters["xTint"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            Shader.Parameters["SelfIllumination"].SetValue(true);

            Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;


            ComponentManager.Render(gameTime, Camera, SpriteBatch, Game.GraphicsDevice,Shader, false);
            
            base.Render(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Game.IsActive)
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                GeometricPrimitive.ExitGame = true;
                Game.Exit();
            }

            /*
            // add dwarfs for kicks and gigs
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                SW.WriteLine("add dwarf");
                SW.Flush();
            }

            string command = CQ.Read();
            if (command.Length > 0)
            {
                if (command == "okay")
                {
                    dorf = EntityFactory.GenerateWalker(new Vector3(rand() * 10 - 5, rand() * 10 - 5, rand() * 10 - 5), ComponentManager, Game.Content, Game.GraphicsDevice, "dorfdorf");
                }
            }
             */
            
            ComponentManager.Update(gameTime, Camera);
            List<BoundingBox> collideBox = new List<BoundingBox>();
            collideBox.Add(ground.GetBoundingBox());

            foreach (LocatableComponent h in hospitals)
            {
                collideBox.Add(h.GetBoundingBox());
            }

            int i = 0;

            foreach (PhysicsComponent d in dorfs)
            {
                d.HasMoved = true;

               
                if(i != 0)
                {
                    d.ApplyForce(new Vector3(rand() - 0.5f, 0, rand() - 0.5f) * 10, (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                    
                d.HandleCollisions(collideBox, (float)gameTime.ElapsedGameTime.TotalSeconds);

                if(i == 0)
                {
                    Camera.Position = d.GlobalTransform.Translation + new Vector3(10, 10,  0);
                    Camera.Target = d.GlobalTransform.Translation;
                }

                i++;
            }
            Camera.Update(gameTime);
            base.Update(gameTime);
        }
    }
}
