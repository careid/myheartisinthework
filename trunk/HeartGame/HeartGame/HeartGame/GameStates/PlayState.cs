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
    public enum Event
    {
        // Keypress events
        W_PRESS = 0,
        A_PRESS = 1,
        S_PRESS = 2,
        D_PRESS = 3,
        SPACE_PRESS = 4,

        // Key release events
        W_RELEASE = 5,
        A_RELEASE = 6,
        S_RELEASE = 7,
        D_RELEASE = 8,
        SPACE_RELEASE = 9,
    };

    public class PlayState : GameState
    {
        public Camera Camera { get; set; }
        public ComponentManager ComponentManager { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public Effect Shader { get; set; }
        public Texture2D SunMap { get; set; }
        public Texture2D AmbientMap { get; set; }
        public Texture2D TorchMap { get; set; }
        public LocatableComponent ground;
        public List<PhysicsComponent> dorfs = new List<PhysicsComponent>();
        public List<Hospital> hospitals = new List<Hospital>();
        public Player player;
        public Drawer2D drawer2D;
        public List<Event> frameEvents;
        public SoundManager sounds;
        public ParticleManager particles;
        protected Client client;
        protected bool online;

        private float rand()
        {
            return (float)RandomHelper.random.NextDouble();
        }

        public PlayState(Game1 game, GameStateManager GSM) :
            base(game, "PlayState", GSM)
        {
            online = false;
            SoundManager.Content = game.Content;
            Camera = new OrbitCamera(Game.GraphicsDevice, 0, 0, 0.001f, new Vector3(0, 15, 0), new Vector3(-10, 10, 0), (float)Math.PI * 0.25f, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);
            ComponentManager = new ComponentManager();
            ComponentManager.RootComponent = new LocatableComponent(ComponentManager, "root", null, Matrix.Identity, Vector3.Zero, Vector3.Zero);

            frameEvents = new List<Event>();
            particles = new ParticleManager(ComponentManager);

            EmitterData testData = new EmitterData();
            testData.AngularDamping = 1.0f;
            List<Point> frm = new List<Point>();
            frm.Add(new Point(0, 0));
            frm.Add(new Point(1, 0));
            frm.Add(new Point(0, 1));
            frm.Add(new Point(1, 1));
            testData.Animation = new Animation(Game.GraphicsDevice, Game.Content.Load<Texture2D>("electricity"), "electricity", 32, 32, frm, true, Color.White, 20f, 1.0f, 1.0f, false);
            testData.ConstantAccel = new Vector3(0, -1, 0);
            testData.LinearDamping = 0.9f;
            testData.AngularDamping = 0.9f;
            testData.EmissionFrequency = 1.0f;
            testData.EmissionRadius = 1.0f;
            testData.EmissionSpeed = 20.0f;
            testData.GrowthSpeed = -0.9f;
            testData.MaxAngle = 3.14159f;
            testData.MinAngle = 0.0f;
            testData.MaxParticles = 1000;
            testData.MaxScale = 1.0f;
            testData.MinScale = 0.1f;
            testData.MinAngular = -5.0f;
            testData.MaxAngular = 5.0f;
            testData.ParticleDecay = 0.1f;
            testData.ParticlesPerFrame = 0;
            testData.ReleaseOnce = true;
            testData.Texture = Game.Content.Load<Texture2D>("electricity");
            particles.RegisterEffect("shock", testData);

            InputManager inputManager = new InputManager();
            InputManager.KeyPressedCallback += pressKey;
            InputManager.KeyReleasedCallback += releaseKey;

            sounds = new SoundManager();

            drawer2D = new Drawer2D(game.Content, game.GraphicsDevice);

            string name;
            if (online)
            {
                client = new Client();
                name = client.Connect();
            }
            else
            {
                name = "0";
            }

            if (name == "0")
            {
                for (int i = 0; i < 10; i++)
                {
                    NPC npc;
                    switch ((int)(rand() * 3))
                    {
                        case (0):
                            npc = new Smoker(new Vector3(rand() * 10 - 5, 5, rand() * 10 - 5), ComponentManager,
                                            Game.Content, Game.GraphicsDevice);
                            break;
                        case (1):
                            npc = new Fatter(new Vector3(rand() * 10 - 5, 5, rand() * 10 - 5), ComponentManager,
                                            Game.Content, Game.GraphicsDevice);
                            break;
                        case (2):
                            npc = new Older(new Vector3(rand() * 10 - 5, 5, rand() * 10 - 5), ComponentManager,
                                            Game.Content, Game.GraphicsDevice);
                            break;
                        default:
                            /* graphics don't exist yet, never reached */
                            npc = new Worker(new Vector3(rand() * 10 - 5, 5, rand() * 10 - 5), ComponentManager,
                                            Game.Content, Game.GraphicsDevice);
                            break;
                    }
                    npc.velocityController.MaxSpeed = 1;
                    npc.Target = new Vector3(-1, -2.1f, -11);
                    npc.Velocity = new Vector3(rand() * 2f - 1f, rand() * 2f - 2f, rand() * 2f - 1f);
                    npc.HasMoved = true;
                    npc.IsSleeping = false;
                    dorfs.Add(npc);
                }
            }

            // Player!
            player = new Player("dorf", new Vector3(rand() * 10 - 5, 5, rand() * 10 - 5),
                ComponentManager, Game.Content, Game.GraphicsDevice, "surgeonwalk");
            player.Velocity = new Vector3(rand() * 2f - 1f, rand() * 2f - 1f, rand() * 2f - 1f);
            player.HasMoved = true;
            dorfs.Add(player);
            VelocityController velocityController = new VelocityController(player);
            velocityController.IsTracking = true;


            Vector3 boundingBoxPos = new Vector3(0, -2, 0);
            Vector3 boundingBoxExtents = new Vector3(100, 4, 100);
            Vector3 boundingBoxMin = boundingBoxPos - boundingBoxExtents * 0.5f;
            Vector3 boundingBoxMax = boundingBoxPos + boundingBoxExtents * 0.5f;

            ground = (LocatableComponent)EntityFactory.GenerateBlankBox(new BoundingBox(boundingBoxMin, boundingBoxMax), ComponentManager, Game.Content, Game.GraphicsDevice, "brown");


            Hospital hospital1 = new Hospital(player, new Vector3(-1, 0, -11), new Vector3(4, 2, 3), ComponentManager, Game.Content, Game.GraphicsDevice, "hospital");
            Hospital hospital2 = new Hospital(null, new Vector3(5, 0, 5), new Vector3(2, 7, 2), ComponentManager, Game.Content, Game.GraphicsDevice, "hospital");
            hospitals.Add(hospital1);
            hospitals.Add(hospital2);

            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Shader = Game.Content.Load<Effect>("Hargraves");

            SunMap = Game.Content.Load<Texture2D>("sungradient");
            AmbientMap = Game.Content.Load<Texture2D>("ambientgradient");
            TorchMap = Game.Content.Load<Texture2D>("torchgradient");

            
        }

        public void pressKey(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    frameEvents.Add(Event.W_PRESS);
                    break;
                case Keys.A:
                    frameEvents.Add(Event.A_PRESS);
                    break;
                case Keys.S:
                    frameEvents.Add(Event.S_PRESS);
                    break;
                case Keys.D:
                    frameEvents.Add(Event.D_PRESS);
                    break;
                case Keys.Space:
                    frameEvents.Add(Event.SPACE_PRESS);
                    break;
                default:
                    break;
            }
        }

        public void releaseKey(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    frameEvents.Add(Event.W_RELEASE);
                    break;
                case Keys.A:
                    frameEvents.Add(Event.A_RELEASE);
                    break;
                case Keys.S:
                    frameEvents.Add(Event.S_RELEASE);
                    break;
                case Keys.D:
                    frameEvents.Add(Event.D_RELEASE);
                    break;
                case Keys.Space:
                    frameEvents.Add(Event.SPACE_RELEASE);

                    if (player.DefibCharge >= 0.25f)
                    {
                        defib(player);
                    }
                    break;
                default:
                    break;
            }
        }

        public void defib(Player owner)
        {
			SoundManager.PlaySound("defibThud", player.GlobalTransform.Translation);
            particles.Trigger("shock", owner.GlobalTransform.Translation, Color.White, (int)(100 * owner.DefibCharge));
            foreach (PhysicsComponent d in dorfs)
            {
                if (d != owner && (d.GlobalTransform.Translation - owner.GlobalTransform.Translation).LengthSquared() < 1 * 1)
                {
                    Vector3 offset = d.GlobalTransform.Translation - owner.GlobalTransform.Translation;
                    offset.Normalize();
                    offset *= 500;
                    offset.Y = 500;
                    offset *= owner.DefibCharge;
                    d.ApplyForce(offset, 1 / 60.0f);

                    if (d is NPC)
                    {
                        NPC npc = (NPC)d;
                        npc.WalkTimer.Reset(owner.DefibCharge * npc.MaxWalkTime);
                        npc.State = NPC.NPCState.Walking;
                    }
                }
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

            if (player.DefibCharge > 0.01f)
            {
                Shader.Parameters["xEnableLighting"].SetValue(true);
            }
            else
            {
                Shader.Parameters["xEnableLighting"].SetValue(false);
            }

            // discharge overfull defib
            if (player.DefibCharge >= 1.0f)
            {
                defib(player);
                player.Charging = false;
                player.DefibCharge = 0.0f;
            }

            Shader.Parameters["xLightPos"].SetValue(player.GlobalTransform.Translation);
            Shader.Parameters["xLightColor"].SetValue(new Vector4(0.25f * player.DefibCharge, 0.5f * player.DefibCharge,player.DefibCharge, 1.0f));
            Shader.Parameters["xFogColor"].SetValue(new Vector3(0, 0, 0));
            Shader.Parameters["Clipping"].SetValue(false);
            Shader.Parameters["xView"].SetValue(Camera.ViewMatrix);

            Shader.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);
            Shader.CurrentTechnique = Shader.Techniques["Textured"];

            Shader.Parameters["xSunGradient"].SetValue(SunMap);
            Shader.Parameters["xAmbientGradient"].SetValue(AmbientMap);
            Shader.Parameters["xTorchGradient"].SetValue(TorchMap);

            Shader.Parameters["xTint"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            Shader.Parameters["SelfIllumination"].SetValue(false);

            Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            ComponentManager.Render(gameTime, Camera, SpriteBatch, Game.GraphicsDevice,Shader, false);
            
            

            SpriteBatch.Begin();
            if (player.DefibCharge > 0.0f)
            {
                Drawer2D.FillRect(SpriteBatch, new Rectangle((int)(0.25 * Game.GraphicsDevice.Viewport.Width), 20,
                    (int)(0.125 * Game.GraphicsDevice.Viewport.Width), 50), new Color(255, 0, 0, 100));
                Drawer2D.FillRect(SpriteBatch, new Rectangle((int)((0.25 + 0.125) * Game.GraphicsDevice.Viewport.Width), 20,
                    (int)((0.5 - 0.125) * Game.GraphicsDevice.Viewport.Width), 50), new Color(0, 0, 0, 100));
                Drawer2D.FillRect(SpriteBatch, new Rectangle((int)(0.25 * Game.GraphicsDevice.Viewport.Width), 20,
                    (int)(0.5 * player.DefibCharge * Game.GraphicsDevice.Viewport.Width), 50), new Color(255, 200, 255, 100));
            }
            Drawer2D.DrawStrokedText(SpriteBatch, "Score: $" + player.Score, Drawer2D.DefaultFont, new Vector2(5, 5), Color.White, Color.Black);
            SpriteBatch.End();

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game.GraphicsDevice.BlendState = BlendState.Opaque;


            base.Render(gameTime);
        }

        protected void doActions()
        {
            string move = client.Read();
            string[] toks = move.Split(',');
            string command = toks[0];
            if (command == "position")
            {
                bool found = false;
                uint id = Convert.ToUInt32(toks[1], 10);
                string action = toks[2];
                float x = (float)Convert.ToDouble(toks[3]);
                float y = (float)Convert.ToDouble(toks[4]);
                float z = (float)Convert.ToDouble(toks[5]);
                float vx = (float)Convert.ToDouble(toks[6]);
                float vy = (float)Convert.ToDouble(toks[7]);
                float vz = (float)Convert.ToDouble(toks[8]);
                foreach (Person p in dorfs)
                {
                    if (p.GlobalID == id)
                    {
                        p.LocalTransform =
                            Matrix.CreateTranslation(new Vector3(x, y, z));
                        p.Velocity = new Vector3(vx, vy, vz);
                        found = true;
                    }
                }
                if (!found)
                {
                    string[] sheets = { "oldwalk", "fatwalk", "smokewalk" };
                    NPC npc = new NPC("person", new Vector3(x, y, z),
                        ComponentManager, Game.Content, Game.GraphicsDevice, sheets[(int)(rand() * 3)]);
                    npc.velocityController.MaxSpeed = 1;
                    npc.Target = new Vector3(-1, -2.1f, -11);
                    npc.Velocity = new Vector3(vx, vy, vz);
                    npc.HasMoved = true;
                    npc.IsSleeping = false;
                    dorfs.Add(npc);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


            SoundManager.Update(gameTime, Camera);

            KeyboardState keyboardState = Keyboard.GetState();

            InputManager.KeysUpdate(keyboardState);

            if (!Game.IsActive)
            {
                return;
            }

            Vector3 TargetVelocity = Vector3.Zero;

            player.PerformActions(frameEvents);

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                GeometricPrimitive.ExitGame = true;
                Game.Exit();
            }

            ComponentManager.Update(gameTime, Camera);
            List<BoundingBox> collideBox = new List<BoundingBox>();
            collideBox.Add(ground.GetBoundingBox());

            foreach (Hospital h in hospitals)
            {
                collideBox.Add(h.Component.GetBoundingBox());
            }

            foreach (PhysicsComponent d in dorfs)
            {
                d.HasMoved = true;
                d.HandleCollisions(collideBox, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            foreach (PhysicsComponent d in dorfs)
            {
                if (!(d is Player) && !d.IsDead)
                {
                    foreach (Hospital h in hospitals)
                    {
                        if (d.GetBoundingBox().Intersects(h.Component.GetBoundingBox()))
                        {
                            if (h.Owner != null)
                            {
                                h.Owner.Score += 100;
                                d.Die();
                            }
                        }  
                    }
                }
            }

            float alpha = 0.90f;
            Camera.Position *= 1.0f - alpha;
            Camera.Position += alpha * (player.GlobalTransform.Translation + new Vector3(10, 10, 0));
            Camera.Target = player.GlobalTransform.Translation;

            frameEvents = new List<Event>();

            Camera.Update(gameTime);
            base.Update(gameTime);
        }
    }
}
