﻿/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2016 Thomas Braun, Jens Klingen, Robin Krom
 * 
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on Sourceforge: http://sourceforge.net/projects/greenshot/
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Greenshot.Addon.Core;
using Greenshot.Addon.Extensions;
using Greenshot.Addon.Interfaces;
using Greenshot.Addon.Interfaces.Destination;
using Greenshot.Addon.Interfaces.Plugin;
using Greenshot.Addon.Windows;

namespace Greenshot.Addon.Photobucket
{
	[Destination(PhotobucketDesignation)]
	public sealed class PhotobucketDestination : AbstractDestination
	{
		private const string PhotobucketDesignation = "Photobucket";
		private static readonly Serilog.ILogger LOG = Serilog.Log.Logger.ForContext(typeof(PhotobucketDestination));
		private static readonly BitmapSource PhotobucketIcon;

		static PhotobucketDestination()
		{
			var resources = new ComponentResourceManager(typeof(PhotobucketPlugin));
			using (var photobucketImage = (Bitmap) resources.GetObject("Photobucket"))
			{
				PhotobucketIcon = photobucketImage.ToBitmapSource();
			}

		}

		[Import]
		private IPhotobucketConfiguration PhotobucketConfiguration
		{
			get;
			set;
		}

		[Import]
		private IPhotobucketLanguage PhotobucketLanguage
		{
			get;
			set;
		}

		/// <summary>
		/// Setup
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
			Designation = PhotobucketDesignation;
			Export = async (exportContext, capture, token) => await ExportCaptureAsync(capture, "0", token);
			Text = PhotobucketLanguage.UploadMenuItem;
			Icon = PhotobucketIcon;
		}

		private async Task<INotification> ExportCaptureAsync(ICapture capture, string album, CancellationToken token = default(CancellationToken))
		{
			var returnValue = new Notification
			{
				NotificationType = NotificationTypes.Success,
				Source = PhotobucketDesignation,
				SourceType = SourceTypes.Destination,
				Text = string.Format(PhotobucketLanguage.UploadSuccess, PhotobucketDesignation)
			};
			var outputSettings = new SurfaceOutputSettings(PhotobucketConfiguration.UploadFormat, PhotobucketConfiguration.UploadJpegQuality, false);
			try
			{
				var photobucketInfo = await PleaseWaitWindow.CreateAndShowAsync(Designation, PhotobucketLanguage.CommunicationWait, async (progress, pleaseWaitToken) =>
				{
					string filename = Path.GetFileName(FilenameHelper.GetFilename(PhotobucketConfiguration.UploadFormat, capture.CaptureDetails));
					return await PhotobucketUtils.UploadToPhotobucket(capture, outputSettings, album, capture.CaptureDetails.Title, filename, progress);
				}, token);

				// This causes an exeption if the upload failed :)
				LOG.Debug("Uploaded to Photobucket page: " + photobucketInfo.Page);
				string uploadUrl = null;
				try
				{
					uploadUrl = PhotobucketConfiguration.UsePageLink ? photobucketInfo.Page : photobucketInfo.Original;
				}
				catch (Exception ex)
				{
					LOG.Error("Can't write to clipboard: ", ex);
				}

				if (uploadUrl != null)
				{
					returnValue.ImageLocation = new Uri(uploadUrl);
					if (PhotobucketConfiguration.AfterUploadLinkToClipBoard)
					{
						ClipboardHelper.SetClipboardData(uploadUrl);
					}
				}
			}
			catch (TaskCanceledException tcEx)
			{
				returnValue.Text = string.Format(PhotobucketLanguage.UploadFailure, PhotobucketDesignation);
                returnValue.NotificationType = NotificationTypes.Cancel;
				returnValue.ErrorText = tcEx.Message;
				LOG.Information(tcEx.Message);
			}
			catch (Exception e)
			{
				returnValue.Text = string.Format(PhotobucketLanguage.UploadFailure, PhotobucketDesignation);
				returnValue.NotificationType = NotificationTypes.Fail;
				returnValue.ErrorText = e.Message;
				LOG.Warning(e, "Photobucket export failed");
				MessageBox.Show(PhotobucketLanguage.UploadFailure + " " + e.Message, PhotobucketDesignation, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return returnValue;
        }
	}
}