Type.registerNamespace("Telerik.Web.UI.MediaPlayers");
(function (b, a)
{
    b.MediaPlayers.MediaPlayerBase=function (c)
    {
        b.MediaPlayerObjects.extendWithEvents(this);
        this.options={};
        this.timers={};
        a.extend(this.options, c);
    }
        ;
    b.MediaPlayers.MediaPlayerBase.prototype={
        initialize: function () { },
        whenReady: function (c)
        {
            if (this.isReady())
            {
                c.call(this);
            }
        },
        _poll: function (f, c, e, d)
        {
            var g=this;
            if (g.timers[f]!=null)
            {
                clearTimeout(g.timers[f]);
            }
            this.timers[f]=setTimeout((function (i)
            {
                return function h()
                {
                    if (c.call(i))
                    {
                        g.timers[f]=setTimeout(h, e);
                    }
                }
                    ;
            }
            )(d), e);
            return g.timers[f];
        },
        keys: function (e)
        {
            var d=[], c;
            for (c in e)
            {
                if (Object.prototype.hasOwnProperty.call(e, c)&&c!="null")
                {
                    d.push(c);
                }
            }
            return d;
        },
        dispose: function ()
        {
            var d=this.keys(this.timers);
            var c=d.length;
            while (c--)
            {
                clearTimeout(this.timers[d[c]]);
            }
        }
    };
    a.registerControlProperties(b.MediaPlayers.MediaPlayerBase, {
        uiSeeking: false,
        playing: false
    });
    b.MediaPlayers.MediaPlayerBase.registerClass("Telerik.Web.UI.MediaPlayers.MediaPlayerBase");
}
)(Telerik.Web.UI, $telerik.$);
Type.registerNamespace("Telerik.Web.UI.MediaPlayers");
(function ()
{
    var a=$telerik.$;
    var b=Telerik.Web.UI;
    b.MediaPlayers.YouTubePlayer=function (f, e, d)
    {
        this.owner=f;
        this._ytPlayerElementID=this.owner.get_id()+"_ytPlayerElement";
        var c=[];
        c[0]=e;
        b.MediaPlayers.YouTubePlayer.initializeBase(this, c);
        this.eventsPanel=$get("pnlEventsYT");
        this.currentMediaFile=d;
        this.initialize();
        this.timeOutFlag;
        this.bytesTotal=null;
        this.bytesLoaded=null;
        this.loaded=false;
        this._lastVolume=-1;
    }
        ;
    b.MediaPlayers.YouTubePlayer.canPlay=function (e, c)
    {
        if (c.mimetype==="video/youtube")
        {
            return true;
        }
        var d=/^http(s)?\:\/\/(www\.)?(youtube\.com|youtu\.be)/i;
        return (c.path&&c.path.search(d)===0);
    }
        ;
    b.MediaPlayers.YouTubePlayer._playersNeedConfigure=[];
    b.MediaPlayers.YouTubePlayer._onYouTubeIframeAPIReady=function ()
    {
        for (var c=0; c<b.MediaPlayers.YouTubePlayer._playersNeedConfigure.length; c++)
        {
            b.MediaPlayers.YouTubePlayer._playersNeedConfigure[c]._configurePlayer();
        }
        b.MediaPlayers.YouTubePlayer._playersNeedConfigure.length=0;
        b.MediaPlayers.YouTubePlayer._playersNeedConfigure=[];
    }
        ;
    b.MediaPlayers.YouTubePlayer.prototype={
        initialize: function ()
        {
            var c=this;
            b.MediaPlayers.YouTubePlayer.callBaseMethod(c, "initialize");
            c._createMediaElement();
            c._initializePlayer();
            if ($telerik.isIE7)
            {
                a(c.owner.get_element()).on("mousedown", function (d)
                {
                    d.stopPropagation();
                });
                a(c.owner.get_element()).on("click", function (f)
                {
                    var d=f.target.childNodes;
                    if (d.length>0&&d[0].nodeName.toUpperCase()=="OBJECT")
                    {
                        f.stopPropagation();
                        try
                        {
                            if (c.isPlaying())
                            {
                                c.pause();
                            } else
                            {
                                c.play();
                            }
                        } catch (g) { }
                    }
                });
            }
        },
        dispose: function ()
        {
            if ($telerik.isIE7)
            {
                a(this.owner.get_element()).off("click");
                a(this.owner.get_element()).off("mousedown");
            }
            b.MediaPlayers.YouTubePlayer.callBaseMethod(this, "dispose");
        },
        loadFile: function (c)
        {
            this.currentMediaFile=c;
            if (c.options.autoPlay)
            {
                this.media.loadVideoById(this.getMediaId());
            } else
            {
                this.media.cueVideoById(this.getMediaId());
            }
            if (c.options.startVolume!=-1)
            {
                this.set_volume(c.options.startVolume);
            }
            if (c.options.duration)
            {
                this.owner.toolbar.setProgressRailMaxValue(c.options.duration);
                this.owner.toolbar.setDuration(this.owner._getTimeString(c.options.duration));
            }
        },
        play: function ()
        {
            this.media.playVideo();
        },
        pause: function ()
        {
            this.media.pauseVideo();
        },
        stop: function ()
        {
            this.media.stopVideo();
        },
        mute: function ()
        {
            this._lastVolume=this.media.getVolume();
            this.media.mute();
        },
        unmute: function ()
        {
            if (this._lastVolume>=0)
            {
                this.media.setVolume(this._lastVolume);
            }
            this.media.unMute();
        },
        startSeek: function ()
        {
            this.set_uiSeeking(true);
            this.set_playing(false);
        },
        seekTo: function (c)
        {
            if (c+3>=this.media.getDuration()|0)
            {
                this.media.seekTo(this.media.getDuration()-3|0, true);
            } else
            {
                this.media.seekTo(c, true);
            }
        },
        set_volume: function (c)
        {
            this._lastVolume=c;
            if (!this.isMuted())
            {
                this.media.setVolume(c);
            }
        },
        get_volume: function ()
        {
            if (this.media)
            {
                if (this.isMuted()&&this._lastVolume>=0)
                {
                    return this._lastVolume;
                }
                return this.media.getVolume();
            } else
            {
                return this.currentMediaFile.options.startVolume;
            }
        },
        get_currentTime: function ()
        {
            if (this.media)
            {
                return this.media.getCurrentTime();
            } else
            {
                return 0;
            }
        },
        set_currentTime: function (c)
        {
            this.seekTo(c);
        },
        get_playerType: function ()
        {
            return b.MediaPlayerType.YouTube;
        },
        getMediaId: function ()
        {
            var e=this.currentMediaFile.path;
            var d=/^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#\&\?]*).*/;
            var c=this.currentMediaFile.path.match(d);
            if (c&&c[7].length==11)
            {
                e=c[7];
            }
            return e;
        },
        getAvailableQualityLevels: function ()
        {
            return this.media.getAvailableQualityLevels();
        },
        toggleHD: function (c)
        {
            var d=this.getAvailableQualityLevels();
            if (d!=null&&d.length>0)
            {
                this.media.setPlaybackQuality(d[0]);
            }
        },
        isPaused: function ()
        {
            return this.media.getPlayerState()==window.YT.PlayerState.PAUSED;
        },
        isPlaying: function ()
        {
            return this.media.getPlayerState()==window.YT.PlayerState.PLAYING;
        },
        isEnded: function ()
        {
            return this.media.getPlayerState()==window.YT.PlayerState.ENDED;
        },
        isMuted: function ()
        {
            return this.media? this.media.isMuted():false;
        },
        _initializePlayer: function ()
        {
            if (!window.YT||!window.YT.Player)
            {
                a.getScript("https://www.youtube.com/iframe_api");
                b.MediaPlayers.YouTubePlayer._playersNeedConfigure[b.MediaPlayers.YouTubePlayer._playersNeedConfigure.length]=this;
                window.onYouTubeIframeAPIReady=b.MediaPlayers.YouTubePlayer._onYouTubeIframeAPIReady;
            } else
            {
                this._configurePlayer();
            }
        },
        _createMediaElement: function ()
        {
            var c=document.createElement("div");
            c.setAttribute("id", this._ytPlayerElementID);
            this.owner.get_element().insertBefore(c, this.owner.get_element().children[1]);
            this.ytPlayerElement=c;
        },
        _configurePlayer: function ()
        {
            var e=this.currentMediaFile.options;
            var g={
                autoplay: e.autoPlay? 1:0,
                wmode: "transparent",
                controls: 0,
                rel: 0,
                showinfo: 0,
                start: e.startTime
            };
            var c=this.owner.get_element()
                , h=c.style.width
                , d=c.style.height;
            if (isNaN(h)||h=="")
            {
                h=c.offsetWidth;
            }
            if (isNaN(d))
            {
                d=c.offsetHeight;
            }
            var f=new window.YT.Player(this._ytPlayerElementID, {
                height: d,
                width: h,
                videoId: this.getMediaId(),
                playerVars: g,
                events: {
                    onReady: a.proxy(this._onPlayerReady, this),
                    onStateChange: a.proxy(this._onPlayerStateChange, this)
                }
            });
            if ($telerik.isIE7)
            {
                a("#"+this._ytPlayerElementID).css("height", d).css("width", h);
            } else
            {
                if (!this._isInFullScreen)
                {
                    a("#"+this._ytPlayerElementID).css("height", d).css("width", h);
                }
            }
        },
        _onDurationChanged: function (c)
        {
            this.owner._progressRail.set_maximumValue(this.media.duration);
        },
        _onPlayerReady: function (c)
        {
            this.media=c.target;
            if (this.media.getDuration())
            {
                this._configMedia(false);
            } else
            {
                if (this.currentMediaFile.options.duration>0)
                {
                    this._configMedia(true);
                }
            }
        },
        _configMedia: function (c)
        {
            if (this._lastVolume>=0)
            {
                this.media.setVolume(this._lastVolume);
            } else
            {
                if (this.currentMediaFile.options.startVolume>=0)
                {
                    this.media.setVolume(this.currentMediaFile.options.startVolume);
                }
            }
            if (this.owner.options.startVolume>=0)
            {
                this.media.setVolume(this.owner.options.startVolume);
            }
            if (this.currentMediaFile.options.muted)
            {
                this.mute();
            }
            if (!c||this.media.getDuration())
            {
                this.currentMediaFile.options.duration=this.media.getDuration();
            }
            this.trigger("ready", {
                duration: this.currentMediaFile.options.duration
            });
        },
        _onPlayerStateChange: function (d)
        {
            if (d.data==0)
            {
                this.media.cueVideoById(this.getMediaId());
                this.media.seekTo(0, true);
                this.media.pauseVideo();
                this.owner.toolbar.setProgressRailValue(0);
                this.trigger("ended");
            } else
            {
                if (d.data==1)
                {
                    var c=this.media.getDuration();
                    if ((!this.currentMediaFile.options.duration)&&c>0)
                    {
                        this._configMedia(false);
                    }
                    this.trigger("play");
                    this.set_playing(true);
                    this.set_uiSeeking(false);
                    this._poll("progress", this._upDateProgress, 500, this);
                    this._poll("loading", this._loading, 1000, this);
                } else
                {
                    if (d.data==2)
                    {
                        this.trigger("pause");
                    } else
                    {
                        if (d.data==5)
                        {
                            this.trigger("ready", {
                                duration: this.currentMediaFile.options.duration
                            });
                        }
                    }
                }
            }
        },
        _loading: function ()
        {
            var d=this.media.getVideoLoadedFraction();
            var c=d<0.99;
            if (!c)
            {
                d=1;
            }
            this.trigger("loading", d*100);
            return c;
        },
        _upDateProgress: function ()
        {
            if (this.get_playing())
            {
                this.trigger("progress", this.get_currentTime());
            }
            return this.get_playing();
        },
        _changeFullScreen: function (c)
        {
            var d=c.keyCode||c.keyCode;
            if (d==27)
            {
                a(this.owner.get_element()).removeClass("rmpfullscreen");
                a(this.media).removeClass("rmpfullscreen");
            } else
            {
                if (d==13&&c.altKey)
                {
                    a(this.owner.get_element()).addClass("rmpfullscreen");
                    a(this.media).addClass("rmpfullscreen");
                }
            }
        },
        _enterFullScreen: function ()
        {
            var c=$get(this.owner.get_id()+"_ytPlayerElement");
            this.owner.get_element().className+=" rmpFullscreen";
            if (c.requestFullScreen)
            {
                this.owner.get_element().requestFullScreen();
            } else
            {
                if (c.mozRequestFullScreen)
                {
                    this.owner.get_element().mozRequestFullScreen();
                } else
                {
                    if (c.webkitRequestFullScreen)
                    {
                        this.owner.get_element().webkitRequestFullScreen();
                    }
                }
            }
            this.owner.dimensions.height=c.style.height;
            this.owner.dimensions.width=c.style.width;
            this._isInFullScreen=true;
            c.style.width="100%";
            c.style.height="100%";
        },
        _exitFullScreen: function ()
        {
            if (this._isInFullScreen)
            {
                this._isInFullScreen=false;
                var f=$get(this._ytPlayerElementID);
                $telerik.$(this.owner.get_element()).removeClass("rmpFullscreen");
                if (document.cancelFullScreen)
                {
                    document.cancelFullScreen();
                } else
                {
                    if (document.mozCancelFullScreen)
                    {
                        document.mozCancelFullScreen();
                    } else
                    {
                        if (document.webkitCancelFullScreen)
                        {
                            document.webkitCancelFullScreen();
                        }
                    }
                }
                var e=this.owner.dimensions.width
                    , d=this.owner.dimensions.height;
                if ($telerik.isIE7)
                {
                    var c=this.owner.get_element();
                    e=c.offsetWidth;
                    d=c.offsetHeight;
                }
                if (e!="")
                {
                    f.style.width=e;
                }
                if (d!="")
                {
                    f.style.height=d;
                }
            }
        }
    };
    b.MediaPlayers.YouTubePlayer.registerClass("Telerik.Web.UI.MediaPlayers.YouTubePlayer", Telerik.Web.UI.MediaPlayers.MediaPlayerBase);
}
)();
Type.registerNamespace("Telerik.Web.UI.MediaPlayers");
(function ()
{
    var a=$telerik.$;
    var b=Telerik.Web.UI;
    b.MediaPlayers.Html5Player=function (f, e, d)
    {
        this.owner=f;
        var c=[];
        c[0]=e;
        b.MediaPlayers.Html5Player.initializeBase(this, c);
        this.eventsPanel=$get("pnlEventsHtml5");
        this.currentMediaFile=d;
        this.jmedia=null;
        this.initialize();
        this.bytesTotal=null;
        this.bytesLoaded=null;
        this.seeking=false;
        this._fullScreenEventCancel=false;
        this.i=0;
    }
        ;
    b.MediaPlayers.Html5Player.canPlay=function (d, c)
    {
        switch (c.mimeType)
        {
            case "video/ogg":
                return !!d.videoOGG;
            case "video/mp4":
            case "video/x-mp4":
            case "video/m4v":
            case "video/x-m4v":
                return !!d.videoH264;
            case "application/vnd.apple.mpegurl":
                return !!d.videoMPEGURL;
            case "video/x-webm":
            case "video/webm":
            case "application/octet-stream":
                return !!d.videoWEBM;
            case "audio/ogg":
                return !!d.audioOGG;
            case "audio/mpeg":
                return !!d.audioMP3;
            case "audio/mp4":
                return !!d.audioMP4;
            case "audio/wav":
                return !!d.audioWAV;
            default:
                return false;
        }
    }
        ;
    b.MediaPlayers.Html5Player.prototype={
        initialize: function ()
        {
            b.MediaPlayers.Html5Player.callBaseMethod(this, "initialize");
            this._createMediaElement();
            this.jmedia=a(this.media);
            this._adjustSize();
            this._addMediaEvents();
            this._initializeMedia();
        },
        dispose: function ()
        {
            this.jmedia.off();
            b.MediaPlayers.Html5Player.callBaseMethod(this, "dispose");
        },
        loadFile: function (c)
        {
            this.currentMediaFile=c;
            a(this.media).children().remove();
            this._addSources("sources");
            this._initializeMedia();
            this.media.load();
        },
        play: function ()
        {
            this.media.play();
        },
        pause: function ()
        {
            this.media.pause();
        },
        stop: function ()
        {
            this.media.stop();
        },
        mute: function ()
        {
            this.media.muted=true;
        },
        unmute: function ()
        {
            this.media.muted=false;
        },
        loop: function ()
        {
            this.media.loop=!this.media.loop;
        },
        get_volume: function ()
        {
            return this.media.volume*100;
        },
        set_volume: function (c)
        {
            this.media.volume=c/100;
        },
        get_currentTime: function ()
        {
            return this.media.currentTime;
        },
        set_currentTime: function (c)
        {
            this.media.currentTime=c;
        },
        set_autoPlay: function (c)
        {
            this.media.autoplay=c;
        },
        get_autoPlay: function ()
        {
            return this.media.autoplay;
        },
        get_playerType: function ()
        {
            return b.MediaPlayerType.HTML5;
        },
        _enterFullScreen: function ()
        {
            this.owner.get_element().className+=" rmpFullscreen";
            if (this.media.requestFullScreen)
            {
                this.media.requestFullScreen();
            } else
            {
                if (this.media.mozRequestFullScreen)
                {
                    this.media.mozRequestFullScreen();
                } else
                {
                    if (this.media.webkitRequestFullScreen)
                    {
                        this.owner.get_element().webkitRequestFullScreen();
                        this.owner.dimensions.height=this.media.style.height;
                        this.owner.dimensions.width=this.media.style.width;
                        this.media.style.width="100%";
                        this.media.style.height="100%";
                    } else
                    {
                        this.owner.dimensions.height=this.media.style.height;
                        this.owner.dimensions.width=this.media.style.width;
                        this.media.style.width="100%";
                        this.media.style.height="100%";
                    }
                }
            }
        },
        _exitFullScreen: function ()
        {
            this._fullScreenEventCancel=true;
            $telerik.$(this.owner.get_element()).removeClass("rmpFullscreen");
            if (document.cancelFullScreen)
            {
                document.cancelFullScreen();
            } else
            {
                if (document.mozCancelFullScreen)
                {
                    document.mozCancelFullScreen();
                } else
                {
                    if (document.webkitCancelFullScreen)
                    {
                        document.webkitCancelFullScreen();
                        this.media.style.width=this.owner.dimensions.width;
                        this.media.style.height=this.owner.dimensions.height;
                    } else
                    {
                        this.media.style.width=this.owner.dimensions.width;
                        this.media.style.height=this.owner.dimensions.height;
                    }
                }
            }
        },
        isFailedOnError: function ()
        {
            return this.media.error!=null;
        },
        isBlocked: function ()
        {
            var c=false;
            if (this.media.readyState==0||this.media.readyState==1||this.media.readyState==2)
            {
                return false;
            }
            return c;
        },
        isPaused: function ()
        {
            return this.media.paused;
        },
        isMuted: function ()
        {
            return this.media.muted;
        },
        isNotStarted: function ()
        {
            return this.media.paused&&this.media.currentTime==0;
        },
        isEnded: function ()
        {
            return this.media.ended;
        },
        isPlaying: function ()
        {
            var c=true;
            if (this.isPaused()||this.isEnded()||this.isBlocked()||this.isFailedOnError())
            {
                c=false;
            }
            return c;
        },
        isSeeking: function ()
        {
            return this.media.seeking;
        },
        getBytesLoaded: function ()
        {
            var c=0;
            if (this.bytesLoaded)
            {
                c=this.bytesLoaded.value;
            } else
            {
                if (this.media.buffered&&this.media.buffered.length>0&&this.media.buffered.end&&this.media.duration)
                {
                    c=this.media.buffered.end(0);
                } else
                {
                    if (this.media.bytesTotal!==undefined&&this.media.bytesTotal>0&&this.media.bufferedBytes!==undefined)
                    {
                        c=this.media.bufferedBytes;
                    }
                }
            }
            return c;
        },
        getBytesTotal: function ()
        {
            var c=0;
            if (this.bytesTotal)
            {
                c=this.bytesTotal;
            } else
            {
                if (this.media.buffered&&this.media.buffered.length>0&&this.media.buffered.end&&this.media.duration)
                {
                    c=this.media.duration;
                } else
                {
                    if (this.media.bytesTotal!==undefined&&this.media.bytesTotal>0&&this.media.bufferedBytes!==undefined)
                    {
                        c=this.media.bytesTotal;
                    }
                }
            }
            return c;
        },
        getConcerningRange: function ()
        {
            var g=this.media.currentTime||0
                , c=this.media.buffered
                , d=c.length
                , f={};
            for (var e=0; e<d; e++)
            {
                f.start=c.start(e);
                f.end=c.end(e);
                if (f.start<=g&&f.end>=g)
                {
                    break;
                }
            }
            return f;
        },
        calculateProgress: function (f)
        {
            var g={}, d, c;
            if (this.media.buffered&&this.media.buffered.length)
            {
                d=this.media.duration;
                if (d)
                {
                    c=this.getConcerningRange();
                    g.relStart=c.start/d*100;
                    g.relLoaded=c.end/d*100;
                }
            } else
            {
                if (f.originalEvent&&"lengthComputable" in f.originalEvent&&f.originalEvent.loaded)
                {
                    if (f.originalEvent.lengthComputable&&f.originalEvent.total)
                    {
                        g.relStart=0;
                        g.relLoaded=f.originalEvent.loaded/f.originalEvent.total*100;
                    }
                }
            }
            if (!g.relLoaded&&this.media.readyState===4)
            {
                g.relStart=0;
                g.relLoaded=100;
            }
            return g.relLoaded;
        },
        startSeek: function ()
        {
            this.set_uiSeeking(true);
        },
        seekTo: function (c)
        {
            if (this.media.seekable.length>0)
            {
                var e=this.media.seekable.start(0);
                var d=this.media.seekable.end(0);
                if (c>=e&&c<=d)
                {
                    this.media.currentTime=c;
                }
            }
            this.set_uiSeeking(false);
        },
        _initializeMedia: function ()
        {
            this.set_autoPlay(this.currentMediaFile.options.autoPlay);
            if (this.currentMediaFile.options.poster)
            {
                this.media.poster=this.currentMediaFile.options.poster;
            }
        },
        _createMediaElement: function ()
        {
            var f=this.currentMediaFile.get_type();
            if (f=="audio")
            {
                var d=this.options.ownerID+"_audio";
                var c=document.createElement("audio");
                c.setAttribute("id", d);
                this.media=c;
                var e=document.createElement("div");
                e.setAttribute("class", "rmpAudioWrapper");
                e.appendChild(c);
                this.owner.get_element().insertBefore(e, this.owner.get_element().children[0]);
            } else
            {
                if (f=="video")
                {
                    var h=this.options.ownerID+"_video";
                    var g=document.createElement("video");
                    g.setAttribute("id", h);
                    this.media=g;
                    this.owner.get_element().insertBefore(this.media, this.owner.get_element().children[0]);
                }
            }
            this._addSources("sources");
        },
        toggleHD: function (c)
        {
            __doPostBack(this.owner.get_uniqueID(), c);
        },
        _addSources: function (g)
        {
            var f;
            var d="";
            if (this.currentMediaFile.path&&!this.currentMediaFile.path.endsWith(".flv"))
            {
                f=document.createElement("source");
                d=this.currentMediaFile.path;
                f.setAttribute("src", d);
                this.media.appendChild(f);
            }
            var c=this.currentMediaFile.options[g].length;
            while (c--)
            {
                var e=this.currentMediaFile.options[g][c];
                if (e.path!=d&&e.path!=""&&!e.path.endsWith(".flv"))
                {
                    f=document.createElement("source");
                    f.setAttribute("src", e.path);
                    this.media.appendChild(f);
                }
            }
        },
        _adjustSize: function ()
        {
            this.jmedia.css("width", "100%");
            this.jmedia.css("height", "100%");
        },
        _addMediaEvents: function ()
        {
            this.jmedia.on({
                loadedmetadata: a.proxy(this._onMetaDataLoaded, this),
                play: a.proxy(this._onPlay, this),
                pause: a.proxy(this._onPause, this),
                ended: a.proxy(this._onEnded, this),
                timeupdate: a.proxy(this._onTimeUpdate, this),
                error: a.proxy(this._onError, this),
                progress: a.proxy(this._onProgress, this),
                waiting: a.proxy(this._onwaiting, this),
                playing: a.proxy(this._onReadyToStart, this),
                seeking: a.proxy(this._onSeeking, this),
                seeked: a.proxy(this._onSeeked, this)
            });
            this.owner._joverlay.on({
                click: a.proxy(this._onClick, this)
            });
        },
        _onReadyToStart: function ()
        {
            this.trigger("readytostart");
        },
        _onSeeking: function ()
        {
            this.trigger("seeking");
        },
        _onSeeked: function ()
        {
            this.trigger("seeked");
        },
        _onCanPlay: function ()
        {
            this.trigger("canplay");
        },
        _onLoadedData: function ()
        {
            this.trigger("loadeddata");
        },
        _onwaiting: function ()
        {
            this.trigger("waiting");
        },
        _onClick: function ()
        {
            if (this.isPlaying())
            {
                this.pause();
            } else
            {
                this.play();
            }
        },
        _changeFullScreen: function (c)
        {
            var d=c.keyCode||c.keyCode;
            if (d==27)
            {
                a(this.owner.get_element()).removeClass("rmpfullscreen");
                this.jmedia.removeClass("rmpfullscreen");
            } else
            {
                if (d==13&&c.altKey)
                {
                    a(this.owner.get_element()).addClass("rmpfullscreen");
                    this.jmedia.addClass("rmpfullscreen");
                }
            }
        },
        _onPlay: function ()
        {
            this.trigger("play");
        },
        _onPause: function ()
        {
            this.trigger("pause");
        },
        _onProgress: function (c)
        {
            var d=this.calculateProgress(c);
            if (d>98)
            {
                d=100;
            }
            this.trigger("loading", d);
        },
        _onTimeUpdate: function ()
        {
            this.trigger("progress", this.media.currentTime);
        },
        _onEnded: function ()
        {
            this.media.currentTime=0;
            this.media.pause();
            this.trigger("ended");
        },
        _onError: function (c)
        {
            var d="";
            if (c.target&&c.target.error)
            {
                switch (c.target.error.code)
                {
                    case c.target.error.MEDIA_ERR_ABORTED:
                        d="You aborted the video playback.";
                        break;
                    case c.target.error.MEDIA_ERR_NETWORK:
                        d="A network error caused the video download to fail part-way.";
                        break;
                    case c.target.error.MEDIA_ERR_DECODE:
                        d="The video playback was aborted due to a corruption problem or because the video used features your browser did not support.";
                        break;
                    case c.target.error.MEDIA_ERR_SRC_NOT_SUPPORTED:
                        d="The video could not be loaded, either because the server or network failed or because the format is not supported.";
                        break;
                    default:
                        d="An unknown error occurred.";
                        break;
                }
                this.trigger("error", d);
            }
        },
        _onMetaDataLoaded: function ()
        {
            var d=this
                , c=this.currentMediaFile.options;
            c.duration=d.media.duration;
            if (c.startTime>0)
            {
                d.startSeek();
                d.seekTo(c.startTime);
            }
            if (c.startVolume!=-1)
            {
                d.set_volume(c.startVolume);
            } else
            {
                if (d.owner.options.startVolume!=-1)
                {
                    d.set_volume(d.owner.options.startVolume);
                }
            }
            if (c.muted)
            {
                d.mute();
            }
            d.trigger("ready", c);
        }
    };
    b.MediaPlayers.Html5Player.registerClass("Telerik.Web.UI.MediaPlayers.Html5Player", Telerik.Web.UI.MediaPlayers.MediaPlayerBase);
}
)();
Type.registerNamespace("Telerik.Web.UI.MediaPlayers");
(function ()
{
    var a=$telerik.$;
    var b=Telerik.Web.UI;
    b.MediaPlayers.FlashPlayer=function (f, e, d)
    {
        this.owner=f;
        this.progressInterval=null;
        this.temporaryVolume=-1;
        this._flashPlayerElementID=this.owner.get_id()+"_flashPlayerElement";
        var c=[];
        c[0]=e;
        b.MediaPlayers.FlashPlayer.initializeBase(this, c);
        this.eventsPanel=$get("pnlEventsFlash");
        this.currentMediaFile=d;
        this.initialize();
    }
        ;
    b.MediaPlayers.FlashPlayer.canPlay=function (d, c)
    {
        if (c.mimeType==="video/x-flv"||c.mimeType==="video/mp4"||c.mimeType==="audio/mpeg"||c.mimeType==="audio/mp4")
        {
            return true;
        }
        return false;
    }
        ;
    b.MediaPlayers.FlashPlayer.prototype={
        initialize: function ()
        {
            b.MediaPlayers.FlashPlayer.callBaseMethod(this, "initialize");
            this._createMediaElement();
            this.owner._joverlay.on({
                click: a.proxy(this._onClick, this)
            });
            var c=this.currentMediaFile.options
                , d=this.owner.options;
            if (c.startVolume!=-1)
            {
                this.temporaryVolume=c.startVolume;
            } else
            {
                if (d.startVolume!=-1)
                {
                    this.temporaryVolume=d.startVolume;
                }
            }
        },
        dispose: function ()
        {
            this.owner._joverlay.off();
            b.MediaPlayers.FlashPlayer.callBaseMethod(this, "dispose");
        },
        loadFile: function (c)
        {
            var d=this;
            d.currentMediaFile=c;
            if (c.options.startVolume!=-1)
            {
                d.set_volume(c.options.startVolume);
            }
            setTimeout(function ()
            {
                d.stop();
                a(d.media).remove();
                d._createMediaElement();
                if (d.currentMediaFile.options.autoPlay)
                {
                    d._removePoster();
                    d.owner.toolbar.setCenterPlayButtonState(true);
                    d.owner.toolbar.setPlayButtonState(true);
                } else
                {
                    d.owner.toolbar.setCenterPlayButtonState(false);
                    d.owner.toolbar.setPlayButtonState(false);
                }
            }, 10);
        },
        _onClick: function ()
        {
            var c=this;
            if (c.isPlaying())
            {
                c.pause();
            } else
            {
                c.play();
            }
        },
        _trackProgress: function ()
        {
            var c=this;
            c.progressInterval=setInterval(function ()
            {
                if (c.isPlaying())
                {
                    c.trigger("progress", c.get_currentTime());
                } else
                {
                    if (!c.isPaused()||c.currentMediaFile.path=="")
                    {
                        c.stop();
                        c.seekTo(0);
                        c.trigger("ended");
                        c.trigger("pause");
                        clearInterval(c.progressInterval);
                    }
                }
            }, 500);
        },
        play: function ()
        {
            var c=this;
            c._removePoster();
            if (c.currentMediaFile.path!="")
            {
                c.media.PlayMedia();
            }
            c._trackProgress();
            c.trigger("play");
        },
        pause: function ()
        {
            var c=this;
            clearInterval(c.progressInterval);
            c.media.PauseMedia();
            c.trigger("pause");
        },
        stop: function ()
        {
            clearInterval(this.progressInterval);
            try
            {
                this.media.StopMedia();
            } catch (c) { }
        },
        mute: function ()
        {
            var c=this;
            c.temporaryVolume=c.get_volume();
            c.media.Mute();
        },
        unmute: function ()
        {
            var c=this;
            c.media.UnMute();
            c.set_volume(c.temporaryVolume);
        },
        startSeek: function ()
        {
            this.set_uiSeeking(true);
            this.set_playing(false);
        },
        seekTo: function (c)
        {
            var d=this;
            clearInterval(d.progressInterval);
            d.trigger("progress", c);
            d.media.SeekTo(c);
            d._trackProgress();
        },
        set_volume: function (e)
        {
            var c=this;
            var d=false;
            if (this.isMuted())
            {
                d=true;
            }
            c.temporaryVolume=e;
            c.media.SetMediaVolume(e);
            if (d)
            {
                c.mute();
            }
        },
        get_volume: function ()
        {
            var d=this;
            try
            {
                if (d.isMuted())
                {
                    return d.temporaryVolume;
                } else
                {
                    return d.media.GetVolume();
                }
            } catch (c)
            {
                return d.temporaryVolume;
            }
        },
        get_currentTime: function ()
        {
            try
            {
                return this.media.GetTime();
            } catch (c)
            {
                return 0;
            }
        },
        set_currentTime: function (c)
        {
            this.seekTo(c);
        },
        get_playerType: function ()
        {
            return b.MediaPlayerType.Flash;
        },
        toggleHD: function (c)
        {
            __doPostBack(this.owner.get_uniqueID(), c);
        },
        isPaused: function ()
        {
            try
            {
                return this.media.IsPaused();
            } catch (c)
            {
                if (this.progressInterval)
                {
                    clearInterval(this.progressInterval);
                }
                return true;
            }
        },
        isPlaying: function ()
        {
            try
            {
                return (!this.media.IsPaused()&&!this.media.IsEnded()&&(this.media.GetDuration()-this.media.GetTime()>0.1));
            } catch (c)
            {
                return false;
            }
        },
        isEnded: function ()
        {
            return this.media.IsEnded();
        },
        isMuted: function ()
        {
            return this.media.IsMuted();
        },
        _toAbsoultePath: function (d)
        {
            var c=location.href;
            c=c.substring(0, c.lastIndexOf("/"));
            while (/^\.\./.test(d))
            {
                c=c.substring(0, c.lastIndexOf("/"));
                d=d.substring(3);
            }
            return c+"/"+d;
        },
        _S4: function ()
        {
            return (((1+Math.random())*65536)|0).toString(16).substring(1);
        },
        _guid: function ()
        {
            var c=this;
            return (c._S4()+c._S4()+"-"+c._S4()+"-"+c._S4()+"-"+c._S4()+"-"+c._S4()+c._S4()+c._S4());
        },
        _createMediaElement: function ()
        {
            var r=this
                , h=r.owner.options._flashModuleUrl+"?guid="+r._guid()
                , g=r.owner.get_element()
                , n=r.currentMediaFile.options
                , e=n.autoPlay
                , q=n.poster
                , u=g.style.width.replace("px", "")*1
                , k=g.style.height.replace("px", "")*1
                , p=r.currentMediaFile.path
                , s=n.sources
                , t=this.currentMediaFile.get_type()
                , m=$telerik.isIE&&($telerik.isIE10Mode&&!$telerik.isIE10);
            if (isNaN(u))
            {
                u=g.offsetWidth;
            }
            if (isNaN(k))
            {
                k=g.offsetHeight;
            }
            if (r.currentMediaFile.path&&b.MediaPlayers.FlashPlayer.canPlay(null, r.currentMediaFile)&&r.currentMediaFile.path.indexOf("http")!=0)
            {
                p=r._toAbsoultePath(r.currentMediaFile.path);
            }
            for (var l=0; l<s.length; l++)
            {
                if (b.MediaPlayers.FlashPlayer.canPlay(null, s[l]))
                {
                    if (r.currentMediaFile.path.indexOf("http")!=0)
                    {
                        p=r._toAbsoultePath(s[l].path);
                    }
                    if (!t)
                    {
                        r.currentMediaFile.mimeType=s[l].mimeType;
                        t=r.currentMediaFile.get_type();
                    }
                    break;
                }
            }
            var j="url="+p+"&width="+u+"&height="+k+"&id="+this.owner.get_id()+"&autoPlay="+e+"&isVideo="+(t=="video");
            if ($telerik.isIE&&!m)
            {
                var f=document.createElement("div");
                f.innerHTML='<object classid="clsid:d27cdb6e-ae6d-11cf-96b8-444553540000" width="'+u+'" name="'+r._flashPlayerElementID+'" height="'+k+'" id="'+r._flashPlayerElementID+'"><param name="allowscriptaccess" value="always"/><param name="movie" value="'+h+'" /><param name="flashVars" value="'+j+'" /><param name="wmode" value="transparent" /><param name="allowFullScreen" value="true" /><a href="http://www.adobe.com/go/getflash"><img src="http://www.adobe.com/images/shared/download_buttons/get_flash_player.gif"alt="Get Adobe Flash player"/></a></object>';
                r.media=f.firstChild;
            } else
            {
                r.media=document.createElement("object");
                r.media.setAttribute("id", r._flashPlayerElementID);
                r.media.setAttribute("type", "application/x-shockwave-flash");
                r.media.setAttribute("data", h);
                var d=document.createElement("param");
                d.setAttribute("name", "allowscriptaccess");
                d.setAttribute("value", "always");
                r.media.appendChild(d);
                var o=document.createElement("param");
                o.setAttribute("name", "flashVars");
                o.setAttribute("value", j);
                r.media.appendChild(o);
                var v=document.createElement("param");
                v.setAttribute("name", "wmode");
                v.setAttribute("value", "transparent");
                r.media.appendChild(v);
                var c=document.createElement("param");
                c.setAttribute("name", "allowFullScreen");
                c.setAttribute("value", "true");
                r.media.appendChild(c);
                a(r.media).css("height", k+"px");
                a(r.media).css("width", u+"px");
            }
            if (t=="audio")
            {
                this.owner._joverlay.addClass("rmpAudioWrapper");
            } else
            {
                if (t=="video"&&q&&!e)
                {
                    r._addPoster(q);
                }
            }
            g.insertBefore(r.media, g.children[1]);
        },
        _addPoster: function (d)
        {
            var c=this.owner._joverlay;
            c.css("background-image", "url("+d+")");
            c.addClass("rmpPosterImage");
        },
        _removePoster: function ()
        {
            var c=this.owner._joverlay;
            c.css("background-image", "");
            c.removeClass("rmpPosterImage");
        },
        _onPlayerReady: function ()
        {
            var e=this;
            var c=e.currentMediaFile.options;
            var d=e.owner.options;
            if (c.startVolume!=-1)
            {
                e.set_volume(c.startVolume);
            } else
            {
                if (d.startVolume!=-1)
                {
                    e.set_volume(d.startVolume);
                }
            }
            if (c.startTime>0)
            {
                e.startSeek();
                e.seekTo(c.startTime);
            }
            this.trigger("ready", {
                duration: e.media.GetDuration()
            });
        },
        _loading: function ()
        {
            var f=0;
            try
            {
                f=this.media.GetLoadingPercent();
            } catch (d) { }
            var c=f<0.99;
            if (!c)
            {
                f=1;
            }
            this.trigger("loading", f*100);
            return c;
        },
        _enterFullScreen: function ()
        {
            var f=this
                , e=f.owner
                , d=f.media;
            e.get_element().className+=" rmpFullscreen";
            var g=e.get_element().clientWidth;
            var c=e.get_element().clientHeight;
            d.width=g;
            d.height=c;
            a(f.media).css("height", c+"px");
            a(f.media).css("width", g+"px");
            d.EnterFullScreen(g, c);
        },
        _exitFullScreen: function ()
        {
            a(this.owner.get_element()).removeClass("rmpFullscreen");
            if (this.media.ExitFullScreen!==undefined)
                this.media.ExitFullScreen();
        }
    };
    b.MediaPlayers.FlashPlayer.registerClass("Telerik.Web.UI.MediaPlayers.FlashPlayer", Telerik.Web.UI.MediaPlayers.MediaPlayerBase);
}
)();
Telerik.Web.UI.MediaPlayers.Ready=function (b)
{
    var c=this;
    var a=$find(b).currentPlayer;
    a._onPlayerReady();
    if (a.currentMediaFile.options.autoPlay)
    {
        a._trackProgress();
    }
    if (a.currentMediaFile.options.muted)
    {
        a.mute();
    }
    c.loadingPercent=setInterval(function ()
    {
        var d=a._loading();
        if (!d)
        {
            clearInterval(c.loadingPercent);
        }
    }, 500);
}
    ;
Type.registerNamespace("Telerik.Web.UI");
(function (b, a)
{
    b.RadMediaPlayer=function (c)
    {
        b.RadMediaPlayer.initializeBase(this, [c]);
        this.ClientID=c.id;
        this.supportedPlayers={};
        this.currentPlayer=null;
        this.currentFile=null;
        this.source=null;
        this.mediaFormats=null;
        this.mediaFiles=[];
        this.toolbar=null;
        this.titlebar=null;
        this.playlist=null;
        this._mediaFilesData=null;
        this.dimensions=[];
        this._uniqueID=null;
        this._fsActive=null;
        this._renderMode=null;
        this._toolBarDocked=null;
        this._loadingIndicator=null;
        this._jdocument=a(document);
        this._joverlay=a($get(this.get_id()+"_Overlay"));
        this._jplayList=a($get(this.get_id()+"_Playlist_Layout"));
        this.options=null;
        this._selectedIndex=0;
        this._banners=null;
        this._jbannerCloseButton=null;
        this._bannerWrapper=null;
        this._scriptOuter=null;
        this._scriptInner=null;
    }
        ;
    b.RadMediaPlayer.prototype={
        initialize: function ()
        {
            b.RadMediaPlayer.callBaseMethod(this, "initialize");
            this.loadingIndicator=$find(this.get_id()+"_LoadingIndicator");
            this.determineSupportedMedia();
            if (this.options._enableAriaSupport)
            {
                this._initializeAriaSupport();
            }
            this._initializeMediaFiles();
            this._initializePlaylist();
            this._addPlayerDefinitions();
            this._initializeCurrentPlayer();
            this._initializeBanners();
            if (this.currentPlayer!=null)
            {
                this._loadingIndicator=$find(this.get_id()+"_LoadingIndicator");
                this._initializeToolbar();
                this._initializeTitleBar();
                this._intializeGlobalEvents();
                if (this.options.fsActive)
                {
                    var c=this;
                    setTimeout(function ()
                    {
                        c.enterFullScreen();
                    }, 10);
                }
            }
        },
        dispose: function ()
        {
            if (this.currentPlayer!=null)
            {
                this.toolbar.off();
                this.currentPlayer.dispose();
                this._jdocument.off("keydown");
                this._jdocument.off("webkitfullscreenchange");
                this._jdocument.off("mozfullscreenchange");
                this._jdocument.off("fullscreenchange");
            }
            if (this._jbannerCloseButton!=null)
            {
                this._jbannerCloseButton.off("click");
            }
        },
        get_rippleZonesConfiguration: function ()
        {
            var d=".rmpToolbar button, .rmpTitleBar button";
            var c=this.get_element().querySelectorAll(d);
            var e=26;
            if (c.length)
            {
                e=c[0].offsetWidth/0.6;
            }
            return [{
                element: this.get_element(),
                rippleConfigurations: [{
                    containerSelector: d,
                    rippleType: Telerik.Web.UI.MaterialRippleType.Icon,
                    maxRippleSize: e
                }, {
                    containerSelector: ".rmpPlaylist button, .rmpBanner button"
                }, {
                    containerSelector: ".rmpBigPlayButton",
                    rippleType: Telerik.Web.UI.MaterialRippleType.Icon,
                    maxRippleSize: 130
                }]
            }];
        },
        _addPlayerDefinitions: function ()
        {
            this.supportedPlayers.HTML5=b.MediaPlayers.Html5Player;
            this.supportedPlayers.YouTube=b.MediaPlayers.YouTubePlayer;
        },
        _initializeAriaSupport: function ()
        {
            var c=this;
            c.get_element().setAttribute("aria-label", "RadMediaPlayer control");
        },
        _initializePlaylist: function ()
        {
            var c=$get(this.get_id()+"_Playlist_Layout");
            if (!c)
            {
                return;
            }
            this.playlist=$create(b.MediaPlayerObjects.MediaPlayerPlaylist, null, null, {
                _owner: this.get_id()
            }, c);
            this.playlist.on({
                playListItemClick: this._playlistItemClicked
            }, this);
        },
        _playlistItemClicked: function (c)
        {
            if (!this.currentPlayer.media)
            {
                return;
            }
            this.options.selectedIndex=c.index;
            var e=this.mediaFiles[c.index]
                , d=e.options;
            this.currentFile=e;
            this.playlist.updateCurrentActiveItem();
            if (d.startVolume>0)
            {
                this.set_volume(d.startVolume);
            }
            if (this.currentPlayer.set_autoPlay)
            {
                this.currentPlayer.set_autoPlay(d.autoPlay);
            }
            this.currentPlayer.loadFile(e);
            this._reloadSubtitles=true;
            this.toolbar.setPlayButtonState(e.options.autoPlay);
            this.updateClientState();
        },
        _initializeMediaFiles: function ()
        {
            var e=this._mediaFilesData.length;
            var d=0;
            while (e--)
            {
                var c=new b.MediaPlayerObjects.MediaFile(this._mediaFilesData[d]);
                this.mediaFiles.push(c);
                d++;
            }
            this.currentFile=this.mediaFiles[this.options.selectedIndex||0];
        },
        _initializeCurrentPlayer: function ()
        {
            var c=this._getBestPlayer();
            if (c!=undefined)
            {
                this.currentPlayer=new this.supportedPlayers[c](this, {
                    ownerID: this.get_id()
                }, this.currentFile);
            }
            this._initializePlayerEvents();
            this._reloadSubtitles=true;
        },
        _initializePlayerEvents: function ()
        {
            if (this.currentPlayer)
            {
                this.currentPlayer.on({
                    ready: this._onPlayerReady,
                    progress: this._onUpdateProgress,
                    loading: this._onBytesLoading,
                    play: this._onPlayerPlay,
                    pause: this._onPlayerPause,
                    ended: this._onPlayerEnded
                }, this);
                if (this.currentPlayer.get_playerType()==b.MediaPlayerType.HTML5||this.currentPlayer.get_playerType()==b.MediaPlayerType.Flash)
                {
                    this.currentPlayer.on({
                        loadeddata: this._onPlayerLoadedData,
                        waiting: this._onPlayerWaiting,
                        readytostart: this._onPlayerReadyToStart,
                        seeking: this._onPlayerSeeking,
                        seeked: this._onPlayerSeeked,
                        canplay: this._onPlayerCanPlay,
                        error: this._onPlayerError
                    }, this);
                }
            }
        },
        _initializeToolbar: function ()
        {
            this.toolbar=new b.MediaPlayerObjects.MediaPlayerToolbar(this, a.extend(this.currentFile.options, {
                hdActive: this.options.hdActive,
                fsActive: this.options.fsActive,
                muted: this.options.muted,
                playerType: this.currentPlayer.get_playerType()
            }));
            this.toolbar.on({
                playToggle: this._onPlayButtonToggle,
                fullScreenToggle: this._onFullScreenToggle,
                hdToggle: this._onHDToggled,
                seekStart: this._onSeekStart,
                volumeChanged: this._onVolumeChanged,
                seekEnd: this._onSeekEnded,
                volumeControlToggle: this._onVolumeControlToggle,
                volumeControlClick: this._onVolumeControlClick,
                progressRailMouseMove: this._onProgressRailMouseMove,
                toolBarMouseMove: this._onToolbarMouseMove,
                ccToggle: this._onCCToggled
            }, this);
        },
        _initializeTitleBar: function ()
        {
            this.titlebar=new b.MediaPlayerObjects.MediaPlayerTitlebar(this);
            this.titlebar.on({
                titleBarMouseMove: this._onTitleBarMouseMove
            }, this);
        },
        _intializeGlobalEvents: function ()
        {
            this._jdocument.on("keydown", a.proxy(this._onKeyDown, this));
            this._jdocument.on("webkitfullscreenchange mozfullscreenchange fullscreenchange", a.proxy(this._onFullScreenChange, this));
            if ($telerik.isIE&&($telerik.isIE7||document.documentMode&&document.documentMode<=9))
            {
                this._joverlay.addClass(" rmpOverlaySolid");
            }
            this._joverlay.on("mousemove", a.proxy(this._onmousemove, this));
            var d=this
                , c=0;
            this._onResizeDelegate=Function.createDelegate(this._owner, function ()
            {
                clearTimeout(c);
                c=setTimeout(function ()
                {
                    clearTimeout(c);
                    if (d.get_element())
                    {
                        d.repaint();
                    }
                }, 100);
            });
            window.$addHandler(window, "resize", d._onResizeDelegate);
        },
        _getBestPlayer: function ()
        {
            var c=null;
            a.each(this.supportedPlayers, a.proxy(function (e, f)
            {
                if (f.canPlay(this.mediaFormats, this.currentFile))
                {
                    c=e;
                } else
                {
                    var g=this.currentFile.options.sources;
                    var d=g.length;
                    while (d--)
                    {
                        if (f.canPlay(this.mediaFormats, g[d]))
                        {
                            this.currentFile.path=g[d].path;
                            this.currentFile.mimeType=g[d].mimeType;
                            c=e;
                            break;
                        }
                    }
                }
            }, this));
            if (!c)
            {
                this.supportedPlayers.Flash=b.MediaPlayers.FlashPlayer;
                c="Flash";
                this._populateCurrentFileMeta(c, this.supportedPlayers.Flash);
            }
            return c;
        },
        _populateCurrentFileMeta: function (d, e)
        {
            var f=this.currentFile.options.sources;
            var c=f.length;
            while (c--)
            {
                if (e.canPlay(this.mediaFormats, f[c]))
                {
                    this.currentFile.path=f[c].path;
                    this.currentFile.mimeType=f[c].mimeType;
                    break;
                }
            }
        },
        _getEventName: function (c)
        {
            return String.format("{0}.{1}", c, this.get_id());
        },
        play: function ()
        {
            this.currentPlayer.play();
            this.updateClientState();
        },
        pause: function ()
        {
            this.currentPlayer.pause();
            this.updateClientState();
        },
        stop: function ()
        {
            this.currentPlayer.pause();
            this.updateClientState();
        },
        seekTo: function (c)
        {
            this.currentPlayer.seekTo(c);
            this._updateScript(c);
            this.updateClientState();
        },
        mute: function ()
        {
            if (this.currentPlayer)
            {
                this.currentPlayer.mute();
                this.updateClientState();
            }
        },
        unmute: function ()
        {
            if (this.currentPlayer)
            {
                this.currentPlayer.unmute();
                this.updateClientState();
            }
        },
        loop: function ()
        {
            this.currentPlayer.loop();
        },
        set_volume: function (c)
        {
            if (c>=0)
            {
                this.currentPlayer.set_volume(c);
                this.toolbar.setVolumeControlValue(c);
                this.updateClientState();
            }
        },
        get_volume: function ()
        {
            return this.currentPlayer.get_volume();
        },
        get_currentTime: function ()
        {
            return this.currentPlayer.get_currentTime();
        },
        set_currentTime: function (c)
        {
            this.currentPlayer.set_currentTime(c);
        },
        isPlaying: function ()
        {
            return this.currentPlayer.isPlaying();
        },
        isMuted: function ()
        {
            return this.currentPlayer.isMuted();
        },
        isPaused: function ()
        {
            return this.currentPlayer.isPaused();
        },
        isEnded: function ()
        {
            return this.currentPlayer.isEnded();
        },
        toggleFullScreen: function ()
        {
            var c=this.toolbar.toggleFullScreenButtonState();
            if (c)
            {
                this.enterFullScreen();
            } else
            {
                this.exitFullScreen();
            }
        },
        exitFullScreen: function ()
        {
            this.toolbar.setFullScreenButtonState(false);
            this.currentPlayer._exitFullScreen();
            if (this.playlist)
            {
                this.playlist._exitFullScreen();
            }
            this.updateClientState();
        },
        enterFullScreen: function ()
        {
            this.toolbar.setFullScreenButtonState(true);
            this.currentPlayer._enterFullScreen();
            if (this.playlist)
            {
                this.playlist._enterFullScreen();
            }
            this.updateClientState();
        },
        showControls: function ()
        {
            this.titlebar.slideDown();
            if (!this.get_toolbarDocked())
            {
                this.toolbar.show(1000);
            }
            if (this.currentPlayer!=null)
            {
                this.currentPlayer._poll("mouseidle", this._mouseidle, 3000, this);
                if (this.currentPlayer.get_playerType()==b.MediaPlayerType.YouTube)
                {
                    this._joverlay[0].style.display="none";
                }
            }
        },
        checkMediaFormat: function (d, e)
        {
            if ((typeof d.canPlayType)==="function")
            {
                if (typeof e==="object")
                {
                    var f=e.length;
                    var g="";
                    while (f--)
                    {
                        g=this.checkMediaFormat(d, e[f]);
                        if (!!g)
                        {
                            break;
                        }
                    }
                    return g;
                } else
                {
                    var c=d.canPlayType(e);
                    if (("no"!==c)&&(""!==c))
                    {
                        return e;
                    }
                }
            }
            return "";
        },
        determineSupportedMedia: function ()
        {
            this.mediaFormats={};
            var c=null;
            c=document.createElement("video");
            this.mediaFormats.videoOGG=this.checkMediaFormat(c, "video/ogg");
            this.mediaFormats.videoH264=this.checkMediaFormat(c, ["video/mp4", "video/h264"]);
            this.mediaFormats.videoWEBM=this.checkMediaFormat(c, ["video/x-webm", "video/webm", "application/octet-stream"]);
            this.mediaFormats.videoMPEGURL=this.checkMediaFormat(c, "application/vnd.apple.mpegurl");
            c=document.createElement("audio");
            this.mediaFormats.audioOGG=this.checkMediaFormat(c, "audio/ogg");
            this.mediaFormats.audioMP3=this.checkMediaFormat(c, "audio/mpeg");
            this.mediaFormats.audioWAV=this.checkMediaFormat(c, "audio/wav");
            this.mediaFormats.audioMP4=this.checkMediaFormat(c, "audio/mp4");
        },
        get_source: function ()
        {
            return this.currentFile.path;
        },
        get_hdActive: function ()
        {
            return this.options.hdActive;
        },
        get_fsActive: function ()
        {
            return a(this.get_element()).hasClass("rmpFullscreen");
        },
        get_options: function ()
        {
            return this.options;
        },
        set_options: function (c)
        {
            this.options=c;
        },
        get_toolbarDocked: function ()
        {
            return this.options.toolbarDocked;
        },
        get_loadingIndicator: function ()
        {
            return this._loadingIndicator;
        },
        set_loadingIndicator: function (c)
        {
            this._loadingIndicator=c;
        },
        shoot: function (d, e)
        {
            if (this._isCanvasSupported)
            {
                var g=this.currentPlayer.media;
                var c=this._capture(g, e);
                var f=[];
                c.onclick=function ()
                {
                    window.open(this.toDataURL());
                }
                    ;
                f.unshift(c);
                d.innerHTML="";
                d.appendChild(f[0]);
            }
        },
        getSnapshotDataUrl: function (h, f, i, j, e)
        {
            if (!Telerik.Web.BrowserFeatures.canvas)
            {
                return null;
            }
            var g=this.currentPlayer.media;
            if (typeof i==="undefined")
            {
                i=0;
            }
            if (typeof j==="undefined")
            {
                j=0;
            }
            if (typeof h==="undefined"||h==null)
            {
                h=g.videoWidth;
            }
            if (typeof f==="undefined"||f==null)
            {
                f=g.videoHeight;
            }
            if (!e)
            {
                e="image/png";
            }
            var c=document.createElement("canvas");
            c.width=h;
            c.height=f;
            var d=c.getContext("2d");
            d.drawImage(g, i, j, h, f);
            return c.toDataURL(e);
        },
        repaint: function ()
        {
            if (this.playlist)
            {
                this.playlist._generateScrollData();
            }
        },
        _onPlayerReady: function (c)
        {
            this.toolbar.setProgressRailMaxValue(c.duration);
            this.toolbar.setProgressRailValue(this.get_currentTime());
            this.toolbar.setDuration(this._getTimeString(c.duration));
            this.toolbar._addProgressBuffer();
            this.toolbar._addProgressTooltip();
            this._raiseControlEvent(this, "ready", {});
        },
        _onBytesLoading: function (c)
        {
            this.toolbar.setBytesLoaded(c);
        },
        _onUpdateProgress: function (c)
        {
            if (typeof (c)!="undefined")
            {
                this.toolbar.setProgressRailValue(c);
                var d=this._getTimeString(c);
                this._updateBanners(c);
                this._updateScript(c);
                this.toolbar.setTimeValue(d);
                this.updateClientState();
            }
        },
        _initializeBanners: function ()
        {
            this._bannerWrapper=$telerik.getChildByClassName(this.get_element(), "rmpBanner");
            if (this._banners==null||this._banners.length<1||this._bannerWrapper==null)
            {
                return;
            }
            this._jbannerCloseButton=a(this._bannerWrapper.children[0]);
            this._jbannerCloseButton.on("click", a.proxy(this._onBannerCloseButtonClick, this));
        },
        _updateBanners: function (e)
        {
            if (this._banners==null||this._banners.length<1)
            {
                return;
            }
            var d=0;
            var c=this._lastActiveBanner||this._banners[d++];
            while (c.startTime>e||c.endTime<=e||c.closed)
            {
                if (d>=this._banners.length)
                {
                    c=null;
                    break;
                }
                c=this._banners[d];
                d++;
            }
            if (c==this._lastActiveBanner)
            {
                return;
            }
            this._showBanner(c);
        },
        _updateScript: function (g, e)
        {
            if (this._reloadSubtitles)
            {
                var c=this.currentFile.subtitlesData;
                if (c)
                {
                    this._mediaPlayerSubtitles=new b.MediaPlayerSubtitles(this, c);
                } else
                {
                    this._mediaPlayerSubtitles=null;
                    if (this._scriptInner)
                    {
                        this._scriptInner.style.display="none";
                    }
                }
                if (this._mediaPlayerSubtitles&&!this._ccActive)
                {
                    this._ccActive=this.toolbar.toggleSubtitlesButtonState();
                }
                this._reloadSubtitles=false;
                e=true;
            }
            if (!this._ccActive)
            {
                this._ccActiveTime=g;
                return;
            } else
            {
                if (e)
                {
                    g=this._ccActiveTime;
                }
            }
            if (this._mediaPlayerSubtitles)
            {
                if (!this._scriptInner)
                {
                    this._scriptOuter=$telerik.getChildByClassName(this.get_element(), "rmpSubtitles");
                    this._scriptInner=this._scriptOuter.children[0];
                }
                if (this._mediaPlayerSubtitles.seek(g)||e)
                {
                    var f=this._mediaPlayerSubtitles.getSubtitle();
                    this._scriptInner.innerHTML=f;
                    var d=f? "":"none";
                    if (this._scriptInner.style.display!==d)
                    {
                        this._scriptInner.style.display=d;
                    }
                }
            } else
            {
                if (e&&this._scriptInner&&this._ccActive)
                {
                    this._ccActive=this.toolbar.toggleSubtitlesButtonState();
                    this._scriptInner.style.display="none";
                }
            }
        },
        _showBanner: function (d)
        {
            this._lastActiveBanner=d;
            var e=this._bannerWrapper;
            if (d)
            {
                var f=e.children[0];
                var c=e.children[1];
                var g=c.children[0];
                c.href=d.navigateUrl;
                c.title=d.toolTip;
                c.target=d.target;
                g.src=d.imageUrl;
                g.alt=d.alternateText;
                g.title=d.toolTip;
                f.style.display=d.showCloseButton? "":"none";
                e.style.display="";
            } else
            {
                e.style.display="none";
            }
        },
        _adjustBannerPosition: function ()
        {
            if (this.toolbar)
            {
                var c=this.toolbar._jwrapper.height()||0;
                if (this._bannerWrapper)
                {
                    this._bannerWrapper.style.bottom=c+20+"px";
                }
                if (this._scriptOuter)
                {
                    this._scriptOuter.style.bottom=c+20+"px";
                }
            }
        },
        _onBannerCloseButtonClick: function ()
        {
            this._lastActiveBanner.closed=true;
            this._showBanner(null);
        },
        _onPlayerPlay: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
            this.toolbar.setPlayButtonState(true);
            if (this.currentPlayer.get_playerType()==b.MediaPlayerType.HTML5||this.currentPlayer.get_playerType()==b.MediaPlayerType.Flash)
            {
                this.toolbar.setCenterPlayButtonState(true);
            }
            this.updateClientState();
            this._raiseControlEvent(this, "play", {});
        },
        _onPlayerPause: function ()
        {
            this.toolbar.setPlayButtonState(false);
            if (this.currentPlayer.get_playerType()==b.MediaPlayerType.HTML5||this.currentPlayer.get_playerType()==b.MediaPlayerType.Flash)
            {
                this.toolbar.setCenterPlayButtonState(false);
            }
            this.updateClientState();
            this._raiseControlEvent(this, "paused", {});
        },
        _onPlayerEnded: function ()
        {
            this._raiseControlEvent(this, "ended", {});
        },
        _onPlayerWaiting: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
            this.get_loadingIndicator().show(this.get_id());
        },
        _onPlayerSeeking: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
            this.get_loadingIndicator().show(this.get_id());
        },
        _onPlayerSeeked: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
        },
        _onPlayerLoadedData: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
            this.get_loadingIndicator().show(this.get_id());
            this.set_currentTime(this.currentFile.options.startTime);
        },
        _onPlayerCanPlay: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
        },
        _onPlayerError: function (c)
        {
            this.get_loadingIndicator().hide(this.get_id());
        },
        _onPlayerReadyToStart: function ()
        {
            this.get_loadingIndicator().hide(this.get_id());
        },
        _onPlayButtonToggle: function ()
        {
            var c=this.toolbar.togglePlayButtonState();
            if (this.currentPlayer.get_playerType()==b.MediaPlayerType.HTML5||this.currentPlayer.get_playerType()==b.MediaPlayerType.Flash)
            {
                this.toolbar.toggleCenterPlayButtonState();
            }
            if (c)
            {
                this.play();
            } else
            {
                this.pause();
            }
        },
        _onVolumeControlToggle: function ()
        {
            this.toolbar.toggleVolumeControlVisibility();
        },
        _onVolumeControlClick: function ()
        {
            var c=this.toolbar.toggleVolumeButtonMutedState();
            if (c)
            {
                this.mute();
            } else
            {
                this.unmute();
            }
        },
        _onFullScreenToggle: function ()
        {
            this.toggleFullScreen();
        },
        _onFullScreenChange: function (c)
        {
            var d=document.fullScreen||document.mozFullScreen||document.webkitIsFullScreen;
            if (!d)
            {
                this.exitFullScreen();
            }
        },
        _onKeyDown: function (c)
        {
            var d=c.keyCode||c.keyCode;
            if (d==27)
            {
                this.exitFullScreen();
            }
        },
        _onmousemove: function (c)
        {
            this.showControls();
        },
        _mouseidle: function ()
        {
            this.titlebar.slideUp();
            if (!this.get_toolbarDocked())
            {
                this.toolbar.hide(1000);
            }
            if (this.currentPlayer.get_playerType()==b.MediaPlayerType.YouTube)
            {
                this._joverlay[0].style.display="";
            }
            return false;
        },
        _onHDToggled: function ()
        {
            var c=this.toolbar.toggleHDButtonState();
            this.currentPlayer.toggleHD(c);
        },
        _onCCToggled: function ()
        {
            if (this._mediaPlayerSubtitles)
            {
                this._ccActive=this.toolbar.toggleSubtitlesButtonState();
                if (!this._ccActive)
                {
                    if (this._scriptInner)
                    {
                        this._scriptInner.style.display="none";
                    }
                } else
                {
                    this._updateScript(0, true);
                }
            }
        },
        _onSeekStart: function ()
        {
            this.currentPlayer.startSeek();
        },
        _onSeekEnded: function (c)
        {
            this.seekTo(c);
        },
        _onVolumeChanged: function (c)
        {
            this.set_volume(c);
            this._raiseControlEvent(this, "volumeChanged", {
                volume: c
            });
        },
        _onProgressRailMouseMove: function (c, d)
        {
            this.toolbar.setPosOnProgressTooltip(null, c-28);
            this.toolbar.setTimeValueOnTooltip(this._getTimeString(d));
        },
        _onToolbarMouseMove: function ()
        {
            this.showControls();
        },
        _onTitleBarMouseMove: function ()
        {
            this.showControls();
        },
        _raiseControlEvent: function (d, e, c)
        {
            a.raiseControlEvent(d, e, c);
        },
        _raiseCancellableControlEvent: function (d, e, c)
        {
            return a.raiseCancellableControlEvent(d, e, c);
        },
        _isCanvasSupported: function ()
        {
            var c=document.createElement("canvas");
            return !!(c.getContext&&c.getContext("2d"));
        },
        _capture: function (g, f)
        {
            if (f==null)
            {
                f=1;
            }
            var i=g.videoWidth*f;
            var e=g.videoHeight*f;
            var c=document.createElement("canvas");
            c.width=i;
            c.height=e;
            var d=c.getContext("2d");
            d.drawImage(g, 0, 0, i, e);
            return c;
        },
        _getTimeString: function (e)
        {
            var f="00:00";
            if (e<0)
            {
                e=0;
            }
            e=parseInt(e, 10);
            var c=parseInt(e/3600, 10);
            e%=3600;
            var d=parseInt(e/60, 10);
            e=e%60;
            if (!c)
            {
                f=(d<10? "0"+d:d)+":"+(e<10? "0"+e:e);
            } else
            {
                f=(c<10? "0"+c:c)+":"+(d<10? "0"+d:d)+":"+(e<10? "0"+e:e);
            }
            return f;
        },
        get__mediaFilesData: function ()
        {
            return this._mediaFilesData;
        },
        set__mediaFilesData: function (c)
        {
            this._mediaFilesData=c;
        },
        get__banners: function ()
        {
            return this._banners;
        },
        set__banners: function (c)
        {
            this._banners=c;
        },
        get_uniqueID: function ()
        {
            return this.options.uniqueID;
        },
        saveClientState: function ()
        {
            var c={};
            c.currentTime=this.currentPlayer.get_currentTime();
            c.volume=this.currentPlayer.get_volume();
            c.playing=this.toolbar._jplayButton[0].children[0].className=="rmpIcon rmpPauseIcon";
            c.fsActive=this.get_fsActive();
            c.muted=this.toolbar._jvcButton[0].className=="rmpActionButton rmpMuteButton";
            c.selectedIndex=this.options.selectedIndex;
            return Sys.Serialization.JavaScriptSerializer.serialize(c);
        }
    };
    a.registerControlEvents(b.RadMediaPlayer, ["play", "ready", "seekStart", "paused", "ended", "volumeChanged"]);
    b.RadMediaPlayer.registerClass("Telerik.Web.UI.RadMediaPlayer", b.RadWebControl);
    a.registerEnum(b, "MediaPlayerType", {
        HTML5: 1,
        YouTube: 2,
        Flash: 3
    });
}
)(Telerik.Web.UI, $telerik.$);
Type.registerNamespace("Telerik.Web.UI.MediaPlayerObjects");
(function (a)
{
    a.MediaPlayerObjects.MediaFile=function (b)
    {
        this.mimeType=null;
        this.mediaType=null;
        this.extension=null;
        this.initialize(b);
    }
        ;
    a.MediaPlayerObjects.MediaFile.prototype={
        initialize: function (c)
        {
            this.options={};
            this.options.startVolume=c.startVolume;
            this.options.startTime=c.startTime;
            this.options.autoPlay=c.autoPlay;
            this.options.sources=c.sources;
            this.options.poster=c.poster;
            this.options.duration=c.duration;
            this.path=c.path;
            this.extension=this.get_fileExtension();
            this.mimeType=c.mimeType||this.get_mimeType();
            this.type=this.get_type();
            this.subtitlesData=c.subtitlesData;
            var b=this.options.sources.length;
            while (b--)
            {
                var d=this.options.sources[b];
                d.extension=this.getExtensionFromPath(d.path);
                d.mimeType=d.mimeType? d.mimeType:this.getMimeTypeFromExtension(d.extension);
            }
        },
        get_mimeType: function ()
        {
            if (!this.mimeType)
            {
                this.mimeType=this.getMimeTypeFromExtension(this.extension);
            }
            return this.mimeType;
        },
        getMimeTypeFromExtension: function (b)
        {
            switch (b)
            {
                case "mp4":
                case "m4v":
                case "f4v":
                    return "video/mp4";
                case "flv":
                    return "video/x-flv";
                case "m3u8":
                    return "application/vnd.apple.mpegurl";
                case "webm":
                    return "video/webm";
                case "ogg":
                case "ogv":
                    return "video/ogg";
                case "3g2":
                    return "video/3gpp2";
                case "3gpp":
                case "3gp":
                    return "video/3gpp";
                case "mov":
                    return "video/quicktime";
                case "swf":
                    return "application/x-shockwave-flash";
                case "oga":
                    return "audio/ogg";
                case "mp3":
                    return "audio/mpeg";
                case "m4a":
                case "f4a":
                    return "audio/mp4";
                case "aac":
                    return "audio/aac";
                case "wav":
                    return "audio/wav";
                case "wma":
                    return "audio/x-ms-wma";
                default:
                    return "unknown";
            }
        },
        getExtensionFromPath: function (c)
        {
            var d=c.indexOf("?");
            if (d>-1)
            {
                var b=c.substring(0, d);
                return b.substring(b.lastIndexOf(".")+1).toLowerCase();
            } else
            {
                return c.substring(c.lastIndexOf(".")+1).toLowerCase();
            }
        },
        get_fileExtension: function ()
        {
            var f=this
                , d=f.path
                , b=f.extension;
            if (!b&&d)
            {
                var e=d.indexOf("?");
                if (e>-1)
                {
                    var c=d.substring(0, e);
                    return c.substring(c.lastIndexOf(".")+1).toLowerCase();
                } else
                {
                    b=d.substring(d.lastIndexOf(".")+1).toLowerCase();
                }
            }
            return b;
        },
        get_type: function ()
        {
            if (!this.type)
            {
                var b=this.mimeType.match(/([^\/]+)(\/)/);
                b=(b&&(b.length>1))? b[1]:"";
                if (b==="video")
                {
                    return "video";
                }
                if (b==="audio")
                {
                    return "audio";
                }
                switch (this.mimeType)
                {
                    case "application/octet-stream":
                    case "application/x-shockwave-flash":
                    case "application/vnd.apple.mpegurl":
                        return "video";
                }
                return "";
            }
            return this.type;
        }
    };
    a.MediaPlayerObjects.MediaFile.registerClass("Telerik.Web.UI.MediaPlayerObjects.MediaFile");
}
)(Telerik.Web.UI);
Type.registerNamespace("Telerik.Web.UI.MediaPlayerObjects");
(function ()
{
    var a=$telerik.$;
    var c=Telerik.Web.UI;
    var b="Mobile";
    c.MediaPlayerObjects.extendWithEvents=function (d)
    {
        (function ()
        {
            var f={};
            a.extend(d, {
                on: function (h, g)
                {
                    a.each(h, function (i, j)
                    {
                        e(i, j, g);
                    });
                },
                trigger: function ()
                {
                    var g=Array.prototype.slice.call(arguments)
                        , h=g.shift()
                        , j=f[h];
                    if (a.type(j)==="array")
                    {
                        for (var k=0; k<j.length; k++)
                        {
                            j[k].func.apply(j[k].context, g);
                        }
                    }
                },
                off: function ()
                {
                    for (var g in f)
                    {
                        delete f[g];
                    }
                }
            });
            function e(h, i, g)
            {
                var j=f[h]||[];
                j.push({
                    func: i,
                    context: g
                });
                f[h]=j;
            }
        }
        )();
    }
        ;
    c.MediaPlayerObjects.MediaPlayerToolbar=function (e, d)
    {
        this.owner=e;
        c.MediaPlayerObjects.extendWithEvents(this);
        this._jwrapper=null;
        this._progressBuffer=null;
        this._progressRail=null;
        this._progressTooltip=null;
        this._volumeControl=null;
        this._timeDisplay=null;
        this._durationDisplay=null;
        this._progressRailTrack=null;
        this._jprogressRailTrack=null;
        this._jplayButton=null;
        this._jfsButton=null;
        this._jvcButton=null;
        this._jhdButton=null;
        this._jccButton=null;
        this._jprogressBuffer=null;
        this._jprogressTooltip=null;
        this._jdragHandler=null;
        this._jplayButtonCenter=null;
        this.initialize(d);
    }
        ;
    c.MediaPlayerObjects.MediaPlayerToolbar.prototype={
        initialize: function (d)
        {
            var e=this.owner.get_id();
            this._jwrapper=a($get(e+"_Toolbar_Wrapper"));
            this._progressRail=$find(e+"_Toolbar_ProgressRail");
            this._volumeControl=$find(e+"_Toolbar_VolumeControl");
            this._timeDisplay=$get(e+"_Toolbar_CurrentTimeDisplay");
            this._durationDisplay=$get(e+"_Toolbar_DurationDisplay");
            this._jplayButton=a($get(e+"_Toolbar_PlayButton"));
            this._jfsButton=a($get(e+"_Toolbar_FSButton"));
            this._jvcButton=a($get(e+"_Toolbar_VolumeControlButton"));
            this._jhdButton=a($get(e+"_Toolbar_HDButton"));
            this._jccButton=a($get(e+"_Toolbar_SubtitlesButton"));
            this._jprogressRail=a(this._progressRail.get_element());
            this._jplayButtonCenter=a($get(e+"_Toolbar_PlayButtonCenter"));
            this._progressRail.add_slideEnd(a.proxy(this._progressDragEnd, this));
            this._progressRail.add_slideStart(a.proxy(this._progressDragStart, this));
            this._progressRail.add_valueChanging(a.proxy(this._progressValueChanging, this));
            this._progressRail.add_valueChanged(a.proxy(this._progressValueChanged, this));
            this._volumeControl.add_valueChanged(a.proxy(this._volumeControlValueChanged, this));
            this._jplayButton.on("click", a.proxy(this._onPlayButtonClick, this));
            this._jfsButton.on("click", a.proxy(this._onFSButtonClick, this));
            this._jhdButton.on("click", a.proxy(this._onHDButtonClick, this));
            this._jccButton.on("click", a.proxy(this._onCCButtonClick, this));
            if (this.owner._renderMode!=b)
            {
                this._jvcButton.on("click", a.proxy(this._onVolumeControlButtonClick, this));
                this._jvcButton.parent().on("mouseenter", a.proxy(this._onVolumeControlButtonHover, this));
                this._jvcButton.parent().on("mouseleave", a.proxy(this._onVolumeControlButtonBlur, this));
            } else
            {
                this._jvcButton.on("click", a.proxy(this._onVolumeButtonClick, this));
            }
            this._jplayButtonCenter.on("click", a.proxy(this._onPlayButtonClick, this));
            this._jwrapper.parent().on("mousemove", a.proxy(this._onToolbarMouseMove, this));
            this.setFullScreenButtonState(d.fsActive);
            this.setHDButtonState(d.hdActive);
            this.setPlayButtonState(d.autoPlay);
            if (d.playerType==c.MediaPlayerType.HTML5||d.playerType==c.MediaPlayerType.Flash)
            {
                this.setCenterPlayButtonState(d.autoPlay);
            } else
            {
                if (d.playerType==c.MediaPlayerType.YouTube)
                {
                    this._jplayButtonCenter.hide();
                }
            }
            if (d.startVolume!=-1)
            {
                this.setVolumeControlValue(d.startVolume);
            } else
            {
                this.setVolumeControlValue(this.owner.options.startVolume);
            }
            this.setVolumeButtonMutedState(d.muted);
        },
        dispose: function ()
        {
            this.off();
            this._jprogressRailTrack.off();
            this._jplayButton.off();
            this._jfsButton.off();
            this._jvcButton.off();
            this._jhdButton.off();
            this._jccButton.off();
            this._jprogressRail.off();
            this._jprogressBuffer.off();
            this._jdragHandler.off();
            this._volumeControl.remove_valueChanged(this._volumeControlValueChanged);
        },
        get_wrapper: function ()
        {
            return this._jwrapper[0];
        },
        hide: function (d)
        {
            this._jwrapper.hide({
                duration: d,
                progress: a.proxy(this.owner._adjustBannerPosition, this.owner)
            });
        },
        show: function (d)
        {
            this._jwrapper.show({
                duration: d,
                progress: a.proxy(this.owner._adjustBannerPosition, this.owner)
            });
        },
        setBytesLoaded: function (d)
        {
            if (this._progressBuffer!=null)
            {
                this._progressBuffer.style.width=d+"%";
            }
        },
        setVolumeControlValue: function (d)
        {
            this._volumeControl.set_value(d);
        },
        setTimeValue: function (d)
        {
            this._timeDisplay.innerHTML=d;
        },
        setTimeValueOnTooltip: function (d)
        {
            this._progressTooltip.innerHTML=d;
        },
        setPosOnProgressTooltip: function (e, d)
        {
            if (e!=null)
            {
                this._jprogressTooltip.css("top", e-this.toolbar._jprogressTooltip.width()/2);
            }
            if (d)
            {
                this._jprogressTooltip.css("left", d);
            }
        },
        setDuration: function (d)
        {
            this._durationDisplay.innerHTML=d;
        },
        setProgressRailMaxValue: function (d)
        {
            this._progressRail.set_maximumValue(d);
        },
        setPlayButtonState: function (f)
        {
            var g=this;
            var e=g.owner.options;
            var d=g._jplayButton[0];
            if (f)
            {
                d.className=d.className.replace("rmpPlayButton", "rmpPauseButton");
                d.title=e.pauseButtonToolTip||"Pause";
                d.children[0].className="rmpIcon rmpPauseIcon";
            } else
            {
                d.className=d.className.replace("rmpPauseButton", "rmpPlayButton");
                d.title=e.playButtonToolTip||"Play";
                d.children[0].className="rmpIcon rmpPlayIcon";
            }
        },
        setCenterPlayButtonState: function (d)
        {
            if (d)
            {
                this._jplayButtonCenter[0].className=this._jplayButtonCenter[0].className.replace("rmpBigPlayButton", "rmpBigPauseButton");
                this._jplayButtonCenter.children()[0].className="rmpIcon rmpPauseIcon";
                this._jplayButtonCenter.hide();
            } else
            {
                this._jplayButtonCenter[0].className=this._jplayButtonCenter[0].className.replace("rmpBigPauseButton", "rmpBigPlayButton");
                this._jplayButtonCenter.children()[0].className="rmpIcon rmpBigPlayIcon";
                this._jplayButtonCenter.show();
            }
        },
        setProgressRailValue: function (d)
        {
            this._progressRail.setValue(d, true);
        },
        togglePlayButtonState: function ()
        {
            var d;
            if (this._jplayButton[0].className.indexOf("rmpActionButton rmpPlayButton")!==-1)
            {
                this._jplayButton[0].className=this._jplayButton[0].className.replace("rmpPlayButton", "rmpPauseButton");
                this._jplayButton[0].children[0].className="rmpIcon rmpPauseIcon";
                d=1;
            } else
            {
                this._jplayButton[0].className=this._jplayButton[0].className.replace("rmpPauseButton", "rmpPlayButton");
                this._jplayButton[0].children[0].className="rmpIcon rmpPlayIcon";
                d=0;
            }
            return d;
        },
        toggleCenterPlayButtonState: function ()
        {
            var d;
            if (this._jplayButtonCenter.hasClass("rmpActionButton rmpBigPlayButton"))
            {
                this._jplayButtonCenter[0].className=this._jplayButtonCenter[0].className.replace("rmpBigPlayButton", "rmpBigPauseButton");
                this._jplayButtonCenter.children()[0].className="rmpIcon rmpPauseIcon";
                this._jplayButtonCenter[0].style.display="none";
                d=1;
            } else
            {
                this._jplayButtonCenter[0].className=this._jplayButtonCenter[0].className.replace("rmpBigPauseButton", "rmpBigPlayButton");
                this._jplayButtonCenter.children()[0].className="rmpIcon rmpBigPlayIcon";
                this._jplayButtonCenter[0].style.display="";
                d=1;
            }
            return d;
        },
        toggleFullScreenButtonState: function ()
        {
            if (!this._jfsButton.length)
            {
                return false;
            }
            var d=false;
            if (this._jfsButton[0].className.indexOf("rmpActionButton rmpFullScrButton")!==-1)
            {
                this._jfsButton[0].className=this._jfsButton[0].className.replace("rmpFullScrButton", "rmpExtFullScrButton");
                this._jfsButton[0].children[0].className="rmpIcon rmpExtFullScrIcon";
                d=true;
            } else
            {
                this._jfsButton[0].className=this._jfsButton[0].className.replace("rmpExtFullScrButton", "rmpFullScrButton");
                this._jfsButton[0].children[0].className="rmpIcon rmpFullScrIcon";
            }
            return d;
        },
        setFullScreenButtonState: function (d)
        {
            if (!this._jfsButton.length)
            {
                return;
            }
            if (d)
            {
                this._jfsButton[0].className=this._jfsButton[0].className.replace("rmpFullScrButton", "rmpExtFullScrButton");
                this._jfsButton[0].children[0].className="rmpIcon rmpExtFullScrIcon";
            } else
            {
                this._jfsButton[0].className=this._jfsButton[0].className.replace("rmpExtFullScrButton", "rmpFullScrButton");
                this._jfsButton[0].children[0].className="rmpIcon rmpFullScrIcon";
            }
        },
        toggleHDButtonState: function ()
        {
            if (!this._jhdButton.length)
            {
                return false;
            }
            var d=this._jhdButton[0].children[0];
            var e=false;
            if (d.className.indexOf("rmpHDIcon")==-1)
            {
                d.className=d.className.replace("rmpHDActiveIcon", "rmpHDIcon");
            } else
            {
                d.className=d.className.replace("rmpHDIcon", "rmpHDActiveIcon");
                e=true;
            }
            return e;
        },
        setHDButtonState: function (d)
        {
            if (!this._jhdButton.length)
            {
                return;
            }
            var e=this._jhdButton[0].children[0];
            if (!d)
            {
                e.className=e.className.replace("rmpHDActiveIcon", "rmpHDIcon");
            } else
            {
                e.className=e.className.replace("rmpHDIcon", "rmpHDActiveIcon");
            }
        },
        toggleSubtitlesButtonState: function ()
        {
            if (!this._jccButton.length)
            {
                return false;
            }
            var d=this._jccButton[0].children[0];
            var e=false;
            if (d.className.indexOf("rmpSubtitlesIcon")==-1)
            {
                d.className=d.className.replace("rmpSubtitlesActiveIcon", "rmpSubtitlesIcon");
            } else
            {
                d.className=d.className.replace("rmpSubtitlesIcon", "rmpSubtitlesActiveIcon");
                e=true;
            }
            return e;
        },
        toggleVolumeControlVisibility: function ()
        {
            if (this.owner._renderMode==b)
            {
                a(this._volumeControl.get_element()).parent().toggle();
            } else
            {
                var d=a(this._volumeControl.get_element()).parent();
                if (d[0].style.display=="")
                {
                    d[0].style.display="none";
                } else
                {
                    d[0].style.display="";
                }
            }
            if ($telerik.isChrome||($telerik.isIE&&document.documentMode&&document.documentMode==11))
            {
                this._volumeControl.repaint();
            }
        },
        toggleVolumeButtonMutedState: function ()
        {
            var d=false;
            if (this._jvcButton[0].className.indexOf("rmpActionButton rmpVolumeButton")!==-1)
            {
                this._jvcButton[0].className=this._jvcButton[0].className.replace("rmpVolumeButton", "rmpMuteButton");
                this._jvcButton[0].children[0].className="rmpIcon rmpMuteIcon";
                d=true;
            } else
            {
                this._jvcButton[0].className=this._jvcButton[0].className.replace("rmpMuteButton", "rmpVolumeButton");
                this._jvcButton[0].children[0].className="rmpIcon rmpVolumeIcon";
            }
            return d;
        },
        setVolumeButtonMutedState: function (d)
        {
            if (d)
            {
                this._jvcButton[0].className=this._jvcButton[0].className.replace("rmpVolumeButton", "rmpMuteButton");
                this._jvcButton[0].children[0].className="rmpIcon rmpMuteIcon";
            } else
            {
                this._jvcButton[0].className=this._jvcButton[0].className.replace("rmpMuteButton", "rmpVolumeButton");
                this._jvcButton[0].children[0].className="rmpIcon rmpVolumeIcon";
            }
        },
        _addProgressBuffer: function ()
        {
            this._progressBuffer=this._progressBuffer||document.createElement("div");
            this._progressBuffer.className="rmpLoadProgressBar";
            this._progressBuffer.style.width="0px";
            this._progressRailTrack=$get("RadSliderTrack_"+this._progressRail.get_id());
            this._jprogressRailTrack=a(this._progressRailTrack);
            this._progressRailTrack.insertBefore(this._progressBuffer, $get("RadSliderSelected_"+this._progressRail.get_id()));
            this._jprogressBuffer=a(this._progressBuffer);
            this._jprogressBuffer.on({
                click: a.proxy(this._progressBufferClick, this),
                mousemove: a.proxy(this._progressBufferMouseMove, this),
                mouseenter: a.proxy(this._progressBufferMouseEnter, this)
            });
        },
        _addProgressTooltip: function ()
        {
            this._progressTooltip=this._progressTooltip||document.createElement("div");
            this._progressTooltip.className="rmpToolTip";
            this._jprogressTooltip=a(this._progressTooltip);
            this._jprogressTooltip.hide();
            var d=$get("RadSliderDrag_"+this._progressRail.get_id());
            this._progressRailTrack=$get("RadSliderTrack_"+this._progressRail.get_id());
            this._progressRailTrack.insertBefore(this._progressTooltip, d);
            this._jdragHandler=a(d);
            a(this._progressRailTrack).on({
                mousemove: a.proxy(this._progressBufferMouseMove, this),
                mouseenter: a.proxy(this._progressBufferMouseEnter, this),
                mouseleave: a.proxy(this._progressBufferMouseOut, this)
            });
            this._jdragHandler.on({
                mousemove: a.proxy(this._progressBufferMouseMove, this),
                mouseenter: a.proxy(this._progressBufferMouseEnter, this)
            });
        },
        _progressBufferClick: function (d)
        {
            var g=(d.pageX-a(d.target).offset().left)/this._progressRail.get_element().clientWidth;
            var f=this._progressRail.get_maximumValue();
            this._progressRail.set_value(g*f);
        },
        _progressBufferMouseMove: function (d)
        {
            var f=d.pageX-this._jprogressRailTrack.offset().left;
            var h=f/this._progressRail.get_element().clientWidth;
            var g=this._progressRail.get_maximumValue();
            this.trigger("progressRailMouseMove", f, h*g);
        },
        showTimeTooltip: function ()
        {
            this._jprogressTooltip.show();
        },
        _progressBufferMouseEnter: function (d)
        {
            this._jprogressTooltip.show();
        },
        _progressBufferMouseOut: function (d)
        {
            this._jprogressTooltip.hide();
        },
        _volumeControlValueChanged: function (g, d)
        {
            var f=this
                , e=f.owner;
            f.trigger("volumeChanged", g.get_value());
            if (e._renderMode==b)
            {
                if (d.get_newValue()==0)
                {
                    e.mute();
                    this.setVolumeButtonMutedState(true);
                } else
                {
                    if (e.isMuted())
                    {
                        e.unmute();
                        this.setVolumeButtonMutedState(false);
                    }
                }
            }
        },
        _progressDragStart: function (e, d)
        {
            this.trigger("seekStart");
        },
        _progressDragEnd: function (e, d)
        {
            this.trigger("seekEnd", e.get_value());
        },
        _progressValueChanging: function (f, d)
        {
            var e=this.owner._raiseCancellableControlEvent(this.owner, "seekStart", d);
            if (!e)
            {
                this.trigger("seekStart");
            } else
            {
                d.set_cancel(true);
            }
        },
        _progressValueChanged: function (e, d)
        {
            this.trigger("seekEnd", e.get_value());
        },
        _onPlayButtonClick: function ()
        {
            this.trigger("playToggle");
        },
        _onToolbarMouseMove: function ()
        {
            this.trigger("toolBarMouseMove");
        },
        _onVolumeControlButtonClick: function ()
        {
            this.trigger("volumeControlClick");
        },
        _onVolumeControlButtonHover: function ()
        {
            this.trigger("volumeControlToggle");
            this._jvcButton.parent().addClass("rmpVolContrHover");
        },
        _onVolumeButtonClick: function ()
        {
            this.trigger("volumeControlToggle");
        },
        _onVolumeControlButtonBlur: function ()
        {
            this.trigger("volumeControlToggle");
            this._jvcButton.parent().removeClass("rmpVolContrHover");
        },
        _onFSButtonClick: function ()
        {
            this.trigger("fullScreenToggle");
        },
        _onHDButtonClick: function ()
        {
            this.trigger("hdToggle");
        },
        _onCCButtonClick: function ()
        {
            this.trigger("ccToggle");
        }
    };
    c.MediaPlayerObjects.MediaPlayerToolbar.registerClass("Telerik.Web.UI.MediaPlayerObjects.MediaPlayerToolbar");
}
)();
Type.registerNamespace("Telerik.Web.UI.MediaPlayerObjects");
(function ()
{
    var a=$telerik.$;
    var c=Telerik.Web.UI;
    var b="Mobile";
    c.MediaPlayerObjects.MediaPlayerTitlebar=function (d)
    {
        this.owner=d;
        c.MediaPlayerObjects.extendWithEvents(this);
        this._$element=null;
        this._jsocialButton=null;
        this._$playlistButton=null;
        this._socialShare=null;
        this._socialShareContainer=null;
        this.initialize();
    }
        ;
    c.MediaPlayerObjects.MediaPlayerTitlebar.prototype={
        initialize: function ()
        {
            var f=this
                , e=this.owner.get_id()
                , d=this._$element=a($get(e+"_Titlebar_Wrapper"));
            this._jsocialButton=a($get(e+"_Titlebar_SocialButton"));
            this._socialShare=$find(e+"_Titlebar_SocialShare");
            this._$playlistButton=d.on("mousemove", a.proxy(this._onMouseMove, this)).find(".rmpOpenPlaylistButton, .rmpClosePlaylistButton").on("click", function (g)
            {
                f._$playlistButton.add(f.owner.playlist.get_element()).toggle(0);
                if (f.owner.options.playlistPosition.indexOf("Vertical")!=-1)
                {
                    f.owner.playlist._data.scrollSize=f.owner.playlist._$list.height();
                } else
                {
                    f.owner.playlist._data.scrollSize=f.owner.playlist._$list.width();
                }
                g.preventDefault();
            });
            if (f.owner.options._enableAriaSupport)
            {
                f._jsocialButton.attr("title", "Social share");
            }
            this._jsocialButton.on("click", a.proxy(this._onSocialButtonClick, this));
            if (this.owner._renderMode==b)
            {
                a(this.get_socialShareContainer().parentNode).on("click", a.proxy(this._onSocialModalContainerClick, this));
                this._socialShare.add_clicking(a.proxy(this._socialShareClicking, this));
            }
        },
        dispose: function ()
        {
            this._jsocialButton.off();
            if (this.owner._renderMode==b)
            {
                a(this.get_socialShareContainer().parentNode).off();
                this._socialShare.remove_clicking(this._socialShareClicking);
            }
        },
        _socialShareClicking: function (e, d)
        {
            if (d.get_socialNetType()=="GoogleBookmarks")
            {
                d.set_cancel(true);
                window.open("https://plus.google.com/share?url_="+d.get_url());
            }
        },
        _onSocialButtonClick: function ()
        {
            this.toggleSocialShareVisibility();
        },
        _onSocialModalContainerClick: function ()
        {
            var d=this.get_socialShareContainer();
            d.style.display="none";
            a(d.parentNode).hide();
        },
        _onMouseMove: function ()
        {
            this.trigger("titleBarMouseMove");
        },
        get_wrapper: function ()
        {
            return this._$element[0];
        },
        get_socialShareContainer: function ()
        {
            if (this._socialShareContainer==null)
            {
                this._socialShareContainer=this._socialShare.get_element().parentElement;
            }
            return this._socialShareContainer;
        },
        toggleSocialShareVisibility: function ()
        {
            var d=this.get_socialShareContainer();
            if (this.owner._renderMode!=b)
            {
                if (d.style.display=="none")
                {
                    d.style.display="";
                } else
                {
                    d.style.display="none";
                }
            } else
            {
                d.style.display="";
                a(d.parentNode).show();
            }
        },
        slideUp: function ()
        {
            this._$element.slideUp();
        },
        slideDown: function ()
        {
            this._$element.slideDown();
        }
    };
    c.MediaPlayerObjects.MediaPlayerTitlebar.registerClass("Telerik.Web.UI.MediaPlayerObjects.MediaPlayerTitlebar");
}
)();
Type.registerNamespace("Telerik.Web.UI");
(function (l, a, m)
{
    var k=10
        , b="rmpActive"
        , c="click"
        , f="mousedown"
        , i="mouseup"
        , g="mouseenter"
        , h="mouseleave"
        , e="hover"
        , j="rmpPlaylist"
        , o="rmpPlaylistVerticalInside"
        , n="rmpPlaylistVertical"
        , d="rmpPlaylistHorizontal";
    l.MediaPlayerObjects.MediaPlayerPlaylist=function (p)
    {
        l.MediaPlayerObjects.extendWithEvents(this);
        l.MediaPlayerObjects.MediaPlayerPlaylist.initializeBase(this, [p]);
        this._$element=a(p);
        this._$list=this._$element.find("ul");
        this._$listItems=this._$element.find("ul > li");
        this._owner=null;
        this._playListTouchScroll=null;
    }
        ;
    l.MediaPlayerObjects.MediaPlayerPlaylist.prototype={
        initialize: function ()
        {
            l.MediaPlayerObjects.MediaPlayerPlaylist.callBaseMethod(this, "initialize");
            var p=this;
            p._setupScrollButtons(p._owner.options.playlistButtonsTrigger);
            p._$listItems.each(function (q)
            {
                a(this).on("click", {
                    index: q
                }, a.proxy(p._playListItemClicked, p));
            });
            p._generateScrollData();
            if (Telerik.Web.UI.TouchScrollExtender&&Telerik.Web.UI.TouchScrollExtender._getNeedsScrollExtender()&&!this._playListTouchScroll)
            {
                this._createTouchScrollExtender();
            }
        },
        _createTouchScrollExtender: function ()
        {
            var q=this
                , p=q._$element.get(0);
            if (p)
            {
                q._playListTouchScroll=new Telerik.Web.UI.TouchScrollExtender(p);
                q._playListTouchScroll.initialize();
            }
        },
        dispose: function ()
        {
            l.MediaPlayerObjects.MediaPlayerPlaylist.callBaseMethod(this, "dispose");
            if (this._playListTouchScroll)
            {
                this._playListTouchScroll.dispose();
                this._playListTouchScroll=null;
            }
        },
        set__owner: function (p)
        {
            this._owner=p;
        },
        updateCurrentActiveItem: function ()
        {
            var p=this._owner.options.selectedIndex;
            this._$list.children().removeClass(b).eq(p).addClass(b);
        },
        _generateScrollData: function ()
        {
            var t=this, p=t._$element, q=t._$list, s=t._owner.options.playlistPosition.indexOf("Horizontal")!=-1, r;
            if (s&&!t._owner.get_fsActive())
            {
                r={
                    size: p.width(),
                    scrollSize: q.width(),
                    padding: parseFloat(q.css("paddingLeft")),
                    marginProp: "marginLeft"
                };
                if (this._owner._renderMode!="Mobile")
                {
                    p.css("marginBottom", -p.height()).parent().css("marginBottom", p.height());
                    if ($telerik.isIE7&&r.scrollSize>r.size)
                    {
                        q.css("marginBottom", 17);
                        p.css("marginBottom", -p.height());
                    }
                }
            } else
            {
                r={
                    size: p.height(),
                    scrollSize: q.height(),
                    padding: parseFloat(q.css("paddingTop")),
                    marginProp: "top"
                };
                p.css("marginBottom", 0).parent().css("marginBottom", 0);
            }
            if (t._data&&r.size-(r.padding*2)>r.scrollSize)
            {
                t._move(Number.MAX_VALUE);
            }
            t._data=r;
        },
        _playListItemClicked: function (p)
        {
            this.trigger("playListItemClick", {
                index: p.data.index
            });
            p.preventDefault();
        },
        _setupScrollButtons: function (w)
        {
            if (!w)
            {
                return;
            }
            var v=this
                , u=v._owner
                , r=v._$element
                , s=r.find(".rmpPlaylistPrevButton")
                , t=r.find(".rmpPlaylistNextButton")
                , p=s.add(t)
                , q=a(document);
            w=w.toLowerCase();
            switch (w)
            {
                case f:
                    v._setupScrollButton(s, f, k, q, i);
                    v._setupScrollButton(t, f, -k, q, i);
                    break;
                case e:
                    v._setupScrollButton(s, g, k, s, h);
                    v._setupScrollButton(t, g, -k, t, h);
                    break;
            }
            p.on(u._getEventName(c), function (z)
            {
                if (w==c)
                {
                    var y=a(z.currentTarget).is(s)? 1:-1
                        , x=v._data;
                    v._move((x.size-x.padding*2)*y, true);
                }
                z.preventDefault();
            });
        },
        _setupScrollButton: function (p, r, t, q, u)
        {
            var v=this
                , s=v._owner;
            p.on(v._owner._getEventName(r), function ()
            {
                v._scroll(t);
                q.on(v._owner._getEventName(u), function ()
                {
                    v._stopScroll();
                    q.off(s._getEventName(u));
                });
            });
        },
        _scroll: function (p)
        {
            var q=this;
            if (q._scrollInterval)
            {
                return;
            }
            clearInterval(q._scrollInterval);
            q._scrollInterval=setInterval(function ()
            {
                q._move(p);
            }, 30);
        },
        _stopScroll: function ()
        {
            clearInterval(this._scrollInterval);
            this._scrollInterval=null;
        },
        _move: function (y, q)
        {
            var p=this._$list
                , s=this._data
                , u=s.marginProp
                , r={}
                , t=parseInt(p.css(u), 10)+y
                , x=q? 400:0
                , v=(s.scrollSize? s.scrollSize:p.height())-s.size+s.padding*2
                , w=this._owner.options.playlistPosition;
            if (isNaN(t))
            {
                t=y;
            }
            if (w==="Horizontal"&&!this._owner.get_fsActive())
            {
                if ((p.width()<s.size-v*2))
                {
                    return;
                }
            } else
            {
                if ((p.height()<s.size-v*2))
                {
                    return;
                }
            }
            if (t>=0)
            {
                r[u]=0;
                p.animate(r, x);
            } else
            {
                if (-t>v)
                {
                    r[u]=-v;
                    p.animate(r, x);
                } else
                {
                    r[u]=t;
                    p.animate(r, x);
                }
            }
        },
        _enterFullScreen: function ()
        {
            var p=this._$element;
            if (!p.hasClass(o))
            {
                p.removeClass(n).removeClass(d).addClass(o);
            }
            this._generateScrollData();
        },
        _exitFullScreen: function ()
        {
            this._$element.removeClass(o).addClass(j+this._owner.options.playlistPosition);
            this._generateScrollData();
        }
    };
    l.MediaPlayerObjects.MediaPlayerPlaylist.registerClass("Telerik.Web.UI.MediaPlayerObjects.MediaPlayerPlaylist", Sys.UI.Control);
}
)(Telerik.Web.UI, $telerik.$);
(function (a, e)
{
    Type.registerNamespace("Telerik.Web.UI");
    var b=Telerik.Web.UI;
    b.MediaPlayerSubtitles=function (l, k)
    {
        var j=[];
        var g=[];
        var h="";
        var i=0;
        var f=0;
        var n=function (o, p)
        {
            return o.start>p.start? 1:(o.start<p.start? -1:0);
        };
        j=(function (p)
        {
            var u=[];
            var o;
            var t;
            var s;
            var r=["SRT", "SBV", "TTXT", "TTML", "VTT", "SSA", "XML", "JSON"];
            p=m(p);
            for (s=0; s<r.length; s++)
            {
                t="Telerik.Web.UI.MediaPlayerSubtitles.parse"+r[s];
                try
                {
                    o=eval(t)(p);
                    if (o.length>u.length)
                    {
                        u=o;
                    }
                    if (u.length>1)
                    {
                        break;
                    }
                } catch (q) { }
            }
            if (u.length>0)
            {
                for (s=0; s<u.length; s++)
                {
                    u[s]=u[s].subtitle;
                }
                u.sort(n);
            }
            return u;
        }
        )(k);
        function m(r)
        {
            var o={
                xml: null,
                text: r,
                json: null
            };
            var q=null;
            var s=null;
            try
            {
                q=a.parseJSON(r);
                if (q)
                {
                    o.json=q;
                }
            } catch (p) { }
            try
            {
                s=a.parseXML(r);
                if (s)
                {
                    o.xml=s;
                }
            } catch (p) { }
            return o;
        }
        this.owner=l;
        this.getSubtitle=function ()
        {
            return h;
        }
            ;
        this.seek=function (r)
        {
            var p=f;
            var q=g.length;
            var o;
            if (r<i)
            {
                g=[];
                f=0;
            }
            while (f<j.length&&j[f].start<=r)
            {
                if (r<j[f].end)
                {
                    g.push(j[f]);
                }
                f++;
            }
            for (o=g.length-1; o>=0; o--)
            {
                if (r>=g[o].end)
                {
                    g.splice(o, 1);
                }
            }
            i=r;
            if (p===f&&q===g.length)
            {
                return false;
            }
            if (g.length===0)
            {
                h="";
            } else
            {
                h=g[0].text;
            }
            for (o=1; o<g.length; o++)
            {
                h=g[o].text+"<br/>"+h;
            }
            return true;
        }
            ;
    }
        ;
    var d=function (j, g, f)
    {
        if (!j||!g)
        {
            return {};
        }
        f=f||this;
        var h, i;
        if (d&&j.forEach===d)
        {
            return j.forEach(g, f);
        }
        if (Object.prototype.toString.call(j)==="[object NodeList]")
        {
            for (h=0,
                i=j.length; h<i; h++)
            {
                g.call(f, j[h], h, j);
            }
            return j;
        }
        for (h in j)
        {
            if (Object.prototype.hasOwnProperty.call(j, h))
            {
                g.call(f, j[h], h, j);
            }
        }
        return j;
    };
    var c=function (g)
    {
        var f=g
            , h=Array.prototype.slice.call(arguments, 1);
        d(h, function (i)
        {
            for (var j in i)
            {
                f[j]=i[j];
            }
        });
        return f;
    };
    b.MediaPlayerSubtitles.parseSRT=function (g)
    {
        function f(u, i)
        {
            var v={};
            v[u]=i;
            return v;
        }
        function t(w)
        {
            var v=w.split(":");
            try
            {
                var u=v[2].split(",");
                if (u.length===1)
                {
                    u=v[2].split(".");
                }
                return parseFloat(v[0], 10)*3600+parseFloat(v[1], 10)*60+parseFloat(u[0], 10)+parseFloat(u[1], 10)/1000;
            } catch (i)
            {
                return 0;
            }
        }
        function o(u, v)
        {
            var i=v;
            while (!u[i])
            {
                i++;
            }
            return i;
        }
        function m(u)
        {
            var i=u.length-1;
            while (i>=0&&!u[i])
            {
                i--;
            }
            return i;
        }
        var h=g.text;
        var q=[], k=0, l=0, n, s, r, j, p;
        if ($telerik.isIE)
        {
            n=h.replace(/\r\n/gm, "\n").split("\n");
        } else
        {
            n=h.split(/(?:\r\n|\r|\n)/gm);
        }
        j=m(n)+1;
        for (k=0; k<j; k++)
        {
            p={};
            r=[];
            k=o(n, k);
            p.id=parseInt(n[k++], 10);
            s=n[k++].split(/[\t ]*-->[\t ]*/);
            p.start=t(s[0]);
            l=s[1].indexOf(" ");
            if (l!==-1)
            {
                s[1]=s[1].substr(0, l);
            }
            p.end=t(s[1]);
            while (k<j&&n[k])
            {
                r.push(n[k++]);
            }
            p.text=r.join("\\N").replace(/\{(\\[\w]+\(?([\w\d]+,?)+\)?)+\}/gi, "");
            p.text=p.text.replace(/</g, "&lt;").replace(/>/g, "&gt;");
            p.text=p.text.replace(/&lt;(\/?(font|b|u|i|s))((\s+(\w|\w[\w\-]*\w)(\s*=\s*(?:\".*?\"|'.*?'|[^'\">\s]+))?)+\s*|\s*)(\/?)&gt;/gi, "<$1$3$7>");
            p.text=p.text.replace(/\\N/gi, "<br />");
            q.push(f("subtitle", p));
        }
        return q;
    }
        ;
    b.MediaPlayerSubtitles.parseXML=function (g)
    {
        var h=g.xml;
        var n=[];
        var k={};
        var o=function (r)
        {
            var i=r.split(":");
            if (i.length===1)
            {
                return parseFloat(i[0], 10);
            } else
            {
                if (i.length===2)
                {
                    return parseFloat(i[0], 10)+parseFloat(i[1]/12, 10);
                } else
                {
                    if (i.length===3)
                    {
                        return parseInt(i[0]*60, 10)+parseFloat(i[1], 10)+parseFloat(i[2]/12, 10);
                    } else
                    {
                        if (i.length===4)
                        {
                            return parseInt(i[0]*3600, 10)+parseInt(i[1]*60, 10)+parseFloat(i[2], 10)+parseFloat(i[3]/12, 10);
                        }
                    }
                }
            }
        };
        var l=function (x)
        {
            var y={};
            for (var s=0, w=x.length; s<w; s++)
            {
                var u=x.item(s).nodeName
                    , r=x.item(s).nodeValue
                    , v=k[r];
                if (u==="in")
                {
                    y.start=o(r);
                } else
                {
                    if (u==="out")
                    {
                        y.end=o(r);
                    } else
                    {
                        if (u==="resourceid")
                        {
                            for (var t in v)
                            {
                                if (v.hasOwnProperty(t))
                                {
                                    if (!y[t]&&t!=="id")
                                    {
                                        y[t]=v[t];
                                    }
                                }
                            }
                        } else
                        {
                            y[u]=r;
                        }
                    }
                }
            }
            return y;
        };
        var f=function (r, i)
        {
            var s={};
            s[r]=i;
            return s;
        };
        var m=function (w, r, v)
        {
            var s={};
            c(s, r, l(w.attributes), {
                text: w.textContent||w.text
            });
            var t=w.childNodes;
            if (t.length<1||(t.length===1&&t[0].nodeType===3))
            {
                if (!v)
                {
                    n.push(f(w.nodeName, s));
                } else
                {
                    k[s.id]=s;
                }
            } else
            {
                for (var u=0; u<t.length; u++)
                {
                    if (t[u].nodeType===1)
                    {
                        m(t[u], s, v);
                    }
                }
            }
        };
        var p=h.documentElement.childNodes;
        for (var j=0, q=p.length; j<q; j++)
        {
            if (p[j].nodeType===1)
            {
                if (p[j].nodeName==="manifest")
                {
                    m(p[j], {}, true);
                } else
                {
                    m(p[j], {}, false);
                }
            }
        }
        return n;
    }
        ;
    b.MediaPlayerSubtitles.parseSBV=function (g)
    {
        var n=[];
        var l;
        var j=0;
        var k=0;
        var q=function (u)
        {
            var s=u.split(":"), r=s.length-1, v;
            try
            {
                v=parseInt(s[r-1], 10)*60+parseFloat(s[r], 10);
                if (r===2)
                {
                    v+=parseInt(s[0], 10)*3600;
                }
            } catch (i)
            {
                throw "Bad cue";
            }
            return v;
        };
        var f=function (r, i)
        {
            var s={};
            s[r]=i;
            return s;
        };
        l=g.text.split(/(?:\r\n|\r|\n)/gm);
        k=l.length;
        while (j<k)
        {
            var m={}
                , o=[]
                , p=l[j++].split(",");
            try
            {
                m.start=q(p[0]);
                m.end=q(p[1]);
                while (j<k&&l[j])
                {
                    o.push(l[j++]);
                }
                m.text=o.join("<br />");
                n.push(f("subtitle", m));
            } catch (h)
            {
                while (j<k&&l[j])
                {
                    j++;
                }
            }
            while (j<k&&!l[j])
            {
                j++;
            }
        }
        return n;
    }
        ;
    b.MediaPlayerSubtitles.parseJSON=function (f)
    {
        var g=f.json;
        var h=[];
        d(g.data, function (j, i)
        {
            h.push(j);
        });
        return h;
    }
        ;
    b.MediaPlayerSubtitles.parseTTXT=function (h)
    {
        var l=function (o)
        {
            var n=o.split(":");
            var p=0;
            try
            {
                return parseFloat(n[0], 10)*60*60+parseFloat(n[1], 10)*60+parseFloat(n[2], 10);
            } catch (m)
            {
                p=0;
            }
            return p;
        };
        var g=function (n, m)
        {
            var o={};
            o[n]=m;
            return o;
        };
        var j=h.xml.lastChild.lastChild;
        var i=Number.MAX_VALUE;
        var f=[];
        while (j)
        {
            if (j.nodeType===1&&j.nodeName==="TextSample")
            {
                var k={};
                k.start=l(j.getAttribute("sampleTime"));
                k.text=j.getAttribute("text");
                if (k.text)
                {
                    k.end=i-0.001;
                    f.push(g("subtitle", k));
                }
                i=k.start;
            }
            j=j.previousSibling;
        }
        return f.reverse();
    }
        ;
    b.MediaPlayerSubtitles.parseTTML=function (f)
    {
        function k(s, v, t)
        {
            var p=s.firstChild, q=i(s, t), u=[], r;
            while (p)
            {
                if (p.nodeType===1)
                {
                    if (p.nodeName==="p")
                    {
                        u.push(l(p, v, q));
                    } else
                    {
                        if (p.nodeName==="div")
                        {
                            r=o(p.getAttribute("begin"));
                            if (r<0)
                            {
                                r=v;
                            }
                            u.push.apply(u, k(p, r, q));
                        }
                    }
                }
                p=p.nextSibling;
            }
            return u;
        }
        function i(q, p)
        {
            var r=q.getAttribute("region");
            if (r!==null)
            {
                return r;
            } else
            {
                return p||"";
            }
        }
        function l(p, s, q)
        {
            var r={};
            r.text=(p.textContent||p.text).replace(n, "").replace(m, "<br />");
            r.id=p.getAttribute("xml:id")||p.getAttribute("id");
            r.start=o(p.getAttribute("begin"), s);
            r.end=o(p.getAttribute("end"), s);
            r.target=i(p, q);
            if (r.end<0)
            {
                r.end=o(p.getAttribute("duration"), 0);
                if (r.end>=0)
                {
                    r.end+=r.start;
                } else
                {
                    r.end=Number.MAX_VALUE;
                }
            }
            return {
                subtitle: r
            };
        }
        function o(r, q)
        {
            var p;
            if (!r)
            {
                return -1;
            }
            p=g(r);
            return parseFloat(r.substring(0, p))*h(r.substring(p))+(q||0);
        }
        function g(q)
        {
            var p=q.length-1;
            while (p>=0&&q[p]<="9"&&q[p]>="0")
            {
                p--;
            }
            return p;
        }
        function h(p)
        {
            return {
                h: 3600,
                m: 60,
                s: 1,
                ms: 0.001
            }[p]||-1;
        }
        var n=/^[\s]+|[\s]+$/gm;
        var m=/(?:\r\n|\r|\n)/gm;
        var j;
        if (!f.xml||!f.xml.documentElement)
        {
            return [];
        }
        j=f.xml.documentElement.firstChild;
        if (!j)
        {
            return [];
        }
        while (j.nodeName!=="body")
        {
            j=j.nextSibling;
        }
        if (j)
        {
            return k(j, 0);
        }
        return [];
    }
        ;
    b.MediaPlayerSubtitles.parseVTT=function (g)
    {
        function t(w)
        {
            var v=w.split(":"), u=w.length, x;
            if (u!==12&&u!==9)
            {
                throw "Bad cue";
            }
            u=v.length-1;
            try
            {
                x=parseInt(v[u-1], 10)*60+parseFloat(v[u], 10);
                if (u===2)
                {
                    x+=parseInt(v[0], 10)*3600;
                }
            } catch (i)
            {
                throw "Bad cue";
            }
            return x;
        }
        function f(u, i)
        {
            var v={};
            v[u]=i;
            return v;
        }
        function m(i)
        {
            var u, x={}, v=/-->/, w=/[\t ]+/;
            if (!i||i.indexOf("-->")===-1)
            {
                throw "Bad cue";
            }
            u=i.replace(v, " --> ").split(w);
            if (u.length<2)
            {
                throw "Bad cue";
            }
            x.id=i;
            x.start=t(u[0]);
            x.end=t(u[2]);
            return x;
        }
        function p(w, v, u)
        {
            while (u<v&&!w[u])
            {
                u++;
            }
            return u;
        }
        function o(w, v, u)
        {
            while (u<v&&w[u])
            {
                u++;
            }
            return u;
        }
        var r=[], j=0, k=0, l, s, q, n=/(?:\r\n|\r|\n)/gm;
        l=g.text.split(n);
        k=l.length;
        if (k===0||l[0]!=="WEBVTT")
        {
            return [];
        }
        j++;
        while (j<k)
        {
            s=[];
            try
            {
                j=p(l, k, j);
                q=m(l[j++]);
                while (j<k&&l[j])
                {
                    s.push(l[j++]);
                }
                q.text=s.join("<br />");
                r.push(f("subtitle", q));
            } catch (h)
            {
                j=o(l, k, j);
            }
        }
        return r;
    }
        ;
    b.MediaPlayerSubtitles.parseSSA=function (g)
    {
        function p(u, t)
        {
            var i=u.substr(10).split(","), v=/\{(\\[\w]+\(?([\w\d]+,?)+\)?)+\}/gi, w=/\\N/gi, x;
            x={
                start: s(i[t.start]),
                end: s(i[t.end])
            };
            if (x.start===-1||x.end===-1)
            {
                throw "Invalid time";
            }
            x.text=j(i, t.text).replace(v, "").replace(w, "<br />");
            return x;
        }
        function s(u)
        {
            var i=u.split(":");
            if (u.length!==10||i.length<3)
            {
                return -1;
            }
            return parseInt(i[0], 10)*3600+parseInt(i[1], 10)*60+parseFloat(i[2], 10);
        }
        function j(u, w)
        {
            var t=u.length
                , x=[]
                , v=w;
            for (; v<t; v++)
            {
                x.push(u[v]);
            }
            return x.join(",");
        }
        function f(t, i)
        {
            var u={};
            u[t]=i;
            return u;
        }
        function o(w)
        {
            var t=w.substr(8).split(", "), x={}, v, u;
            for (u=0,
                v=t.length; u<v; u++)
            {
                if (t[u]==="Start")
                {
                    x.start=u;
                } else
                {
                    if (t[u]==="End")
                    {
                        x.end=u;
                    } else
                    {
                        if (t[u]==="Text")
                        {
                            x.text=u;
                        }
                    }
                }
            }
            return x;
        }
        var q=/(?:\r\n|\r|\n)/gm, r=[], n, k, l=0, m;
        n=g.text.split(q);
        m=n.length;
        while (l<m&&n[l]!=="[Events]")
        {
            l++;
        }
        k=o(n[++l]);
        while (++l<m&&n[l]&&n[l][0]!=="[")
        {
            try
            {
                r.push(f("subtitle", p(n[l], k)));
            } catch (h) { }
        }
        return r;
    }
        ;
    b.MediaPlayerSubtitles.registerClass("Telerik.Web.UI.MediaPlayerSubtitles");
}
)($telerik.$);
if (typeof (Sys)!=='undefined')
    Sys.Application.notifyScriptLoaded();
