using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LocalMusicPlayer.Converters;

public class EnumBooleanConverter : EnumToBoolConverter
{
    public new static readonly EnumBooleanConverter Instance = new();
}
