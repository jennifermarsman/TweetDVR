﻿(function () {

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

    var initialDate = new Date(Date.UTC(2015, 3, 12, 20));
    var dvrDateControl;
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
            { tName: "Game of Thrones", tValue: "GameOfThrones", hashtags: new WinJS.Binding.List([{ htName: '#GoT', isSelected: true }, { htName: '#GameOfThrones', isSelected: true }]) },
            { tName: "NHL Playoffs", tValue: "NHLPlayoffs", hashtags: new WinJS.Binding.List([{ htName: '#NHL', isSelected: true }, { htName: '#NHLPlayoffs', isSelected: true }, { htName: '#BecauseItsTheCup', isSelected: true }, { htName: 'StanleyCup', isSelected: true }, { htName: 'MyPlayoffsMoment', isSelected: true }]) },
            { tName: "Microsoft Build", tValue: "MSBuild", hashtags: new WinJS.Binding.List([{ htName: '#HDInsights', isSelected: true }, { htName: '#HDIonAzure', isSelected: true }, { htName: '#MSBuild', isSelected: true }, { htName: '#Microsoft', isSelected: true }, { htName: 'bldwin', isSelected: true }]) }
        ]),
        lastTweet: "",
        maxListTime: initialDate,
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
        sentimentFilter: {
            get: function () {
   
                if (context.model.isPositiveSelected) {
                    return 1;
                }
                if (context.model.isNeutralSelected) {
                    return 0;
                }
                if (context.model.isNegativeSelected) {
                    return -1;
                }
                    return 3;

            }
        },
        changeTopic: function (option) {
            for (var i = 0; i < App.topics.length; i++) {
                if (App.topics.getAt(i).tValue === option.value) {
                    context.model.selectedTopic = App.topics.getAt(i);
                    document.getElementById("hashtagRepeater").winControl.data = context.model.selectedTopic.hashtags;
                    App.pendingReset = true;
                }
            }
        },
        fetch: function (date, maxCount) {
            var isoDate = date.toISOString();
            var apiDate = isoDate.substring(0, isoDate.length - 2);
            var url = "/api/tweetsapi?topic=" + context.model.selectedTopic.tValue + "&time=" + apiDate + "&maxCount=" + maxCount + "&sentimentFilter=" + App.sentimentFilter;
            console.log("fetch: " + url);
            console.log("REQ: " + date.toString());
            return WinJS.xhr({
                url: url,
                responseType: "json"
            }).then(function (arg) {
                var sentimentMapping = {
                    "-1": ":(",
                    "0": ":|",
                    "1": ":)"
                };
                var tweetArray = typeof arg.response === "string" ? JSON.parse(arg.response) : arg.response;
                App.lastFetchTime = Date.now();
                return tweetArray.map(function (entry) {
                    var sentimentFace;
                    return merge(entry, {
                        CreatedAt: new Date(entry.CreatedAt + "Z"),
                        sentimentFace: sentimentMapping[entry.Sentiment]
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
            var newPendingTweets = [];
            var tweetContainer = document.getElementById("container");
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
        allSelected: WinJS.UI.eventHandler(function () {
                context.model.isAllSelected = true;
                context.model.isPositiveSelected = false;
                context.model.isNegativeSelected = false;
                context.model.isNeutralSelected = false;
                App.pendingReset = true;
        }),
        positiveSelected: WinJS.UI.eventHandler(function () {
            context.model.isAllSelected = false;
            context.model.isPositiveSelected = true;
            context.model.isNegativeSelected = false;
            context.model.isNeutralSelected = false;
            App.pendingReset = true;
        }),
        negativeSelected: WinJS.UI.eventHandler(function () {
            context.model.isAllSelected = false;
            context.model.isPositiveSelected = false;
            context.model.isNegativeSelected = true;
            context.model.isNeutralSelected = false;
            App.pendingReset = true;
        }),
        neutralSelected: WinJS.UI.eventHandler(function () {
            context.model.isAllSelected = false;
            context.model.isPositiveSelected = false;
            context.model.isNegativeSelected = false;
            context.model.isNeutralSelected = true;
            App.pendingReset = true;
        }),
        togglePlayPause: WinJS.UI.eventHandler(function (evt) {
            context.model.currentMode = (context.model.currentMode.icon === App.modes.play.icon) ? App.modes.pause : App.modes.play;

            if (context.model.currentMode.icon === App.modes.pause.icon) {
                // Now playing
                App.startPlaying();
                document.getElementById("cmdLive").winControl.selected = false;
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
            } else {
                // Not Live
            }

            /*
            var tb = document.getElementById("dvrToolbar");
            if (tb.winControl.data.length === 6) 
                tb.winControl.data.splice(2, 1);
            else
                tb.winControl.data.splice(2, 0, dvrDateControl);
             */

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
            isAllSelected: true,
            isPositiveSelected: false,
            isNeutralSelected: false,
            isNegativeSelected: false,
            isLive: false,
            selectedTopic: App.topics.getAt(0),
            currentMode: App.modes.play,
            dvrDate: initialDate,
            dvrTime: initialDate
        }   
    });

    WinJS.UI.processAll().then(function () {
        WinJS.Binding.processAll(document.body, context);
        document.getElementById("hashtagRepeater").winControl.data = App.topics.getAt(0).hashtags;
        dvrDateControl = document.getElementById("dvrToolbar").winControl.data.getAt(2);
    });

    window.context = context;
})();