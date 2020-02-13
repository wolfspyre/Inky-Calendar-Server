﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace InkyCal.Utils.Tests
{
	public abstract class IPanelTests<T> where T : IPanel
	{

		protected abstract T GetPanel();

		[Fact()]
		public void GetImageTest()
		{
			//arrange
			var panel = GetPanel();
			var filename = $"GetImageTest_{typeof(T).Name}.png";
			DisplayModel.epd_7_in_5_v2_colour.GetSpecs(out var width, out var height, out var colors);

			//act
			var image = panel.GetImage(
								width: height, 
								height: width, 
								colors: colors);

			//assert
			Assert.NotNull(image);

			using var fileStream = File.Create(filename);
			image.Save(fileStream, new PngEncoder());

			var fi = new FileInfo(filename);
			Assert.True(fi.Exists, $"File {fi.FullName} does not exist");

			Trace.WriteLine(fi.FullName);

		}
	}
}
