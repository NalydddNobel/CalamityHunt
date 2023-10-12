using System;
using CalamityHunt.Common.Systems;
using CalamityHunt.Content.Bosses.Goozma;
using CalamityHunt.Content.Bosses.Goozma.Projectiles;
using CalamityHunt.Content.Items.Masks;
using Microsoft.Xna.Framework;
using ReLogic.Peripherals.RGB;
using Terraria;
using Terraria.GameContent.RGB;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Chroma
{
    public class GoozmaAuricSoulShader : ChromaShader
    {
        public class Condition : ChromaCondition
        {
            public override bool IsActive()
            {
                return GoozmaSystem.GoozmaAuricSoulItemIndex != -1 || Pulse > 0f;
            }
        }

        private static Vector3[] _colorMap;
        public static float Pulse;

        public override void Update(float elapsedTime)
        {
            if (Pulse > 0f)
            {
                Pulse -= 0.035f;
                if (Pulse < 0f)
                {
                    Pulse = 0f;
                }
            }
        }

        public override void Process(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            _colorMap = Array.ConvertAll(SlimeUtils.GoozColors, (color) => color.ToVector3());
            base.Process(device, fragment, quality, time);
        }

        private void DrawPulse(Vector2 center, RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            float intensity = Pulse;
            if (intensity > 0.9f)
            {
                intensity = 1f - (intensity - 0.9f) / 0.1f;
            }
            else if (intensity < 0.6f)
            {
                intensity /= 0.6f;
            }
            for (int i = 0; i < fragment.Count; i++)
            {
                Vector2 keyPosition = fragment.GetCanvasPositionOfIndex(i);
                Vector2 difference = keyPosition - center;
                float distance = difference.Length() * (0.3f + 1.7f * (1f - intensity));
                float rotation = difference.ToRotation();

                float progress = Math.Abs(distance * 4f - time * 2f) % _colorMap.Length;
                int index = _colorMap.Length - 1 - (int)progress;
                int nextIndex = (index + 1) % _colorMap.Length;

                float lerpAmount = progress % 1f;
                if (index == _colorMap.Length - 2)
                {
                    lerpAmount = 1f;
                }

                var color = Vector3.Lerp(_colorMap[nextIndex], _colorMap[index], lerpAmount);
                fragment.SetColor(i, new(color * 0.7f * (1f - MathF.Pow(1f - intensity, 2f)) * (1f - MathF.Pow(Math.Clamp(distance / 2f, 0f, 1f), 2f)), 1f));
            }
        }

        [RgbProcessor(new EffectDetailLevel[] { EffectDetailLevel.High })]
        private void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            Vector2 center = fragment.CanvasCenter;
            center += time.ToRotationVector2() * MathF.Sin(time * (float)Math.E) * 1f;
            DrawPulse(center, device, fragment, quality, time);
            FixColors(fragment);
        }

        [RgbProcessor(new EffectDetailLevel[] { EffectDetailLevel.Low })]
        private void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            Vector2 center = new Vector2(1.7f, 0.5f);
            DrawPulse(center, device, fragment, quality, time);
            FixColors(fragment);
        }

        private void FixColors(Fragment fragment)
        {
            for (int i = 0; i < fragment.Count; i++)
            {
                var color = fragment.Colors[i];
                color.X = Math.Clamp(color.X, 0f, 1f);
                color.Y = Math.Clamp(color.Y, 0f, 1f);
                color.Z = Math.Clamp(color.Z, 0f, 1f);
                color.W = Math.Clamp(color.W, 0f, 1f);
                fragment.SetColor(i, color);
            }
        }
    }
}
