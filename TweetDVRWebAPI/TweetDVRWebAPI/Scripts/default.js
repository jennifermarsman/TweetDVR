(function () {
    // http://localhost:5848/api/tweetsapi?topic=GameOfThrones&time=2015-04-14&maxCount=100

    function merge(a, b) {
        var result = {};
        for (key in a) { result[key] = a[key]; }
        for (key in b) { result[key] = b[key]; }
        return result;
    }

    WinJS.Namespace.define("App", {
        // State
        //
        modes: {
            play: {
                icon: "play",
                label: "Play",
                tooltip: "Play the video"
            },
            pause: {
                icon: "pause",
                label: "Pause",
                tooltip: "Pause the video"
            }
        },
        hashtags: new WinJS.Binding.List([
            { htName: '#GoT', isSelected: true },
            { htName: '#GameOfThrones', isSelected: true }
        ]),
        currentTime: new Date(),
        maxListTime: new Date(),
        list: new WinJS.Binding.List(),

        // Functions
        //
        fetch: function (date) {
            var url = "/api/tweetsapi?topic=GameOfThrones&time=" + date.toISOString() + "&maxCount=100";
            return WinJS.xhr({
                url: url,
                responseType: "json"
            }).then(function (arg) {
                return arg.response.map(function (entry) {
                    return merge(entry, {
                        CreatedAt: new Date(entry.CreatedAt),
                        tweetURL: "https://twitter.com/"+ entry.ScreenName +"/status/"+ entry.IdStr
                    });
                });
            });
        },
        positiveSelected: WinJS.UI.eventHandler(function () {
            context.model.isPositiveSelected = true;
            context.model.isNegativeSelected = false;
            context.model.isNeutralSelected = false;
        }),
        negativeSelected: WinJS.UI.eventHandler(function () {
            context.model.isPositiveSelected = false;
            context.model.isNegativeSelected = true;
            context.model.isNeutralSelected = false;

        }),
        neutralSelected: WinJS.UI.eventHandler(function () {
            context.model.isPositiveSelected = false;
            context.model.isNegativeSelected = false;
            context.model.isNeutralSelected = true;
        }),
        togglePlayPause: WinJS.UI.eventHandler(function (evt) {
            context.modedl.currentMode = (context.model.currentMode.icon === App.modes.play.icon) ? App.modes.pause : App.modes.play;

            if (context.model.currentMode === App.modes.play) {
                App.fetch(context.model.tweetDate).then(function (arg) {
                    arg.forEach(function (entry) {
                        if (entry.Text) {
                            App.list.unshift(entry);
                        }
                    });
                    App.maxListTime = arg[arg.length - 1].CreatedAt;
                });


            };
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
    });

    var context = WinJS.Binding.as({
        model: {
            isPositiveSelected: false,
            isNeutralSelected: true,
            isNegativeSelected: false,
            currentMode: App.modes.play,
            dvrDate: new Date(2015, 3, 12, 21, 0, 0, 0),
            dvrTime: new Date(2015, 3, 12, 21, 0, 0, 0)
        }
    });

    WinJS.UI.processAll().then(function () {
        WinJS.Binding.processAll(document.body, context);

        
    });
})();