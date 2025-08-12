using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.Utils
{
    public static class DebugRender
    {
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos">Circle center</param>
        /// <param name="radius">Radius</param>
        /// <param name="numVerts">How many vertices. More verts result in a more round circle.</param>
        /// <param name="deltaTime">How long it last until it gets deleted. Best to use engine tick time since then the
        /// circle gets redrawn every frame and can be updated if any changes are made</param>
        public static void RenderCircle(Vec3 pos, float radius, int numVerts, float deltaTime)
        {
            float delta = 2f * TaleWorlds.Library.MathF.PI / numVerts;
            Vec3 oldPoint = pos;
            
            for (int i = 0; i <= numVerts; i++)
            {
                // circle math, don't worry about it
                float alpha = i * delta;
                float x = TaleWorlds.Library.MathF.Cos(alpha);
                float y = TaleWorlds.Library.MathF.Sin(alpha);
                Vec3 point = new Vec3(x, y, 0f);
                point = pos + (radius * point);
                
                // jump over first vert
                if (i != 0)
                {
                    MBDebug.RenderDebugLine(oldPoint, point - oldPoint, UInt32.MaxValue, false, deltaTime);
                }
                
                oldPoint = point;
            }
        }
    }
}