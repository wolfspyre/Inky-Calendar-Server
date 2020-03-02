﻿using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InkyCal.Utils
{
	/// <summary>
	/// An image panel, assumes a landscape image, resizes and flips it to portait.
	/// </summary>
	public class PanelOfPanelRenderer : IPanelRenderer
	{
		private PanelOfPanels pp;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pp"></param>
		public PanelOfPanelRenderer(PanelOfPanels pp)
		{
			this.pp = pp;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <returns></returns>
		public async Task<Image> GetImage(int width, int height, Color[] colors)
		{
			Color primaryColor = colors.FirstOrDefault();
			Color supportColor = (colors.Count() > 2) ? colors[2] : primaryColor;
			Color errorColor = supportColor;
			Color backgroundColor = colors.Skip(1).First();

			var result = new Image<Rgba32>(new Configuration() { }, width, height, Color.Transparent);

			if (!(pp.Panels?.Any()).GetValueOrDefault())
			{
				result.Mutate(x =>
				{
					x.DrawText("No sub-panels configured", new Font(FontHelper.NotoSans, 16), errorColor, new Point(0, 0));
				});
			}
			else
			{
				var y = 0;
				var subPanelHeight = height / pp.Panels.Count();
				foreach (var panel in pp.Panels.OrderBy(x => x.SortIndex))
				{
					var renderer = panel.Panel.GetRenderer();

					try
					{
						var subImage = await renderer.GetImage(width, subPanelHeight, colors);

						result.Mutate(
							x => x.DrawImage(subImage, new Point(0, y),1f));

						//result.Mutate(x =>
						//{
						//	x.DrawText(
						//		new TextGraphicsOptions(true) { 
						//			WrapTextWidth = width }, 
						//			panel.Panel.Name ?? string.Empty,
						//			new Font(FontHelper.NotoSans, 16),
						//			primaryColor, 
						//			new Point(0, y));
						//});
					}
					catch (Exception ex) {
						result.Mutate(x =>
						{
							x.DrawText(new TextGraphicsOptions(true) { WrapTextWidth = width }, ex.Message, new Font(FontHelper.NotoSans, 16), errorColor, new Point(0, y));
						});
					}

					y += subPanelHeight;
				}
			}

			return result;
		}
	}
}