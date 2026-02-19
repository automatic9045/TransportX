using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

using TransportX.Extensions.Utilities;

namespace TransportX.Scripting.Commands
{
    public class DirectionalLight
    {
        private readonly ScriptWorld World;

        public Vector3 Color => World.DirectionalLight.Color;
        public Vector3 Direction => World.DirectionalLight.Direction;
        public float Intensity => World.DirectionalLight.Intensity;

        internal DirectionalLight(ScriptWorld world)
        {
            World = world;
        }

        public void SetColor(Vector3 value)
        {
            World.SetDirectionalLight(World.DirectionalLight with
            {
                Color = value,
            });
        }

        public void SetColor(string colorText)
        {
            Color color = System.Drawing.Color.White;
            try
            {
                color = ColorTranslator.FromHtml(colorText);
            }
            catch
            {
                ScriptError error = new(ErrorLevel.Error, $"カラーコード '{colorText}' は無効です。");
                World.ErrorCollector.Report(error);
            }

            SetColor(color.ToVector3());
        }

        public void SetDirection(Vector3 value)
        {
            World.SetDirectionalLight(World.DirectionalLight with
            {
                Direction = Vector3.Normalize(value),
            });
        }

        public void SetDirection(double x, double y, double z)
        {
            Vector3 direction = new((float)x, (float)y, (float)z);
            SetDirection(direction);
        }

        public void SetIntensity(double value)
        {
            World.SetDirectionalLight(World.DirectionalLight with
            {
                Intensity = (float)value,
            });
        }
    }
}
