(function () {

    twttr.ready(function () {
        console.log("twttr ready");
    });

    function merge(a, b) {
        var result = {};
        for (key in a) { result[key] = a[key]; }
        for (key in b) { result[key] = b[key]; }
        return result;
    }

    var initialDate = new Date(Date.UTC(2015, 3, 12, 20));

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
        hashtags: new WinJS.Binding.List([
            { htName: '#GoT', isSelected: true },
            { htName: '#GameOfThrones', isSelected: true }
        ]),
        lastTweet: "",
        maxListTime: initialDate,
        list: new WinJS.Binding.List(),
        pendingReset: true,
        pendingTweets: [],
        tickTimer: 0,
        updateTweetsPromise: null,

        // Functions
        //
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
        fetch: function (date, maxCount) {
            var isoDate = date.toISOString();
            var apiDate = isoDate.substring(0, isoDate.length - 2);
            var url = "/api/tweetsapi?topic=GameOfThrones&time=" + apiDate + "&maxCount=" + maxCount + "&sentimentFilter=" + App.sentimentFilter;
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
                App.lastFetchTime = Date.now();
                return arg.response.map(function (entry) {
                    console.log("GOT: " + new Date(entry.CreatedAt).toString());
                    var sentimentFace;
                    return merge(entry, {
                        CreatedAt: new Date(entry.CreatedAt),
                        tweetURL: "https://twitter.com/" + entry.ScreenName + "/status/" + entry.IdStr,
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
                console.log("moved tweet: " + pendingTweets[i].CreatedAt);
                //uiList.unshift(pendingTweets[i]);
                var addDiv = document.createElement("div");
                tweetContainer.insertBefore(addDiv, tweetContainer.firstElementChild);
                twttr.widgets.createTweet(pendingTweets[i].IdStr, addDiv, {theme: 'dark'});
                //twttr.widgets.createTweet('266031293945503744', document.getElementById("tweetme"));
            }
            if (i > 0) {
                // Only slice if we moved some pendingTweets to the UI
                App.pendingTweets = pendingTweets.slice(i);
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
            App.tickTimer = setInterval(function () {
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
        },
        stopPlaying: function () {
            clearInterval(App.tickTimer);
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
            } else {
                // Now paused
                App.stopPlaying();
            }
        }),
        toggleHashtag: WinJS.UI.eventHandler(function (evt) {
            var hashtags = App.hashtags;
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
            currentMode: App.modes.play,
            dvrDate: initialDate,
            dvrTime: initialDate
        }   
    });

    WinJS.UI.processAll().then(function () {
        WinJS.Binding.processAll(document.body, context);

        
    });
})();