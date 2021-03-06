﻿using System;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace InkyCal.Utils
{
	/// <summary>
	/// A renderer for weather
	/// </summary>
	public class WeatherPanelRenderer : IPanelRenderer
	{
		private readonly string token;
		private readonly string city;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="city"></param>
		public WeatherPanelRenderer(string token, string city)
		{
			this.token = token;
			this.city = city;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherPanelRenderer"/> class.
		/// </summary>
		/// <param name="panel">The panel.</param>
		public WeatherPanelRenderer(WeatherPanel panel)
		{
			this.token = panel.Token;
			this.city = panel.Location;
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
			//Forecast weather;
			//Station station;

			//using (var api = new OpenWeather.Noaa.Api())
			//{
			//	var stations = await api.GetStationsAsync();

			//	foreach (var s in stations.OrderBy(x => x.CountryCode).ThenBy(x => x.Name))
			//		Trace.WriteLine($"{s.Name}");

			//	var cityName = city.Split(',').FirstOrDefault();
			//	var countryCode = city.Split(',').Skip(1).FirstOrDefault();
			//	station = stations
			//				.OrderByDescending(x => x.CountryCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))
			//				.ThenByDescending(x => x.Name.StartsWith(cityName, StringComparison.InvariantCultureIgnoreCase))
			//				.ThenByDescending(x => x.Name.Equals(cityName, StringComparison.InvariantCultureIgnoreCase))
			//				.FirstOrDefault();

			//	var parameters = new WeatherParameters();
			//	parameters.SelectAll();


			//	weather = await api.GetForecastByStationAsync(station,
			//									  DateTime.UtcNow,
			//									  DateTime.UtcNow.AddDays(4),
			//									  OpenWeather.Noaa.Base.RequestType.Forcast,
			//									  OpenWeather.Noaa.Base.Units.Imperial,
			//									  parameters);
			//}

			Weather.RootObject weather;
			using (var util = new Weather.Util(token))
				weather = await util.GetForeCast(city);

			var station = weather?.city;

			colors.ExtractMeaningFullColors(
				out var primaryColor
				, out var supportColor
				, out var errorColor
				, out var backgroundColor
				);

			var result = PanelRenderHelper.CreateImage(width, height, backgroundColor);
			var textFont = new Font(FontHelper.MonteCarlo, 12);
			var weatherFont = new Font(FontHelper.WeatherIcons, 40);

			var textGraphicsOptions = new TextGraphicsOptions(false)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			};
			var rendererOptions = textGraphicsOptions.ToRendererOptions(textFont);
			var weatherRendererOptions = textGraphicsOptions.ToRendererOptions(weatherFont);

			result.Mutate(context =>
			{
				var y = 0;
				var locationInfo = $"{station?.name},{station?.country}";
				context.DrawText(textGraphicsOptions, locationInfo, textFont, primaryColor, new PointF(5, y));
				y += (int)Math.Ceiling(TextMeasurer.Measure(locationInfo, rendererOptions).Height);

				try
				{
					if (weather is null)
						context.DrawText(textGraphicsOptions, $"No weather found for {station.name}", textFont, primaryColor, new PointF(0, y));
					else
					{
						var firstIcon = weather.list.OrderBy(l => l.dt).FirstOrDefault().weather.FirstOrDefault().icon;
						var weatherIconDimensions = TextMeasurer.Measure(firstIcon, weatherRendererOptions);
						var widthPerIcon = (int)Math.Ceiling(weatherIconDimensions.Width);
						var heightPerIcon = (int)Math.Ceiling(weatherIconDimensions.Height);
						var iconPadding = 5;
						var icons = (int)Math.Floor((decimal)width / (widthPerIcon + 2 * iconPadding));

						var indentPerIcon = (int)Math.Ceiling(((double)width + iconPadding) / icons);

						var x = 0;
						foreach (var forecast in weather.list.OrderBy(l => l.dt).Take(icons))
						{

							if (FontHelper.WeatherIconsMap.TryGetValue(forecast.weather.FirstOrDefault()?.icon, out var icon))
								context.DrawText(
									textGraphicsOptions,
									icon,
									weatherFont,
									supportColor,
									new PointF(x + iconPadding, y));
							else
								context.DrawText(
									textGraphicsOptions,
									$"{forecast.weather.FirstOrDefault()?.icon} has no icon mapping",
									textFont,
									errorColor,
									new PointF(x, y));

							context.DrawText(
								textGraphicsOptions,
								$@"{forecast.Date.ToLocalTime():HH:mm}
{forecast.weather.FirstOrDefault()?.main}",
								textFont, primaryColor, new PointF(x + iconPadding, y + heightPerIcon + iconPadding));

							x += indentPerIcon;

						}

						//var serialized = JsonConvert.SerializeObject(weather, Formatting.Indented);
						//Console.Write(serialized);
					}

				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(ex.ToString());
					y = 100;
					context.RenderErrorMessage(
						ex.Message.ToString(),
						errorColor, backgroundColor, ref y, width, rendererOptions);
				}
			});


			return result;
		}
	}
}
