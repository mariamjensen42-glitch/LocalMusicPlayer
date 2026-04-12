using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace LocalMusicPlayer.Services.OnlineLyrics.QQMusic;

#nullable disable

public class MusicFcgApiResult
{
    public long Code { get; set; }
    public long Ts { get; set; }
    public long StartTs { get; set; }
    public string Traceid { get; set; }
    public MusicFcgReq Req { get; set; }
    public MusicFcgReq0 Req_0 { get; set; }
    public MusicFcgReq1 Req_1 { get; set; }

    public class MusicFcgReq
    {
        public long Code { get; set; }
        public MusicFcgReqData Data { get; set; }

        public class MusicFcgReqData
        {
            public string[] Sip { get; set; }
            public string Keepalivefile { get; set; }
            public string Testfile2g { get; set; }
            public string Testfilewifi { get; set; }
        }
    }

    public class MusicFcgReq0
    {
        public long Code { get; set; }
        public MusicFcgReq0Data Data { get; set; }

        public class MusicFcgReq0Data
        {
            public string[] Sip { get; set; }
            public string Testfile2g { get; set; }
            public string Testfilewifi { get; set; }
            public MusicFcgReq0DataMidurlinfo[] Midurlinfo { get; set; }
        }

        public class MusicFcgReq0DataMidurlinfo
        {
            public string Songmid { get; set; }
            public string Purl { get; set; }
        }
    }

    public class MusicFcgReq1
    {
        public long Code { get; set; }
        public MusicFcgReq1Data Data { get; set; }

        public class MusicFcgReq1Data
        {
            public long Code { get; set; }
            public long Ver { get; set; }
            public MusicFcgReq1DataBody Body { get; set; }
            public MusicFcgReq1DataMeta Meta { get; set; }

            public class MusicFcgReq1DataBody
            {
                public MusicFcgReq1DataBodyAlbum Album { get; set; }
                public MusicFcgReq1DataBodySong Song { get; set; }
                public MusicFcgReq1DataBodyPlayList Songlist { get; set; }

                public class MusicFcgReq1DataBodyAlbum
                {
                    public MusicFcgReq1DataBodyAlbumInfo[] List { get; set; }

                    public class MusicFcgReq1DataBodyAlbumInfo
                    {
                        public long AlbumID { get; set; }
                        public string AlbumMID { get; set; }
                        public string AlbumName { get; set; }
                        public long Song_count { get; set; }
                        public string PublicTime { get; set; }
                        public Singer[] Singer_list { get; set; }
                    }
                }

                public class MusicFcgReq1DataBodySong
                {
                    public Song[] List { get; set; }
                }

                public class MusicFcgReq1DataBodyPlayList
                {
                    public MusicFcgReq1DataBodyPlayListInfo[] List { get; set; }

                    public class MusicFcgReq1DataBodyPlayListInfo
                    {
                        public string Dissid { get; set; }
                        public string Dissname { get; set; }
                        public string Imgurl { get; set; }
                        public string Introduction { get; set; }
                        public long Song_Count { get; set; }
                        public long Listennum { get; set; }
                        public MusicFcgReq1DataBodyPlayListCreator Creator { get; set; }
                    }

                    public class MusicFcgReq1DataBodyPlayListCreator
                    {
                        public string Name { get; set; }
                        public long Qq { get; set; }
                    }
                }
            }

            public class MusicFcgReq1DataMeta
            {
                public long Curpage { get; set; }
                public long Nextpage { get; set; }
                public long Perpage { get; set; }
                public string Query { get; set; }
                public long Sum { get; set; }
            }
        }
    }
}

public class MusicFcgApiAlternativeResult
{
    public long Code { get; set; }
    public long Ts { get; set; }
    public long StartTs { get; set; }
    public string Traceid { get; set; }

    [JsonProperty("music.search.SearchCgiService")]
    public SearchCgiService Search { get; set; }

    public class SearchCgiService
    {
        public DataBody Data { get; set; }
    }

    public class DataBody
    {
        public SearchData Body { get; set; }
    }

    public class SearchData
    {
        public Song Song { get; set; }
    }

    public class Song
    {
        public QQMusicSong[] List { get; set; }
    }
}

public class AlbumResult
{
    public long Code { get; set; }
    public AlbumInfo Data { get; set; }
    public string Message { get; set; }
}

public class PlaylistResult
{
    public long Code { get; set; }
    public Playlist[] Cdlist { get; set; }
}

public class SongResult
{
    public long Code { get; set; }
    public Song[] Data { get; set; }

    public bool IsIllegal()
    {
        return Code != 0 || Data.Length == 0;
    }
}

public class LyricResult
{
    public long Code { get; set; }
    public string Lyric { get; set; }
    public string Trans { get; set; }

    public LyricResult Decode()
    {
        if (Lyric != null)
        {
            Lyric = Encoding.UTF8.GetString(Convert.FromBase64String(Lyric));
        }

        if (Trans != null)
        {
            Trans = Encoding.UTF8.GetString(Convert.FromBase64String(Trans));
        }

        return this;
    }
}

public class Playlist
{
    public string Disstid { get; set; }
    public string Dissname { get; set; }
    public string Nickname { get; set; }
    public string Logo { get; set; }
    public string Desc { get; set; }
    public Tag[] Tags { get; set; }
    public long Songnum { get; set; }
    public string Songids { get; set; }
    public Song[] SongList { get; set; }
    public long Visitnum { get; set; }
    public long CTime { get; set; }
}

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long Pid { get; set; }
}

public class Song
{
    public Album Album { get; set; }
    public string Id { get; set; }
    public int Interval { get; set; }
    public string Mid { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public Singer[] Singer { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Time_public { get; set; }
    [JsonProperty("grp")]
    public List<Song> Group { get; set; }
    public int Language { get; set; }
    public int Genre { get; set; }
}

public class QQMusicSong
{
    public Album Album { get; set; }
    public string Id { get; set; }
    public int Interval { get; set; }
    public string Mid { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public Singer[] Singer { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Time_public { get; set; }
    [JsonProperty("grp")]
    public List<QQMusicSong> Group { get; set; }
    public int Language { get; set; }
    public int Genre { get; set; }
}

public class AlbumInfo
{
    public string ADate { get; set; }
    public string Company { get; set; }
    public string Desc { get; set; }
    public long Id { get; set; }
    public string Mid { get; set; }
    public string Lan { get; set; }
    public AlbumSong[] List { get; set; }
    public string Name { get; set; }
    public long Singerid { get; set; }
    public string Singermid { get; set; }
    public string Singername { get; set; }
    public int Total { get; set; }

    public class AlbumSong
    {
        public Singer[] Singer { get; set; }
        public long Songid { get; set; }
        public string Songmid { get; set; }
        public string Songname { get; set; }
    }
}

public class SingerSongResult
{
    public int Code { get; set; }
    public long Ts { get; set; }
    public long Start_Ts { get; set; }
    public string Traceid { get; set; }
    public SingerData Singer { get; set; }

    public class SingerData
    {
        public int Code { get; set; }
        public SingerInfo Data { get; set; }
    }

    public class SingerInfo
    {
        public List<SongInfo> Songlist { get; set; }
        public string Singer_brief { get; set; }
        public List<object> Music_grp { get; set; }
        public int Total_album { get; set; }
        public int Total_mv { get; set; }
        public int Total_song { get; set; }
        public string Yinyueren { get; set; }
        public bool Show_singer_desc { get; set; }
    }
}

public class Singer
{
    public long Id { get; set; }
    public string Mid { get; set; }
    public string Name { get; set; }
    public string Pmid { get; set; }
    public string Title { get; set; }
    public int Type { get; set; }
}

public class Album
{
    public int Id { get; set; }
    public string Mid { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Time_public { get; set; }
    public string Pmid { get; set; }
}

public class SongInfo
{
    public int Id { get; set; }
    public int Type { get; set; }
    public string Mid { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public List<Singer> Singer { get; set; }
    public Album Album { get; set; }
    public Mv Mv { get; set; }
    public int Interval { get; set; }
    public int Isonly { get; set; }
    public int Language { get; set; }
    public int Genre { get; set; }
    public int Index_cd { get; set; }
    public int Index_album { get; set; }
    public string Time_public { get; set; }
    public int Status { get; set; }
    public int Fnote { get; set; }
    public FileInfo File { get; set; }
    public Pay Pay { get; set; }
    public Action Action { get; set; }
    public Ksong Ksong { get; set; }
    public Volume Volume { get; set; }
    public string Label { get; set; }
    public string Url { get; set; }
    public int Bpm { get; set; }
    public int Version { get; set; }
    public string Trace { get; set; }
    public int Data_type { get; set; }
    public int Modify_stamp { get; set; }
    public string Pingpong { get; set; }
    public string Ppurl { get; set; }
    public int Tid { get; set; }
    public int Ov { get; set; }
}

public class Mv
{
    public int Id { get; set; }
    public string Vid { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public int Vt { get; set; }
}

public class FileInfo
{
    public string Media_mid { get; set; }
    public int Size_24aac { get; set; }
    public int Size_48aac { get; set; }
    public int Size_96aac { get; set; }
    public int Size_192ogg { get; set; }
    public int Size_192aac { get; set; }
    public int Size_128mp3 { get; set; }
    public int Size_320mp3 { get; set; }
    public int Size_ape { get; set; }
    public int Size_flac { get; set; }
    public int Size_dts { get; set; }
    public int Size_try { get; set; }
    public int Try_begin { get; set; }
    public int Try_end { get; set; }
    public string Url { get; set; }
    public int Size_hires { get; set; }
    public int Hires_sample { get; set; }
    public int Hires_bitdepth { get; set; }
    public int B_30s { get; set; }
    public int E_30s { get; set; }
    public int Size_96ogg { get; set; }
}

public class Pay
{
    public int Pay_month { get; set; }
    public int Price_track { get; set; }
    public int Price_album { get; set; }
    public int Pay_play { get; set; }
    public int Pay_down { get; set; }
    public int Pay_status { get; set; }
    public int Time_free { get; set; }
}

public class Action
{
    public int Switch { get; set; }
    public int Msgid { get; set; }
    public int Alert { get; set; }
    public int Icons { get; set; }
    public int Msgshare { get; set; }
    public int Msgfav { get; set; }
    public int Msgdown { get; set; }
    public int Msgpay { get; set; }
}

public class Ksong
{
    public int Id { get; set; }
    public string Mid { get; set; }
}

public class Volume
{
    public float Gain { get; set; }
    public float Peak { get; set; }
    public float Lra { get; set; }
}
