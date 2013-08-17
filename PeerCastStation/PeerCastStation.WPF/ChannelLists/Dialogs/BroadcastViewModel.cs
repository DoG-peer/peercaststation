﻿// PeerCastStation, a P2P streaming servent.
// Copyright (C) 2013 PROGRE (djyayutto@gmail.com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Windows.Input;
using PeerCastStation.Core;
using PeerCastStation.WPF.ChannelLists.ChannelInfos;
using PeerCastStation.WPF.Commons;
using PeerCastStation.WPF.CoreSettings;

namespace PeerCastStation.WPF.ChannelLists.Dialogs
{
  class BroadcastViewModel : ViewModelBase
  {
    private readonly ContentReaderItem[] contentTypes;
    public ContentReaderItem[] ContentTypes { get { return contentTypes; } }

    private readonly YellowPageItem[] yellowPages;
    public YellowPageItem[] YellowPages { get { return yellowPages; } }

    private string streamUrl = "";
    public string StreamUrl
    {
      get { return streamUrl; }
      set
      {
        SetProperty("StreamUrl", ref streamUrl, value,
          () => start.OnCanExecuteChanged());
      }
    }

    private int? bitrate;
    public int? Bitrate
    {
      get { return bitrate; }
      set { SetProperty("Bitrate", ref bitrate, value); }
    }

    private ContentReaderItem contentType;
    public ContentReaderItem ContentType
    {
      get { return contentType; }
      set
      {
        SetProperty("ContentType", ref contentType, value,
          () => start.OnCanExecuteChanged());
      }
    }

    private IYellowPageClient yellowPage;
    public IYellowPageClient YellowPage
    {
      get { return yellowPage; }
      set { SetProperty("YellowPage", ref yellowPage, value); }
    }

    private string channelName = "";
    public string ChannelName
    {
      get { return channelName; }
      set
      {
        SetProperty("ChannelName", ref channelName, value,
          () => start.OnCanExecuteChanged());
      }
    }

    private string genre = "";
    public string Genre
    {
      get { return genre; }
      set { SetProperty("Genre", ref genre, value); }
    }

    private string description = "";
    public string Description
    {
      get { return description; }
      set { SetProperty("Description", ref description, value); }
    }

    private string comment = "";
    public string Comment
    {
      get { return comment; }
      set { SetProperty("Comment", ref comment, value); }
    }

    private string contactUrl = "";
    public string ContactUrl
    {
      get { return contactUrl; }
      set { SetProperty("ContactUrl", ref contactUrl, value); }
    }

    private string trackTitle = "";
    public string TrackTitle
    {
      get { return trackTitle; }
      set { SetProperty("TrackTitle", ref trackTitle, value); }
    }

    private string trackAlbum = "";
    public string TrackAlbum
    {
      get { return trackAlbum; }
      set { SetProperty("TrackAlbum", ref trackAlbum, value); }
    }

    private string trackArtist = "";
    public string TrackArtist
    {
      get { return trackArtist; }
      set { SetProperty("TrackArtist", ref trackArtist, value); }
    }

    private string trackGenre = "";
    public string TrackGenre
    {
      get { return trackGenre; }
      set { SetProperty("TrackGenre", ref trackGenre, value); }
    }

    private string trackUrl = "";
    public string TrackUrl
    {
      get { return trackUrl; }
      set { SetProperty("TrackUrl", ref trackUrl, value); }
    }

    private readonly Command start;
    public ICommand Start { get { return start; } }

    private IContentReaderFactory ContentReaderFactory
    {
      get { return contentType == null ? null : contentType.ContentReaderFactory; }
    }

    private Uri StreamSource
    {
      get
      {
        Uri source;
        if (!Uri.TryCreate(streamUrl, UriKind.Absolute, out source))
        {
          return null;
        }
        return source;
      }
    }

    public BroadcastViewModel(PeerCast peerCast)
    {
      contentTypes = peerCast.ContentReaderFactories
        .Select(reader => new ContentReaderItem(reader)).ToArray();

      yellowPages = new YellowPageItem[] { new YellowPageItem("掲載なし", null) }
        .Concat(peerCast.YellowPages.Select(yp => new YellowPageItem(yp))).ToArray();
      if (contentTypes.Length > 0) contentType = contentTypes[0];

      start = new Command(() =>
        {
          var source = StreamSource;
          var contentReaderFactory = ContentReaderFactory;
          if (!CanBroadcast(source, contentReaderFactory, channelName))
          {
            return;
          }
          IYellowPageClient yellowPage = this.yellowPage;
          var channelInfo = CreateChannelInfo(this);
          var channelTrack = CreateChannelTrack(this);

          var channel_id = Utils.CreateChannelID(
            peerCast.BroadcastID,
            channelName,
            genre,
            source.ToString());
          var channel = peerCast.BroadcastChannel(
            yellowPage,
            channel_id,
            channelInfo,
            source,
            contentReaderFactory);
          if (channel != null)
          {
            channel.ChannelTrack = channelTrack;
          }
        },
        () => CanBroadcast(StreamSource, ContentReaderFactory, channelName));
    }

    private bool CanBroadcast(Uri streamSource, IContentReaderFactory contentReaderFactory, string channelName)
    {
      return streamSource != null
        && contentReaderFactory != null
        && !String.IsNullOrEmpty(channelName);
    }

    private ChannelInfo CreateChannelInfo(BroadcastViewModel viewModel)
    {
      var info = new AtomCollection();
      if (viewModel.bitrate.HasValue) info.SetChanInfoBitrate(viewModel.bitrate.Value);
      info.SetChanInfoName(viewModel.channelName);
      info.SetChanInfoGenre(viewModel.genre);
      info.SetChanInfoDesc(viewModel.description);
      info.SetChanInfoComment(viewModel.comment);
      info.SetChanInfoURL(viewModel.contactUrl);
      return new ChannelInfo(info);
    }

    private ChannelTrack CreateChannelTrack(BroadcastViewModel viewModel)
    {
      var collection = new AtomCollection();
      collection.SetChanTrackTitle(viewModel.TrackTitle);
      collection.SetChanTrackGenre(viewModel.TrackGenre);
      collection.SetChanTrackAlbum(viewModel.TrackAlbum);
      collection.SetChanTrackCreator(viewModel.TrackArtist);
      collection.SetChanTrackURL(viewModel.TrackUrl);
      return new ChannelTrack(collection);
    }
  }
}
