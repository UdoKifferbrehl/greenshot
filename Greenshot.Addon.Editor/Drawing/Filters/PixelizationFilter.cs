﻿/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2016 Thomas Braun, Jens Klingen, Robin Krom
 * 
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on GitHub: https://github.com/greenshot
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using Greenshot.Addon.Core;
using Greenshot.Addon.Editor.Helpers;
using Greenshot.Addon.Interfaces.Drawing;

namespace Greenshot.Addon.Editor.Drawing.Filters
{
	[Serializable]
	public class PixelizationFilter : AbstractFilter
	{
		protected int _pixelSize = 5;

		[Field(FieldTypes.PIXEL_SIZE)]
		public int PixelSize
		{
			get
			{
				return _pixelSize;
			}
			set
			{
				_pixelSize = value;
				OnFieldPropertyChanged(FieldTypes.PIXEL_SIZE);
			}
		}

		public PixelizationFilter(DrawableContainer parent) : base(parent)
		{
		}

		public override void Apply(Graphics graphics, Bitmap applyBitmap, Rectangle rect, RenderMode renderMode)
		{
			Rectangle applyRect = ImageHelper.CreateIntersectRectangle(applyBitmap.Size, rect, Invert);
			var pixelSize = _pixelSize;
			if (pixelSize <= 1 || rect.Width == 0 || rect.Height == 0)
			{
				// Nothing to do
				return;
			}
			if (rect.Width < pixelSize)
			{
				pixelSize = rect.Width;
			}
			if (rect.Height < pixelSize)
			{
				pixelSize = rect.Height;
			}
			using (IFastBitmap dest = FastBitmap.CreateCloneOf(applyBitmap, rect))
			{
				using (IFastBitmap src = FastBitmap.Create(applyBitmap, rect))
				{
					List<Color> colors = new List<Color>();
					int halbPixelSize = pixelSize/2;
					for (int y = src.Top - halbPixelSize; y < src.Bottom + halbPixelSize; y = y + pixelSize)
					{
						for (int x = src.Left - halbPixelSize; x <= src.Right + halbPixelSize; x = x + pixelSize)
						{
							colors.Clear();
							for (int yy = y; yy < y + pixelSize; yy++)
							{
								if (yy >= src.Top && yy < src.Bottom)
								{
									for (int xx = x; xx < x + pixelSize; xx++)
									{
										if (xx >= src.Left && xx < src.Right)
										{
											colors.Add(src.GetColorAt(xx, yy));
										}
									}
								}
							}
							Color currentAvgColor = ColorHelper.Mix(colors);
							for (int yy = y; yy <= y + pixelSize; yy++)
							{
								if (yy >= src.Top && yy < src.Bottom)
								{
									for (int xx = x; xx <= x + pixelSize; xx++)
									{
										if (xx >= src.Left && xx < src.Right)
										{
											dest.SetColorAt(xx, yy, currentAvgColor);
										}
									}
								}
							}
						}
					}
				}
				dest.DrawTo(graphics, rect.Location);
			}
		}
	}
}