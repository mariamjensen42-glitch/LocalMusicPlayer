using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LocalMusicPlayer.Services.OnlineLyrics.Helpers;

namespace LocalMusicPlayer.Services.OnlineLyrics.QQMusic;

public class Api : BaseApi
{
    protected override string HttpRefer => "https://c.y.qq.com/";

    protected override Dictionary<string, string>? AdditionalHeaders => null;

    private static readonly DateTime _dtFrom = new(1970, 1, 1, 8, 0, 0, 0, DateTimeKind.Local);

    public enum SearchTypeEnum
    {
        SONG_ID = 0,
        ALBUM_ID = 1,
        PLAYLIST_ID = 2,
    }

    public async Task<MusicFcgApiResult?> Search(string keyword, SearchTypeEnum searchType)
    {
        var type = searchType switch
        {
            SearchTypeEnum.SONG_ID => 0,
            SearchTypeEnum.ALBUM_ID => 2,
            SearchTypeEnum.PLAYLIST_ID => 3,
            _ => 0,
        };
        var data = new Dictionary<string, object>
        {
            {
                "req_1", new Dictionary<string, object>
                {
                    { "method", "DoSearchForQQMusicDesktop" },
                    { "module", "music.search.SearchCgiService" },
                    {
                        "param", new Dictionary<string, object>
                        {
                            { "num_per_page", "20" },
                            { "page_num", "1" },
                            { "query", keyword },
                            { "search_type", type },
                        }
                    }
                }
            }
        };

        var resp = await PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", data);

        return resp.ToEntity<MusicFcgApiResult>();
    }

    public async Task<MusicFcgApiAlternativeResult?> SearchAlternative(string keyword)
    {
        string data = "{\"music.search.SearchCgiService\": {\"method\": \"DoSearchForQQMusicDesktop\",\"module\": \"music.search.SearchCgiService\",\"param\": {\"num_per_page\": 10,\"page_num\": 1,\"query\": \"" + keyword + "\",\"search_type\": 0}}}";

        var resp = await PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", data);

        return resp.ToEntity<MusicFcgApiAlternativeResult>();
    }

    public async Task<SongResult?> GetSong(string id)
    {
        const string callBack = "getOneSongInfoCallback";

        var data = new Dictionary<string, string>
        {
            { id.IsNumber() ? "songid" : "songmid", id },
            { "tpl", "yqq_song_detail" },
            { "format", "jsonp" },
            { "callback", callBack },
            { "g_tk", "5381" },
            { "jsonpCallback", callBack },
            { "loginUin", "0" },
            { "hostUin", "0" },
            { "outCharset", "utf8" },
            { "notice", "0" },
            { "platform", "yqq" },
            { "needNewCode", "0" },
        };

        var resp = await PostAsync("https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg", data);

        return ResolveRespJson(callBack, resp).ToEntity<SongResult>();
    }

    public async Task<LyricResult?> GetLyric(string songMid)
    {
        var currentMillis = (DateTime.Now.ToLocalTime().Ticks - _dtFrom.Ticks) / 10000;

        const string callBack = "MusicJsonCallback_lrc";

        var data = new Dictionary<string, string>
        {
            { "callback", "MusicJsonCallback_lrc" },
            { "pcachetime", currentMillis.ToString() },
            { "songmid", songMid },
            { "g_tk", "5381" },
            { "jsonpCallback", callBack },
            { "loginUin", "0" },
            { "hostUin", "0" },
            { "format", "jsonp" },
            { "inCharset", "utf8" },
            { "outCharset", "utf8" },
            { "notice", "0" },
            { "platform", "yqq" },
            { "needNewCode", "0" },
        };

        var resp = await PostAsync("https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg", data);

        var result = ResolveRespJson(callBack, resp).ToEntity<LyricResult>();

        return result?.Decode();
    }

    private static string ResolveRespJson(string callBackSign, string val)
    {
        if (!val.StartsWith(callBackSign))
        {
            return string.Empty;
        }

        var jsonStr = val.Replace(callBackSign + "(", string.Empty);
        return jsonStr.Remove(jsonStr.Length - 1);
    }

    protected virtual string GetGuid()
    {
        var guid = new StringBuilder(10);
        var r = new Random();
        for (var i = 0; i < 10; i++)
        {
            guid.Append(Convert.ToString(r.Next(10)));
        }

        return guid.ToString();
    }
}
