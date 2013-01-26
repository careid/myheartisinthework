using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HeartGame
{
    public class OrbitCamera : Camera
    {
        public float Theta { get; set; }
        public float Phi { get; set; }
        public float Radius { get; set; }
        public float CameraMoveSpeed { get; set; }

        private float m_thetaOnClick = 0.0f;
        private float m_phiOnClick = 0.0f;
        private float m_radiusOnClick = 0.0f;
        private Vector3 m_targetOnClick = Vector3.Zero;
        private bool m_isLeftPressed = false;
        private bool m_isRightPressed = false;
        private Point m_mouseCoordsOnClick;
        private bool m_hasMoved = false;
        private bool m_hasZoomed = false;
        public GraphicsDevice Graphics { get; set; }

        private int m_lastWheel = 1;
        private Timer m_moveTimer = new Timer(0.25f, true);

        public OrbitCamera(GraphicsDevice graphics, float theta, float phi, float radius, Vector3 target, Vector3 position, float fov, float aspectRatio, float nearPlane, float farPlane) :
            base(target, position, fov, aspectRatio, nearPlane, farPlane)
        {
            Theta = theta;
            Phi = phi;
            Radius = radius;
            CameraMoveSpeed = 0.1f;
            Graphics = graphics;
            m_lastWheel = Mouse.GetState().ScrollWheelValue;
        }


        public override void Update(GameTime time)
        {
            MouseState mouse = Mouse.GetState();
            KeyboardState keys = Keyboard.GetState();

            int edgePadding = 20;

            bool stateChanged = false;

            if (keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift))
            {
                if (!m_isLeftPressed && mouse.LeftButton == ButtonState.Pressed)
                {
                    m_isLeftPressed = true;
                    stateChanged = true;
                }
                else if (mouse.LeftButton == ButtonState.Released)
                {
                    m_isLeftPressed = false;
                }

                if (!m_isRightPressed && mouse.RightButton == ButtonState.Pressed)
                {
                    m_isRightPressed = true;
                    stateChanged = true;
                }
                else if (mouse.RightButton == ButtonState.Released)
                {
                    m_isRightPressed = false;
                }



                if (stateChanged)
                {
                    m_thetaOnClick = Theta;
                    m_phiOnClick = Phi;
                    m_radiusOnClick = Radius;
                    m_mouseCoordsOnClick = new Point(mouse.X, mouse.Y);
                    m_targetOnClick = Target;
                    m_hasMoved = false;
                    m_hasZoomed = false;
                }


                float diffX = mouse.X - m_mouseCoordsOnClick.X;
                float diffY = mouse.Y - m_mouseCoordsOnClick.Y;

                if (m_isLeftPressed && !m_isRightPressed && !m_hasMoved && !m_hasZoomed)
                {
                    Theta = m_thetaOnClick - (0.01f * diffX);
                    Phi = m_phiOnClick - 0.01f * diffY;

                    if (Phi < -1.5f)
                    {
                        Phi = -1.5f;
                    }
                    else if (Phi > 1.57f)
                    {
                        Phi = 1.5f;
                    }


                }
                else if (m_isRightPressed && m_isLeftPressed && !m_hasMoved)
                {
                    Radius = m_radiusOnClick - 1.5f * diffY;

                    if (Radius < 1)
                    {
                        Radius = 1;
                    }
                    m_hasZoomed = true;
                }
                else if (m_isRightPressed && !m_isLeftPressed && !m_hasZoomed)
                {
                    Vector3 forward = (Target - Position);
                    forward.Normalize();
                    Vector3 right = Vector3.Cross(forward, UpVector);
                    Vector3 up = Vector3.Cross(right, forward);
                    Target = m_targetOnClick + up * (0.5f * diffY) + right * (-0.5f * diffX);
                    m_hasMoved = true;
                }
            }

            if (keys.IsKeyDown(Keys.W))
            {
                Vector3 forward = (Target - Position);
                forward.Normalize();

                Target = Target + forward * CameraMoveSpeed;
            }
            else if (keys.IsKeyDown(Keys.S))
            {
                Vector3 forward = (Target - Position);
                forward.Normalize();

                Target = Target - forward * CameraMoveSpeed;
            }

            if (keys.IsKeyDown(Keys.A))
            {
                Vector3 forward = (Target - Position);
                forward.Normalize();
                Vector3 right = Vector3.Cross(forward, UpVector);
                Target = Target - right * CameraMoveSpeed;
            }
            else if (keys.IsKeyDown(Keys.D))
            {
                Vector3 forward = (Target - Position);
                forward.Normalize();
                Vector3 right = Vector3.Cross(forward, UpVector);
                Target = Target + right * CameraMoveSpeed;
            }



            if (!keys.IsKeyDown(Keys.LeftShift) && !keys.IsKeyDown(Keys.RightShift))
            {
                if (mouse.X < edgePadding || mouse.X > Graphics.Viewport.Width - edgePadding)
                {
                    if (m_moveTimer.HasTriggered)
                    {
                        float dir = 0.0f;

                        if (mouse.X < edgePadding)
                        {
                            dir = edgePadding - mouse.X;
                        }
                        else
                        {
                            dir = (Graphics.Viewport.Width - edgePadding) - mouse.X;
                        }

                        dir *= 0.05f;

                        Vector3 forward = (Target - Position);
                        forward.Normalize();
                        Vector3 right = Vector3.Cross(forward, UpVector);
                        Vector3 delta = right * CameraMoveSpeed * dir;
                        delta.Y = 0;
                        Target = Target - delta;
                    }
                    else
                    {
                        m_moveTimer.Update(time);
                    }
                }
                else if (mouse.Y < edgePadding || mouse.Y > Graphics.Viewport.Height - edgePadding)
                {
                    if (m_moveTimer.HasTriggered)
                    {
                        float dir = 0.0f;

                        if (mouse.Y < edgePadding)
                        {
                            dir = -(edgePadding - mouse.Y);
                        }
                        else
                        {
                            dir = -((Graphics.Viewport.Height - edgePadding) - mouse.Y);
                        }

                        dir *= 0.1f;

                        Vector3 forward = (Target - Position);
                        forward.Normalize();
                        Vector3 right = Vector3.Cross(forward, UpVector);
                        Vector3 up = Vector3.Cross(right, forward);
                        Vector3 delta = up * CameraMoveSpeed * dir;
                        delta.Y = 0;
                        Target = Target - delta;
                    }
                    else
                    {
                        m_moveTimer.Update(time);
                    }
                }
                else
                {
                    m_moveTimer.Reset(m_moveTimer.TargetTimeSeconds);
                        
                }
            }

            if (mouse.ScrollWheelValue != m_lastWheel)
            {
                Vector3 delta = new Vector3(0, mouse.ScrollWheelValue - m_lastWheel, 0);
                Target = Target + delta * 0.01f;
                m_lastWheel = mouse.ScrollWheelValue;
            }

            UpdateBasisVectors();
            base.Update(time);
        }

        public void UpdateBasisVectors()
        {
            //Vector3 p = new Vector3();
            //p.Z = (float)(Radius * Math.Cos(Theta) * Math.Sin(Phi));
            //p.Y = (float)(Radius * Math.Cos(Phi));
            //p.X = (float)(Radius * Math.Sin(Theta) * Math.Sin(Phi));

            Position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(Theta, Phi, 0)) * Radius + Target;

            /*
            Vector3 u = new Vector3();
            u.Y = (float)(-Math.Cos(Theta) * Math.Cos(Phi));
            u.Z = (float)(Math.Sin(Phi));
            u.X = (float)(-Math.Sin(Theta) * Math.Cos(Phi));
            
            UpVector = u;
             */
             
        }
    }
}
