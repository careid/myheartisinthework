﻿using System;
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

        NOP = 10,
        SCORE = 11,

        ESC_PRESS = 12,
        ESC_RELEASE = 13,
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
        public List<Person> dorfs = new List<Person>();
        public List<Player> players = new List<Player>();
        public List<Hospital> hospitals = new List<Hospital>();
        public Player player;
        public Drawer2D drawer2D;
        public SoundManager sounds;
        public ParticleManager particles;
        protected Client client;
        protected bool online;
        protected Notification notification;
        protected Dictionary<string, string> wordsDict;
        protected string[] wordsArray;
        protected int MapWidth { get; set; }
        protected int MapHeight { get; set; }

        private float rand()
        {
            return (float)RandomHelper.random.NextDouble();
        }

        private float detRand(Random r)
        {
            return (float)r.NextDouble();
        }

        public PlayState(Game1 game, GameStateManager GSM) :
            base(game, "PlayState", GSM)
        {
            MapHeight = 40;
            MapWidth = 40;
            Player.defib = new Player.defibCallbackType(defib);
            online = true; // DO NOT CHANGE, offline mode is now detected when server connection is refused
            SoundManager.Content = game.Content;
            Camera = new OrbitCamera(Game.GraphicsDevice, 0, 0, 0.001f, new Vector3(0, 15, 0), new Vector3(-10, 10, 0), (float)Math.PI * 0.25f, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);
            ComponentManager = new ComponentManager();
            ComponentManager.RootComponent = new LocatableComponent(ComponentManager, "root", null, Matrix.Identity, Vector3.Zero, Vector3.Zero);

            notification = null;
            particles = new ParticleManager(ComponentManager);

            EmitterData testData = new EmitterData();
            testData.AngularDamping = 1.0f;
            List<Point> frm = new List<Point>();
            frm.Add(new Point(0, 0));
            frm.Add(new Point(1, 0));
            frm.Add(new Point(0, 1));
            frm.Add(new Point(1, 1));
            testData.Animation = new Animation(Game.GraphicsDevice, Game.Content.Load<Texture2D>("electricity"),
                "electricity", 32, 32, frm, true, Color.White, 20f, 1.0f, 1.0f, false);
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

            wordsDict = new Dictionary<string, string>();
            wordsDict.Add("MONSTER SAVE", "monstersave");
            wordsDict.Add("DOUBLE SAVE", "doublesave");
            wordsDict.Add("TRIPLE SAVE", "triplesave");

            drawer2D = new Drawer2D(game.Content, game.GraphicsDevice);

            client = new Client(online);
            string name = client.Connect();
            if (name == "error")
            {
                Console.Out.WriteLine("connection refused");
                name = "0";
                online = false;
            }
            
            Hospital hospital1 = new Hospital(new Vector3(-10, 0, -10), new Vector3(4, 2, 3), ComponentManager, Game.Content, Game.GraphicsDevice, "hospital", Color.Red, new Point(2, 0));
            Hospital hospital2 = new Hospital(new Vector3(12, 0, 12), new Vector3(4, 2, 3), ComponentManager, Game.Content, Game.GraphicsDevice, "hospital", Color.Blue, new Point(1, 0));
            hospitals.Add(hospital1);
            hospitals.Add(hospital2);

            Random r = new Random(1);
            for (int i = 0; i < 20; i++) // fnord
            {
                NPC npc;
                switch ((int)(detRand(r) * 2))
                {
                    case (0):
                        npc = new Smoker(new Vector3(detRand(r) * 9, 5, detRand(r) * 10), ComponentManager,
                                        Game.Content, Game.GraphicsDevice);
                        break;
                    case (1):
                        npc = new Fatter(new Vector3(detRand(r) * 9, 5, detRand(r) * 10), ComponentManager,
                                        Game.Content, Game.GraphicsDevice);
                        break;
                    case (2):
                        npc = new Older(new Vector3(detRand(r) * 9, 5, detRand(r) * 10), ComponentManager,
                                        Game.Content, Game.GraphicsDevice);
                        break;
                    default:
                        /* graphics don't exist yet, never reached */
                        npc = new Worker(new Vector3(detRand(r) * 9, 5, detRand(r) * 10 - 0), ComponentManager,
                                        Game.Content, Game.GraphicsDevice);
                        break;
                }
                npc.velocityController.MaxSpeed = 1;
                npc.SetTag((i+1000).ToString());
                int al = (int)(detRand(r) * 2);
                npc.Team = hospitals[al];
                npc.Velocity = new Vector3(0f, -0.5f, 0f);
                npc.HasMoved = true;
                dorfs.Add(npc);
            }

            player = new Player(name, new Vector3(rand() * 10 - 5, 5, rand() * 10 - 5),
                                ComponentManager, Game.Content, Game.GraphicsDevice, "surgeonwalk");
            player.Velocity = new Vector3(0f, -0.5f, 0f);
            player.HasMoved = true;
            dorfs.Add(player);
            players.Add(player);

            VelocityController velocityController3 = new VelocityController(player);
            velocityController3.IsTracking = true;

            Vector3 boundingBoxPos = new Vector3(0, -2, 0);
            Vector3 boundingBoxExtents = new Vector3(MapWidth, 4, MapHeight);
            Vector3 boundingBoxMin = boundingBoxPos - boundingBoxExtents * 0.5f;
            Vector3 boundingBoxMax = boundingBoxPos + boundingBoxExtents * 0.5f;

            ground = (LocatableComponent)EntityFactory.GenerateBlankBox(new BoundingBox(boundingBoxMin, boundingBoxMax), ComponentManager, Game.Content, Game.GraphicsDevice, "newground", Point.Zero, Point.Zero, 128, 128);

            /*
            Hospital hospital1 = new Hospital(new Vector3(-1, 0, -11), new Vector3(4, 2, 3), ComponentManager, Game.Content, Game.GraphicsDevice, "hospital", Color.Red, new Point(2, 0));
            Hospital hospital2 = new Hospital(new Vector3(5, 0, 5), new Vector3(2, 7, 2), ComponentManager, Game.Content, Game.GraphicsDevice, "hospital", Color.Green, new Point(1, 0));
            hospitals.Add(hospital1);
            hospitals.Add(hospital2);
             */

            if (Convert.ToInt32(name) % 2 == 0)
                player.team = hospital1;
            else
                player.team = hospital2;

            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Shader = Game.Content.Load<Effect>("Hargraves");

            SunMap = Game.Content.Load<Texture2D>("sungradient");
            AmbientMap = Game.Content.Load<Texture2D>("ambientgradient");
            TorchMap = Game.Content.Load<Texture2D>("torchgradient");

            AddNotification("Mash Space to Begin");
        }

        public void pressKey(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    client.Write(encodePerson(player,Event.W_PRESS.ToString()));
                    break;
                case Keys.A:
                    client.Write(encodePerson(player,Event.A_PRESS.ToString()));
                    break;
                case Keys.S:
                    client.Write(encodePerson(player,Event.S_PRESS.ToString()));
                    break;
                case Keys.D:
                    client.Write(encodePerson(player,Event.D_PRESS.ToString()));
                    break;
                case Keys.Space:
                    client.Write(encodePerson(player,Event.SPACE_PRESS.ToString()));
                    break;
                case Keys.Escape:
                    if (client.t != null)
                        client.t.Abort();
                    // EXPERIMENTAL RESTART
                    //StateManager.States["PlayState"] = new PlayState(Game, StateManager);
                    GeometricPrimitive.ExitGame = true;
                    Game.Exit();
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
                    client.Write(encodePerson(player,Event.W_RELEASE.ToString()));
                    break;
                case Keys.A:
                    client.Write(encodePerson(player,Event.A_RELEASE.ToString()));
                    break;
                case Keys.S:
                    client.Write(encodePerson(player,Event.S_RELEASE.ToString()));
                    break;
                case Keys.D:
                    client.Write(encodePerson(player,Event.D_RELEASE.ToString()));
                    break;
                case Keys.Space:
                    client.Write(encodePerson(player,Event.SPACE_RELEASE.ToString()));
                    break;
                default:
                    break;
            }
        }

        public void AddNotification(string text, Hospital team= null, float time = 1.0f)
        {
            notification = new Notification(text, time);
            if (wordsDict.ContainsKey(text))
            {
                SoundManager.Play2DSound(wordsDict[text]);
            }
        }

        public void defib(Player owner)
        {
			SoundManager.PlaySound("defibThud", owner.GlobalTransform.Translation);
            particles.Trigger("shock", owner.GlobalTransform.Translation, Color.White, (int)(100 * owner.DefibCharge));

            if (owner != player)
                return;

            foreach (Person d in dorfs)
            {
                if (d != owner && (d.GlobalTransform.Translation - owner.GlobalTransform.Translation).LengthSquared() < 1 * 1)
                {
                    if (d.GetType() != typeof(Player))
                    {
                        ((NPC)d).Team = owner.team;
                    }
                    Vector3 offset = d.GlobalTransform.Translation - owner.GlobalTransform.Translation;
                    offset.Y = 0;
                    if (offset.X != 0 || offset.Z != 0)
                    {
                        offset.Normalize();
                        offset *= 10;
                    }
                    else
                    {
                        offset = new Vector3(0);
                    }

                    offset.Y = 10;
                    offset *= 1.2f*owner.DefibCharge;
                    //d.Velocity = offset;

                    Vector3 oldTranslation = d.LocalTransform.Translation;
                    d.LocalTransform = Matrix.CreateTranslation(new Vector3(oldTranslation.X, oldTranslation.Y + 0.25f, oldTranslation.Z));

                    if (d is NPC)
                    {
                        NPC npc = (NPC)d;
                        npc.WalkTimer.Reset(owner.DefibCharge * npc.MaxWalkTime);

                        npc.State = "walk";
                    }

                    if (owner == player)
                        client.Write(encodePerson(d, Event.NOP.ToString(), offset));
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
            Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Game.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            if (player.DefibCharge > 0.01f)
            {
                Shader.Parameters["xEnableLighting"].SetValue(true);
            }
            else
            {
                Shader.Parameters["xEnableLighting"].SetValue(false);
            }

            // discharge overfull defibrillator
            if (player.DefibCharge >= 1.0f)
            {
                client.Write(encodePerson(player, Event.SPACE_RELEASE.ToString()));
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
                int barX = (int)((0.25 + 0.5 * player.DefibCharge) * Game.GraphicsDevice.Viewport.Width);
                if (player.DefibCharge < 0.25)
                {
                    Drawer2D.FillRect(SpriteBatch, new Rectangle(barX, 20,
                        (int)((0.25 + 0.125) * Game.GraphicsDevice.Viewport.Width - barX), 50), new Color(255, 0, 0, 100));
                }
                int shadedX = (int)((0.25 + 0.125) * Game.GraphicsDevice.Viewport.Width);
                if (shadedX < barX)
                    shadedX = barX;
                Drawer2D.FillRect(SpriteBatch, new Rectangle(shadedX, 20,
                    (int)(0.75 * Game.GraphicsDevice.Viewport.Width - shadedX), 50), new Color(0, 0, 0, 100));
                Drawer2D.FillRect(SpriteBatch, new Rectangle((int)(0.25 * Game.GraphicsDevice.Viewport.Width), 20,
                    (int)(0.5 * player.DefibCharge * Game.GraphicsDevice.Viewport.Width), 50), new Color((int)(100 + 150 * player.DefibCharge), (int)(100 + 150 * player.DefibCharge), 255, 100));
            }
            string redName = "US";
            string blueName = "THEM";
            int redX = 5;
            int blueX = 1000;
            if (player.team == hospitals[1])
            {
                redName = "THEM";
                blueName = "US";
                redX = 1000;
                blueX = 5;
            }

            Drawer2D.DrawStrokedText(SpriteBatch, redName + " $" + hospitals[0].Score, Drawer2D.DefaultFont, new Vector2(redX, 5), Color.White, Color.Red);
            Drawer2D.DrawStrokedText(SpriteBatch, blueName + " $" + hospitals[1].Score, Drawer2D.DefaultFont, new Vector2(blueX, 5), Color.White, Color.Blue);

            if (notification != null)
            {
                Drawer2D.DrawStrokedText(SpriteBatch, notification.text, Drawer2D.BigFont, new Vector2(300, 300), Color.White, new Color(1.0f, 0, 0, notification.displayLength));
            }

            drawer2D.Render(SpriteBatch, Camera, Game.GraphicsDevice.Viewport);
            
            SpriteBatch.End();

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game.GraphicsDevice.BlendState = BlendState.Opaque;

            base.Render(gameTime);
        }

        public NPC GetClosestDeadNPC(Player player)
        {
            float closestDist = 999999;
            NPC closestNPC = null;
            foreach (Person p in dorfs)
            {
                if (p is NPC)
                {
                    NPC npc = (NPC)p;
                    if (npc.State == "dead")
                    {
                        float dist = (npc.GlobalTransform.Translation - player.GlobalTransform.Translation).LengthSquared();
                        if (dist < closestDist)
                        {
                            closestNPC = npc;
                            closestDist = dist;
                        }
                    }
                }
            }

            return closestNPC;

        }

        protected void doActions()
        {
            string move = client.Read();
            if (move.Length == 0)
            {
                return;
            }
            string[] toks = move.Split(',');
            string command = toks[0];
            if (command == "general")
            {
                bool found = false;
                int id = Convert.ToInt32(toks[1], 10);
                string action = toks[2];
                float x = (float)Convert.ToDouble(toks[3]);
                float y = (float)Convert.ToDouble(toks[4]);
                float z = (float)Convert.ToDouble(toks[5]);
                float vx = (float)Convert.ToDouble(toks[6]);
                float vy = (float)Convert.ToDouble(toks[7]);
                float vz = (float)Convert.ToDouble(toks[8]);
                int team = (int)Convert.ToInt32(toks[9]);
                foreach (Person p in ComponentManager.FilterComponentsWithTag(toks[1], dorfs))
                {
                    Event e = (Event)Enum.Parse(typeof(Event), action, true);
                    if (e == Event.SCORE)
                    {
                        p.team.Score += 70000;
                        p.team.NumPatients++;
                        SoundManager.PlaySound("kaChing", p.GlobalTransform.Translation);
                        p.Die();
                        string[] choices = wordsDict.Keys.ToArray();
                        AddNotification(choices[((int)(rand() * choices.Length))], p.team);
                    }
                    p.PerformAction(e);
                    p.LocalTransform =
                        Matrix.CreateTranslation(new Vector3(x, y, z));
                    p.Velocity = new Vector3(vx, vy, vz);
                    p.team = hospitals[team];
                    found = true;
                }
                if (!found)
                {
                    Person p;
                    if (id < 1000)
                    {
                        p = new Player(toks[1], new Vector3(x, y, z),
                            ComponentManager, Game.Content, Game.GraphicsDevice, "surgeonwalk");
                        VelocityController velocityController = new VelocityController(p);
                        velocityController.IsTracking = true;

                        p.team = hospitals[id % 2];
                        p.Velocity = new Vector3(0, -0.5f, 0);
                        p.HasMoved = true;
                        dorfs.Add(p);
                        players.Add((Player)p);
                    }
                }
            }
            else if (command == "sendpos")
            {
                client.Write(encodePerson(player, Event.NOP.ToString()));
            }
            else if (command == "exit")
            {
                pressKey(Keys.Escape);
                releaseKey(Keys.Escape);
            }
        }

        protected string encodePerson(Person p, string action)
        {
            return encodePerson(p, action, p.Velocity);
        }

        protected string encodePerson(Person p, string action, Vector3 vel)
        {
            int team = 0;
            if (hospitals[0] == p.team)
            {
                team = 0;
            }
            else
            {
                team = 1;
            }
            string[] info =
            {"general", 
                p.tag, 
                action,
                p.LocalTransform.Translation.X.ToString(),
                p.LocalTransform.Translation.Y.ToString(),
                p.LocalTransform.Translation.Z.ToString(),
                vel.X.ToString(),
                vel.Y.ToString(),
                vel.Z.ToString(),
                team.ToString()};
            string msg = String.Join(",", info);
            return msg;
        }

        public bool PatientsExist()
        {
            foreach (PhysicsComponent p in dorfs)
            {
                if (p is NPC && !p.IsDead)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


            doActions();

            KeyboardState keyboardState = Keyboard.GetState();
            InputManager.KeysUpdate(keyboardState);

            if (online)
            {
                foreach (Player p in players)
                {
                    if (!p.isReady)
                        return;
                }
                if (players.Count() < 2)
                    return;
            }
            
            SoundManager.Update(gameTime, Camera);

            if (!Game.IsActive)
            {
                return;
            }

            Vector3 TargetVelocity = Vector3.Zero;

            ComponentManager.Update(gameTime, Camera);
            List<BoundingBox> collideBox = new List<BoundingBox>();
            collideBox.Add(ground.GetBoundingBox());

            foreach (Hospital h in hospitals)
            {
                collideBox.Add(h.Component.GetBoundingBox());
                //collideBox.Add(h.InvisibleBox.GetBoundingBox());
            }

            foreach (Person d in dorfs)
            {
                if (d.team != null)
                {
                    d.teamCircle.IsVisible = (d is Player)? true : false;
                    d.teamCircle.Tint = d.team.Color;
                }
                else
                {
                    d.teamCircle.IsVisible = false;
                }

                d.HasMoved = true;
                d.HandleCollisions(collideBox, (float)gameTime.ElapsedGameTime.TotalSeconds);

                if (d.GlobalTransform.Translation.X > MapWidth / 2 || d.GlobalTransform.Translation.X < -MapWidth / 2 || d.GlobalTransform.Translation.Z > MapHeight /2 || d.GlobalTransform.Translation.Z < -MapWidth/2 || d.GlobalTransform.Translation.Y < 0)
                {
                    float x = Math.Max(Math.Min(d.GlobalTransform.Translation.X, MapWidth / 2), -MapWidth / 2);
                    float z = Math.Max(Math.Min(d.GlobalTransform.Translation.Z, MapHeight / 2), -MapHeight / 2);
                    float y = Math.Max(d.GlobalTransform.Translation.Y, 0);
                    d.LocalTransform 
                        = Matrix.CreateTranslation(new Vector3(x, d.GlobalTransform.Translation.Y, z));
                    d.Velocity *= -0.1f;
                }
            }

            foreach (Person d in dorfs)
            {
                if (!(d is Player) && !d.IsDead)
                {
                    Hospital h = player.team;
                    if (d.GetBoundingBox().Intersects(h.Component.GetBoundingBox()) && d.team == h)
                    {
                        client.Write(encodePerson(d, Event.SCORE.ToString()));
                    }
                }
            }

            if (notification != null)
            {
                notification.displayLength -= dt;
                if (notification.displayLength < 0)
                {
                    notification = null;
                }
            }

            float alpha = 0.05f;
            Camera.Position *= 1.0f - alpha;
            Camera.Position += alpha * (player.GlobalTransform.Translation + new Vector3(10, 10, 0));
            Camera.Target = alpha * player.GlobalTransform.Translation + (1.0f - alpha) * Camera.Target; 

            Camera.Update(gameTime);

            NPC closest = GetClosestDeadNPC(player);

            if (closest != null)
            {
                float x = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5) * 0.5f + 0.5f);
                Color flash = new Color(1.0f, 1.0f, 1.0f, x);
                Color back = new Color(0, 0, 0, x);
                Drawer2D.DrawText("Revive! (Hold Space)", closest.GlobalTransform.Translation, flash, back);
            }

            if (!PatientsExist())
            {
                StateManager.SwitchState("WinState");
            }


            base.Update(gameTime);
        }

        public string name { get; set; }
    }
}
