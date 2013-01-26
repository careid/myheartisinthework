using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    class SkyRenderer
    {
        public TextureCube SkyTexture { get; set; }
        public TextureCube NightTexture { get; set; }
        public TextureCube SunMoon { get; set; }
        public Model SkyMesh { get; set; }
        public Texture2D SkyGrad { get; set; }
        public Effect SkyEffect { get; set; }
        public float TimeOfDay { get; set; }
        public float CosTime { get; set; }

        public SkyRenderer(TextureCube skyTexture, TextureCube nightTexture, TextureCube sunMoon, Texture2D skyGrad, Model skyMesh, Effect skyEffect)
        {
            SkyTexture = skyTexture;
            NightTexture = nightTexture;
            SunMoon = sunMoon;
            SkyMesh = skyMesh;
            SkyEffect = skyEffect;
            SkyGrad = skyGrad;
            SkyEffect.Parameters["SkyboxTexture"].SetValue(SkyTexture);
            SkyEffect.Parameters["TintTexture"].SetValue(SkyGrad);
            TimeOfDay = 0.0f;
            CosTime = 0.0f;

            foreach (ModelMesh mesh in SkyMesh.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = SkyEffect;
                }
            }
        }

        public void Render(GameTime time, GraphicsDevice device, Camera camera)
        {
            RenderNightSky(time, device, camera);
            RenderDaySky(time, device, camera);
            RenderSunMoon(time, device, camera);
        }

        public void RenderDaySky(GameTime time, GraphicsDevice device, Camera camera)
        {
            SkyEffect.Parameters["SkyboxTexture"].SetValue(SkyTexture);
            SkyEffect.Parameters["ViewMatrix"].SetValue(camera.ViewMatrix);
            SkyEffect.Parameters["ProjectionMatrix"].SetValue(camera.ProjectionMatrix);
            SkyEffect.Parameters["xTransparency"].SetValue(1.0f - (float)Math.Pow(TimeOfDay, 2));
            SkyEffect.Parameters["xRot"].SetValue(Matrix.CreateRotationY((float)time.TotalGameTime.TotalSeconds * 0.005f));
            SkyEffect.CurrentTechnique = SkyEffect.Techniques[0];
            SkyEffect.Parameters["xTint"].SetValue(TimeOfDay);
            foreach (ModelMesh mesh in SkyMesh.Meshes)
            {
                mesh.Draw();
            }

        }

        public void RenderNightSky(GameTime time, GraphicsDevice device, Camera camera)
        {
            SkyEffect.Parameters["SkyboxTexture"].SetValue(NightTexture);
            SkyEffect.Parameters["ViewMatrix"].SetValue(camera.ViewMatrix);
            SkyEffect.Parameters["ProjectionMatrix"].SetValue(camera.ProjectionMatrix);
            SkyEffect.Parameters["xTransparency"].SetValue(TimeOfDay);
            SkyEffect.Parameters["xRot"].SetValue(Matrix.CreateRotationZ(-(CosTime + 0.5f)));
            SkyEffect.Parameters["xTint"].SetValue(0.0f);
            SkyEffect.CurrentTechnique = SkyEffect.Techniques[0];
            foreach (ModelMesh mesh in SkyMesh.Meshes)
            {
                mesh.Draw();
            }
        }

        public void RenderSunMoon(GameTime time, GraphicsDevice device, Camera camera)
        {
            SkyEffect.Parameters["SkyboxTexture"].SetValue(SunMoon);
            SkyEffect.Parameters["ViewMatrix"].SetValue(camera.ViewMatrix);
            SkyEffect.Parameters["ProjectionMatrix"].SetValue(camera.ProjectionMatrix);
            SkyEffect.Parameters["xTransparency"].SetValue(1.0f);
            SkyEffect.Parameters["xRot"].SetValue(Matrix.CreateRotationZ(-(CosTime + 0.5f *(float)Math.PI)));
            SkyEffect.CurrentTechnique = SkyEffect.Techniques[1];
            SkyEffect.Parameters["xTint"].SetValue(0.0f);
            foreach (ModelMesh mesh in SkyMesh.Meshes)
            {
                mesh.Draw();
            }
        }
    }
}
