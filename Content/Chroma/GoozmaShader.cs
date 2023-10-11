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
    public class GoozmaShader : ChromaShader
    {
        public class Condition : ChromaCondition
        {
            public override bool IsActive()
            {
                return GoozmaSystem.GoozmaBossIndex != -1;
            }
        }

        public enum ShaderState
        {
            Normal = 0,
            Laser = 1,
            CrimsonSlime = 2,
            CorruptionSlime = 3,
            HallowedSlime = 4,
            AstralSlime = 5,
            TOUCHME = 6,
            PhaseTransition = 7,
            Entrance = 8,
            Death = 9
        }

        private static Vector3[] _colorMap;

        public ShaderState state;
        public float pulseTime;
        public float pulseSpeed;
        public float pulseColorMultiplier;
        public float whiteLuminance;
        public float textScroll;
        public float flash;

        private void UpdateColorMap()
        {
            _colorMap = SlimeUtils.GoozColorsVector3;
        }

        private void UpdateState(Goozma goozma)
        {
            state = ShaderState.Normal;
            if ((int)goozma.Phase == 1)
            {
                state = ShaderState.PhaseTransition;
                return;
            }
            else if ((int)goozma.Phase == 3)
            {
                state = ShaderState.Death;
                return;
            }
            else if ((int)goozma.Phase == -1)
            {
                state = ShaderState.Entrance;
                return;
            }
            else if (goozma.Phase == -2 || goozma.Attack == (int)Goozma.AttackList.FusionRay && FindProjectile(ModContent.ProjectileType<FusionRay>()) != null)
            {
                state = ShaderState.Laser;
                return;
            }
            else if (goozma.NPC.ai[3] >= 0f && goozma.ActiveSlime.active)
            {
                var activeSlime = goozma.ActiveSlime;
                if (activeSlime.ModNPC is EbonianBehemuck)
                {
                    state = ShaderState.CorruptionSlime;
                    return;
                }
                else if (activeSlime.ModNPC is CrimulanGlopstrosity)
                {
                    state = ShaderState.CrimsonSlime;
                    return;
                }
                else if (activeSlime.ModNPC is DivineGargooptuar)
                {
                    if (FindProjectile(ModContent.ProjectileType<PixieBall>()) != null)
                    {
                        state = ShaderState.TOUCHME;
                    }
                    else
                    {
                        state = ShaderState.HallowedSlime;
                    }
                    return;
                }
                else if (activeSlime.ModNPC is StellarGeliath)
                {
                    state = ShaderState.AstralSlime;
                    return;
                }
            }
        }

        public override void Update(float elapsedTime)
        {
            if (GoozmaSystem.GoozmaBossIndex == -1 || Main.npc[GoozmaSystem.GoozmaBossIndex].ModNPC is not Goozma goozma)
                return;

            UpdateState(goozma);
            //Main.NewText(state);
            if (Math.Abs(goozma.Phase) == 2 || (state == ShaderState.PhaseTransition && goozma.Time > 250))
            {
                pulseSpeed = Math.Min(pulseSpeed + 0.00075f, 1f / 20f);
                pulseColorMultiplier = Math.Min(pulseColorMultiplier + 0.005f, 1f);
            }
            else
            {
                pulseSpeed = Math.Max(pulseSpeed - 0.01f, 1f / 50f);
                pulseColorMultiplier = Math.Max(pulseColorMultiplier - 0.01f, 0.4f);
            }
            if (state == ShaderState.Laser)
            {
                whiteLuminance = Math.Min(whiteLuminance + 0.003f, 0.4f);
            }
            else
            {
                whiteLuminance = Math.Max(whiteLuminance - 0.01f, 0f);
            }

            pulseTime += pulseSpeed;
        }

        public override void Process(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            UpdateColorMap();
            base.Process(device, fragment, quality, time);
        }

        private void DrawPulse(Vector2 center, RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            //NoiseHelper.GetStaticNoise()
            for (int i = 0; i < fragment.Count; i++)
            {
                Vector2 keyPosition = fragment.GetCanvasPositionOfIndex(i);
                Vector2 difference = keyPosition - center;
                float distance = difference.Length();
                float rotation = difference.ToRotation();

                float progress = Math.Abs(distance * 4f - pulseTime * 8f) % _colorMap.Length;
                int index = _colorMap.Length - 1 - (int)progress;
                int nextIndex = (index + 1) % _colorMap.Length;

                float lerpAmount = progress % 1f;
                if (index == _colorMap.Length - 2)
                {
                    lerpAmount = 1f;
                }

                var color = Vector3.Lerp(_colorMap[nextIndex], _colorMap[index], lerpAmount);
                color = Vector3.Lerp(color, new(1f, 1f, 1f), whiteLuminance);
                color *= pulseColorMultiplier;
                fragment.SetColor(i, new(color, 1f));
            }
        }

        private void DrawLaserAura(Vector2 center, RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            float auraBrightness = whiteLuminance / 0.4f;
            if (auraBrightness <= 0f)
            {
                return;
            }

            float auraIntensity = MathF.Sin(time * 2f) * 0.5f + 0.5f;
            for (int i = 0; i < fragment.Count; i++)
            {
                Vector2 keyPosition = fragment.GetCanvasPositionOfIndex(i);
                Vector2 difference = keyPosition - center;
                float distance = difference.Length() * (1f + 1.5f * auraIntensity);
                float rotation = difference.ToRotation();
                if (distance < 1f)
                {
                    fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], new Vector4(1f, 1f, 1f, 1f), (1f - MathF.Pow(distance, 6f)) * auraBrightness));
                }
            }
        }

        private void DrawSlimeAura(Vector2 center, RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            switch (state) {
                case ShaderState.CrimsonSlime:
                    {
                        for (int i = 0; i < fragment.Count; i++)
                        {
                            Point keyPosition = fragment.GetGridPositionOfIndex(i);
                            Vector2 canvasPosition = fragment.GetCanvasPositionOfIndex(i);
                            canvasPosition.Y = fragment.CanvasSize.Y - canvasPosition.Y;
                            Vector4 value = Vector4.Zero;
                            float noise = ((NoiseHelper.GetStaticNoise(keyPosition.X) * 10f + time) % 10f - (1f - canvasPosition.Y)) * 2f;
                            if (noise > 0f)
                            {
                                float amount = Math.Max(0f, 1.5f - noise);
                                if (noise < 0.5f)
                                {
                                    amount = noise * 2f;
                                }

                                value = Vector4.Lerp(fragment.Colors[i] * 0.2f, new Vector4(1f, 0f, 0f, 1f), amount);
                            }

                            float staticNoise = NoiseHelper.GetStaticNoise(canvasPosition * 0.3f + new Vector2(0f, time * 0.1f));
                            staticNoise = Math.Max(0f, 1f - staticNoise * (1f + (1f - canvasPosition.Y) * 4f));
                            staticNoise *= Math.Max(0f, (canvasPosition.Y - 0.1f) / 0.9f);
                            value = Vector4.Lerp(value, new Vector4(1f, 0f, 0f, 1f), staticNoise);
                            fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], value with { W = 1f }, value.W));
                        }
                    }
                    break;

                case ShaderState.CorruptionSlime:
                    {
                        //Terraria.GameContent.RGB.UndergroundMushroomShader
                        var corruptionColor = new Vector4(0.3f, 0.1f, 0.8f, 1f);
                        for (int i = 0; i < fragment.Count; i++)
                        {
                            Point keyPosition = fragment.GetGridPositionOfIndex(i);
                            Vector2 canvasPosition = fragment.GetCanvasPositionOfIndex(i);
                            Vector4 value = Vector4.Zero;
                            float noise = ((NoiseHelper.GetStaticNoise(keyPosition.X) * 10f + time) % 10f - (1f - canvasPosition.Y)) * 2f;
                            if (noise > 0f)
                            {
                                float amount = Math.Max(0f, 1.5f - noise);
                                if (noise < 0.5f)
                                {
                                    amount = noise * 2f;
                                }

                                value = Vector4.Lerp(fragment.Colors[i] * 0.2f, corruptionColor, amount);
                            }

                            float staticNoise = NoiseHelper.GetStaticNoise(canvasPosition * 0.3f + new Vector2(0f, time * 0.1f));
                            staticNoise = Math.Max(0f, 1f - staticNoise * (1f + (1f - canvasPosition.Y) * 4f));
                            staticNoise *= Math.Max(0f, (canvasPosition.Y - 0.1f) / 0.9f);
                            value = Vector4.Lerp(value, corruptionColor, staticNoise);
                            fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], value with { W = 1f }, value.W));
                        }
                    }
                    break;

                case ShaderState.TOUCHME:
                case ShaderState.HallowedSlime:
                    {
                        for (int i = 0; i < fragment.Count; i++)
                        {
                            Vector2 keyPosition = fragment.GetCanvasPositionOfIndex(i);
                            Vector2 difference = keyPosition - center;
                            float yDistance = Math.Abs(difference.Y * 1.5f);

                            if (yDistance < 1f)
                            {
                                fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], new Vector4(1f, 0.7f, 1f, 1f), Math.Min(1f - yDistance, 1f) * 0.6f));
                            }
                        }
                        if (state == ShaderState.TOUCHME)
                        {
                            Vector4 textColor = Color.Yellow.ToVector4();
                            for (int i = 0; i < fragment.Count; i++)
                            {
                                Vector2 keyPosition = fragment.GetCanvasPositionOfIndex(i);
                                if (keyPosition.Y > 1f)
                                {
                                    continue;
                                }

                                float textPosition = (keyPosition.X * 0.7f + time * 3.5f) % 10f;
                                if (textPosition > 5f)
                                {
                                    textPosition += 0.5f;
                                }
                                float textPositionRelative = textPosition % 1f;
                                if (textPositionRelative < 0.1f || textPositionRelative > 0.9f)
                                {
                                    continue;
                                }

                                if (textPosition < 1f)
                                {
                                    if (keyPosition.Y < 0.4f || (textPositionRelative > 0.33f && textPositionRelative < 0.66f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 2f)
                                {
                                    if ((keyPosition.Y < 0.4f || keyPosition.Y > 0.6f) || !(textPositionRelative > 0.33f && textPositionRelative < 0.66f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 3f)
                                {
                                    if ((keyPosition.Y > 0.6f) || !(textPositionRelative > 0.33f && textPositionRelative < 0.66f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 4f)
                                {
                                    if ((keyPosition.Y < 0.4f || keyPosition.Y > 0.6f) || textPositionRelative < 0.33f)
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 5f)
                                {
                                    if ((keyPosition.Y > 0.3f && keyPosition.Y < 0.7f) || !(textPositionRelative > 0.33f && textPositionRelative < 0.66f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 6f)
                                {
                                }
                                else if (textPosition < 7f)
                                {
                                    if ((keyPosition.Y < 0.4f || keyPosition.Y > 0.6f) || (textPositionRelative > 0.33f && textPositionRelative < 0.66f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 8f)
                                {
                                    if (keyPosition.Y < 0.4f || (textPositionRelative > 0.33f && textPositionRelative < 0.66f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                                else if (textPosition < 9f)
                                {
                                    if (textPositionRelative > 0.33f && textPositionRelative < 0.66f && (keyPosition.Y < 0.6f || keyPosition.Y > 0.75f))
                                    {
                                        fragment.SetColor(i, textColor);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case ShaderState.AstralSlime:
                    {
                        for (int i = 0; i < fragment.Count; i++)
                        {
                            Vector2 keyPosition = fragment.GetCanvasPositionOfIndex(i);
                            Vector2 difference = keyPosition - center;
                            float yDistance = difference.Y * 0.75f;

                            if (yDistance > 0f)
                            {
                                fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], new Vector4(0f, 0f, 1f, 1f), Math.Min(yDistance, 1f)));
                            }
                            if (yDistance < 0f)
                            {
                                fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], new Vector4(1f, 0.5f, 0f, 1f), Math.Min(-yDistance, 1f)));
                            }
                        }
                    }
                    break;
            }

            var projectile = FindProjectile(ModContent.ProjectileType<HolyExplosion>());
            float holyExplosionTime = 60f;
            if (projectile != null && projectile.ai[0] < holyExplosionTime)
            {
                float intensity = 0f;
                float introTime = 7f;
                if (projectile.ai[0] < 7f)
                {
                    intensity = MathF.Sin(projectile.ai[0] / introTime * MathHelper.PiOver2);
                }
                else
                {
                    intensity = 1f - MathF.Pow((projectile.ai[0] - introTime) / (holyExplosionTime - introTime), 5f);
                }
                for (int i = 0; i < fragment.Count; i++)
                {
                    fragment.SetColor(i, Vector4.Lerp(fragment.Colors[i], Color.White.ToVector4(), intensity));
                }
            }
        }

        [RgbProcessor(new EffectDetailLevel[] { EffectDetailLevel.High })]
        private void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            Vector2 center = fragment.CanvasCenter;
            center += time.ToRotationVector2() * MathF.Sin(time * (float)Math.E) * pulseSpeed;
            // Screen Shake effect, deemed too 'buggy looking'
            //Vector2 cameraShakeVector = Main.screenPosition;
            //Main.instance.CameraModifiers.ApplyTo(ref cameraShakeVector);
            //center += (Main.screenPosition - cameraShakeVector) * 0.05f;

            DrawPulse(center, device, fragment, quality, time);
            DrawLaserAura(center, device, fragment, quality, time);
            DrawSlimeAura(center, device, fragment, quality, time);
            FixColors(fragment);
        }

        [RgbProcessor(new EffectDetailLevel[] { EffectDetailLevel.Low })]
        private void ProcessLowDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            Vector2 center = new Vector2(1.7f, 0.5f);
            DrawPulse(center, device, fragment, quality, time);
            FixColors(fragment);
        }

        #region Helpers
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

        private Projectile FindProjectile(int type)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == type)
                {
                    return Main.projectile[i];
                }
            }
            return null;
        }
        #endregion
    }
}
