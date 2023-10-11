using System.Linq;
using CalamityHunt.Common.Graphics.SlimeMonsoon;
using CalamityHunt.Content.Items.Misc.AuricSouls;
using Terraria;
using Terraria.ModLoader;

namespace CalamityHunt.Common.Systems
{
    public class GoozmaAuricSoulScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/GoozmaAuricSoulMusic");

        public override bool IsSceneEffectActive(Player player)
        {
            return GoozmaSystem.GoozmaAuricSoulItemIndex != -1;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive)
            {
                player.GetModPlayer<EffectTilePlayer>().effectorCount["SlimeMonsoon"] = 5;
                SlimeMonsoonBackground.lightningEnabled = false;
                Main.windSpeedTarget = 1;
            }
        }
    }

    public class YharonAuricSoulScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/YharonAuricSoulMusic");

        public override bool IsSceneEffectActive(Player player)
        {
            return Main.item.Any(n => n.active && n.type == ModContent.ItemType<FieryAuricSoul>());
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            //if (isActive && ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            //{
            //}
        }
    }
}
