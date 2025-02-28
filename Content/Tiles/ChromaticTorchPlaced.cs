﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

using CalamityHunt.Content.Items.Placeable;
using CalamityHunt.Content.Bosses.Goozma;
using CalamityHunt.Core;

namespace CalamityHunt.Content.Tiles
{
	public class ChromaticTorchPlaced : ModTile
	{
		private Asset<Texture2D> flameTexture;
		public Color rainbowGlow => new GradientColor(SlimeUtils.GoozOilColors, 0.2f, 0.2f).ValueAt(Main.GlobalTimeWrappedHourly * 100f);

		public override void SetStaticDefaults()
		{
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.Torch[Type] = true;

			//DustType = ModContent.DustType<Sparkle>();
			AdjTiles = new int[] { TileID.Torches };

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Torches, 0));
			/*  This is what is copied from the Torches tile
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124, 561, 574, 575, 576, 577, 578 };
			TileObjectData.addAlternate(1);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124, 561, 574, 575, 576, 577, 578 };
			TileObjectData.addAlternate(2);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.addAlternate(0);
			*/
			TileObjectData.addTile(Type);

			// Etc
			AddMapEntry(new Color(256, 256, 256), Language.GetText("ItemName.Torch"));

			// Assets
			if (!Main.dedServ)
			{
				flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
			}
		}

		//public override float GetTorchLuck(Player player)
		//{
			// GetTorchLuck is called when there is an ExampleTorch nearby the client player
			// In most use-cases you should return 1f for a good luck torch, or -1f for a bad luck torch.
			// You can also add a smaller amount (eg 0.5) for a smaller postive/negative luck impact.
			// Remember that the overall torch luck is decided by every torch around the player, so it may be wise to have a smaller amount of luck impact.
			// Multiple example torches on screen will have no additional effect.

			// Positive and negative luck are accumulated separately and then compared to some fixed limits in vanilla to determine overall torch luck.
			// Postive luck is capped at 1, any value higher won't make any difference and negative luck is capped at 2.
			// A negative luck of 2 will cancel out all torch luck bonuses.

			// The influence positive torch luck can have overall is 0.1 (if positive luck is any number less than 1) or 0.2 (if positive luck is greater than or equal to 1)

			//bool inExampleUndergroundBiome = player.InModBiome<ExampleUndergroundBiome>();
			//return inExampleUndergroundBiome ? 1f : -0.1f; // ExampleTorch gives maximum positive luck when in example biome, otherwise a small negative luck
		//}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(1, 3);

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];

			// If the torch is on
			if (tile.TileFrameX < 66)
			{
				// Make it emit the following light.
				r = rainbowGlow.R / 155;
				g = rainbowGlow.G / 155;
				b = rainbowGlow.B / 155;
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = 0;

			if (WorldGen.SolidTile(i, j - 1))
			{
				offsetY = 2;

				if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
				{
					offsetY = 4;
				}
			}
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			// The following code draws multiple flames on top our placed torch.

			int offsetY = 0;

			if (WorldGen.SolidTile(i, j - 1))
			{
				offsetY = 2;

				if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
				{
					offsetY = 4;
				}
			}

			Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

			if (Main.drawToScreen)
			{
				zero = Vector2.Zero;
			}

			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i); // Don't remove any casts.
			Color color = rainbowGlow;
			int width = 20;
			int height = 20;
			var tile = Main.tile[i, j];
			int frameX = tile.TileFrameX;
			int frameY = tile.TileFrameY;

			for (int k = 0; k < 7; k++)
			{
				float xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
				float yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

				spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
			}
		}
	}
}
