using System.Collections.Generic;
using CalamityHunt.Common.Systems;
using CalamityHunt.Content.Items.Misc.AuricSouls;
using ReLogic.Peripherals.RGB;
using Terraria;
using Terraria.GameContent.RGB;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Chroma
{
    public class RGBSystem : ModSystem
    {
        private readonly List<ChromaShader> _registeredShaders = new();

        private void RegisterShader(ChromaShader shader, ChromaCondition condition, ShaderLayer layer)
        {
            _registeredShaders.Add(shader);
            Main.Chroma.RegisterShader(shader, condition, layer);
        }

        #region Loading
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Load_OverlayHooks();
            RegisterShader(new GoozmaShader(), new GoozmaShader.Condition(), ShaderLayer.Boss);
            RegisterShader(new GoozmaAuricSoulShader(), new GoozmaAuricSoulShader.Condition(), ShaderLayer.Boss);
        }

        public override void OnModUnload()
        {
            if (Main.dedServ)
                return;
            foreach (var shader in _registeredShaders)
            {
                Main.Chroma.UnregisterShader(shader);
            }
        }
        #endregion

        #region Updating 
        public override void PostUpdateNPCs()
        {
            if (Main.dedServ)
                return;

            DisableOverlays = GoozmaSystem.GoozmaBossIndex != -1 || GoozmaSystem.GoozmaAuricSoulItemIndex != -1;
        }
        #endregion

        #region Overlay Disabling
        public static bool DisableOverlays;

        private void Load_OverlayHooks()
        {
            On_SlimeRainShader.ProcessHighDetail += On_SlimeRainShader_ProcessHighDetail;
            On_RainShader.ProcessHighDetail += On_RainShader_ProcessHighDetail;
        }

        private static void On_RainShader_ProcessHighDetail(On_RainShader.orig_ProcessHighDetail orig, RainShader self, RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            if (DisableOverlays)
                return;

            orig(self, device, fragment, quality, time);
        }

        private static void On_SlimeRainShader_ProcessHighDetail(On_SlimeRainShader.orig_ProcessHighDetail orig, SlimeRainShader self, RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time)
        {
            if (DisableOverlays)
                return;

            orig(self, device, fragment, quality, time);
        }

        #endregion
    }
}
