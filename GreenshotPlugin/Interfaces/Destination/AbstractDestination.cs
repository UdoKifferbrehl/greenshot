﻿/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2015 Thomas Braun, Jens Klingen, Robin Krom
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GreenshotPlugin.Interfaces.Destination
{
	/// <summary>
	/// A simple base implementation for the IDestination
	/// </summary>
	public abstract class AbstractDestination : IDestination
	{
		private string _text;
		private string _shortcut;
		private bool _isEnabled = true;
		private ImageSource _icon;

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Simple OnPropertyChanged implementation
		/// </summary>
		/// <param name="propertyName"></param>
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string Shortcut
		{
			get
			{
				return _shortcut;
			}
			set
			{
				if (value != _shortcut)
				{
					_shortcut = value;
					OnPropertyChanged();
				}
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value != _text)
				{
					_text = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsEnabled
		{
			get
			{
				return _isEnabled;
			}
			set
			{
				if (value != _isEnabled)
				{
					_isEnabled = value;
					OnPropertyChanged();
				}
			}
		}

		public ImageSource Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (!Equals(value, _icon))
				{
					_icon = value;
					OnPropertyChanged();
				}
			}
		}

		public Func<ICapture, Task<ExportInformation>> Export
		{
			get;
			set;
		}

		public ObservableCollection<IDestination> Children
		{
			get;
			set;
		} = new ObservableCollection<IDestination>();

	}
}