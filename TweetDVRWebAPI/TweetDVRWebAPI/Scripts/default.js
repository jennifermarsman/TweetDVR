(function () {

    var _twitterReadyPromise = new WinJS.Promise(function (complete) {
        twttr.ready(function () {
            complete();
        });
    });
    // Returns a version of twitterReadyPromise that cannot be canceled. If
    // twitterReadyPromise was involved in a promise chain directly and the
    // promise chain was canceled, twitterReadyPromise would end up in a canceled
    // state. Then we'd never know when the twitter API was ready.
    function twitterReady() {
        return new WinJS.Promise(function (complete){ 
            _twitterReadyPromise.then(complete);
        });
    }

    function merge(a, b) {
        var result = {};
        for (key in a) { result[key] = a[key]; }
        for (key in b) { result[key] = b[key]; }
        return result;
    }

    var maxTweetsInDom = 50;

    WinJS.Namespace.define("App", {
        // State
        //
        modes: {
            play: {
                icon: "play",
                label: "Play",
                tooltip: "Play"
            },
            pause: {
                icon: "pause",
                label: "Pause",
                tooltip: "Pause"
            }
        },
        topics: new WinJS.Binding.List([
            { tName: "Game of Thrones", tValue: "GameOfThrones", tSelected: false,  initialDate: new Date(Date.UTC(2015, 3, 27, 3)), hashtags: new WinJS.Binding.List([{ htName: '#GoT', isSelected: true }, { htName: '#GameOfThrones', isSelected: true }]) },
            { tName: "NHL Playoffs", tValue: "NHLPlayoffs", tSelected: false,  initialDate: new Date(Date.UTC(2015, 3, 30, 0,1)), hashtags: new WinJS.Binding.List([{ htName: '#NHL', isSelected: true }, { htName: '#NHLPlayoffs', isSelected: true }, { htName: '#BecauseItsTheCup', isSelected: true }, { htName: 'StanleyCup', isSelected: true }, { htName: 'MyPlayoffsMoment', isSelected: true }]) },
            { tName: "Microsoft Build", tValue: "MSBuild", tSelected: true, initialDate: new Date(Date.UTC(2015, 3, 29, 18,30)), hashtags: new WinJS.Binding.List([{ htName: '#HDInsights', isSelected: true }, { htName: '#HDIonAzure', isSelected: true }, { htName: '#MSBuild', isSelected: true }, { htName: '#Microsoft', isSelected: true }, { htName: 'bldwin', isSelected: true }]) }
        ]),
        lastTweet: "",
        maxListTime: "",
        list: new WinJS.Binding.List(),
        pendingReset: true,
        pendingTweets: [],
        tickerPromise: null, // Promise will never complete successfully. Cancel to stop ticking.
        updateTweetsPromise: null,

        // Functions
        //
        tweetRenderer: WinJS.Utilities.markSupportedForProcessing(function (tweet) {
            var element = document.createElement("div");
            twttr.widgets.createTweet(tweet.IdStr, element, { theme: 'dark' });
            return element;
        }),
        dvrDateTime: {
            get: function () {
                var date = context.model.dvrDate;
                var time = context.model.dvrTime;
                return new Date(
                    date.getFullYear(), date.getMonth(), date.getDate(),
                    time.getHours(), time.getMinutes(), time.getSeconds(), time.getMilliseconds()
                );
            }
        },
        changeTopic: function (option) {
            for (var i = 0; i < App.topics.length; i++) {
                if (App.topics.getAt(i).tValue === option.value) {
                    context.model.selectedTopic = App.topics.getAt(i);
                    document.getElementById("hashtagRepeater").winControl.data = context.model.selectedTopic.hashtags;
                    context.model.dvrDate = context.model.selectedTopic.initialDate;
                    context.model.dvrTime = context.model.selectedTopic.initialDate;
                    App.pendingReset = true;
                }
            }
        },
        fetch: function (date, maxCount) {
            var isoDate = date.toISOString();
            var apiDate = isoDate.substring(0, isoDate.length - 2);
            var url = "/api/tweetsapi?topic=" + context.model.selectedTopic.tValue + "&time=" + apiDate + "&maxCount=" + maxCount;
            console.log("fetch: " + url);
            console.log("REQ: " + date.toString());
            return WinJS.xhr({
                url: url,
                responseType: "json"
            }).then(function (arg) {
                var tweetArray = typeof arg.response === "string" ? JSON.parse(arg.response) : arg.response;
                App.lastFetchTime = Date.now();
                return tweetArray.map(function (entry) {
                    return merge(entry, {
                        CreatedAt: new Date(entry.CreatedAt + "Z")
                    });
                });
            });
        },
        fetchMore: function () {
            var maxListTime = App.maxListTime;
            var pendingTweets = App.pendingTweets;
            var fetchCount = 100;
     
            return App.fetch(maxListTime, fetchCount).then(function (newTweets) {
                newTweets.forEach(function (tweet) {
                    if (App.lastTweet != tweet.IdStr) {
                        pendingTweets.push(tweet);
                        App.lastTweet = tweet.IdStr
                    }
                    
                });
                App.maxListTime = pendingTweets[pendingTweets.length - 1].CreatedAt;
            });
        },
        processPendingTweets: function () {
            var dvrDateTime = App.dvrDateTime;
            var uiList = App.list;
            var pendingTweets = App.pendingTweets;
            for (var i = 0; i < pendingTweets.length && pendingTweets[i].CreatedAt <= dvrDateTime; i++) {
                uiList.unshift(pendingTweets[i]);
            }
            if (i > 0) {
                // Only slice if we moved some pendingTweets to the UI
                App.pendingTweets = pendingTweets.slice(i);
            }
            if (uiList.length > maxTweetsInDom) {
                uiList.length = maxTweetsInDom;
            }
        },
        updateTweets: function () {
            if (!App.updateTweetsPromise) {
                App.processPendingTweets();
                if (App.pendingTweets.length < 20) {
                    App.updateTweetsPromise = App.fetchMore().then(function () {
                        App.updateTweetsPromise = null;
                        App.updateTweets();
                    }, function () {
                        App.updateTweetsPromise = null;
                    });
                }
            }
        },
        startPlaying: function () {
            var interval;
            this.stopPlaying();
            App.tickerPromise = twitterReady().then(function () {
                interval = setInterval(function () {
                    var dvrDateTime = new Date(App.dvrDateTime.getTime() + 1000);
                    context.model.dvrDate = dvrDateTime;
                    context.model.dvrTime = dvrDateTime;
                    if (App.pendingReset) {
                        App.pendingReset = false;
                        App.resetState(dvrDateTime);
                    }
                    console.log("Tick: " + dvrDateTime);
                    App.updateTweets();
                }, 1000);

                return new WinJS.Promise(function () { /* never complete successfully */ });
            }).then(null, function () {
                interval && clearInterval(interval);
            })
        },
        stopPlaying: function () {
            App.tickerPromise && App.tickerPromise.cancel();
            App.tickerPromise = null;
        },
        resetState: function (dvrDateTime) {
            App.list.length = 0;
            App.pendingTweets = [];
            App.maxListTime = dvrDateTime;
        },
        togglePlayPause: WinJS.UI.eventHandler(function (evt) {
            context.model.currentMode = (context.model.currentMode.icon === App.modes.play.icon) ? App.modes.pause : App.modes.play;

            if (context.model.currentMode.icon === App.modes.pause.icon) {
                // Now playing
                App.startPlaying();
                
                if (context.model.isLive) {
                    context.model.isLive = false;
                    document.getElementById("cmdLive").winControl.selected = false;
                    $.connection.hub.stop();
                }
            } else {
                // Now paused
                App.stopPlaying();
            }
        }),
        toggleLiveMode: WinJS.UI.eventHandler(function (evt) {
            context.model.isLive = document.getElementById("cmdLive").winControl.selected
            if (context.model.isLive) {
                context.model.currentMode = App.modes.play;
                App.list.length = 0
                App.pendingReset = true;
                App.stopPlaying();
                $.connection.hub.start();
            } else {
                $.connection.hub.stop();
            }

        }),
        toggleHashtag: WinJS.UI.eventHandler(function (evt) {
            var hashtags = context.model.selectedTopic.hashtags;
            var chk = evt.currentTarget;
            var hashtag = chk.value;

            for (var i = 0; i < hashtags.length; i++) {
                if (hashtags.getAt(i).htName === hashtag) {
                    hashtags.getAt(i).isSelected = chk.checked;
                    hashtags.setAt(i, hashtags.getAt(i));
                }
            }

        }),
        dateChanged: WinJS.UI.eventHandler(function (evt) {
            context.model.dvrDate = evt.currentTarget.winControl.current;
            App.pendingReset = true;
        }), 
        timeChanged: WinJS.UI.eventHandler(function (evt) {
            context.model.dvrTime = evt.currentTarget.winControl.current;
            App.pendingReset = true;
        }),
    });

    var context = WinJS.Binding.as({
        model: {
            isLive: false,
            selectedTopic: App.topics.getAt(2),
            currentMode: App.modes.play,
            dvrDate: App.topics.getAt(2).initialDate,
            dvrTime: App.topics.getAt(2).initialDate
        }   
    });

    var twitterHub = $.connection.twitterHub;
    twitterHub.client.showTweet = function (topic, tweetId) {
        if (topic === context.model.selectedTopic.tValue) {
            var uiList = App.list;
            uiList.unshift({ IdStr: tweetId });
            if (uiList.length > maxTweetsInDom) {
                uiList.length = maxTweetsInDom;
            }
        }
    };

    WinJS.UI.processAll().then(function () {
        WinJS.Binding.processAll(document.body, context);
        document.getElementById("hashtagRepeater").winControl.data = App.topics.getAt(2).hashtags;
    });

    window.context = context;
})();