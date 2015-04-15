(function () {
    // http://localhost:5848/api/tweetsapi?topic=GameOfThrones&time=2015-04-14&maxCount=100

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
            { htName: '#DrWho', isSelected: false },
            { htName: '#buildwindows', isSelected: false }
        ]),
        currentTime: new Date(),
        list: new WinJS.Binding.List([{
            "IdStr": null,
            "Text": "#GameofThrones lots of each\n https://t.co/q9UPK0nuDj",
            "Lang": null,
            "Sentiment": 0,
            "Name": "Little Ted",
            "ProfileImageUrl": "http://pbs.twimg.com/profile_images/1121581727/Publication2_normal.jpg",
            "FavouriteCount": "0",
            "Hashtags": "GameofThrones",
            "RetweetCount": "0",
            "ScreenName": "THELITTLETED"
        }, {
            "IdStr": null,
            "Text": "via @TheWasNews: a quiet force as Jon Snow #GameofThrones Dion Dublin Гюнтер Грасс http://t.co/qzkDNhRvYV",
            "Lang": null,
            "Sentiment": 0,
            "Name": "anyel",
            "ProfileImageUrl": "http://pbs.twimg.com/profile_images/535786621338001408/Hs6-cUxH_normal.jpeg",
            "FavouriteCount": "0",
            "Hashtags": "GameofThrones",
            "RetweetCount": "0",
            "ScreenName": "anyelwae"
        }]),

        // Functions
        //
        fetch: function (date) {
            var url = "http://localhost:5848/api/tweetsapi?topic=GameOfThrones&time=" + date.toISOString() + "&maxCount=100";
            return WinJS.xhr({
                url: url,
                responseType: "json"
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
            context.model.currentMode = (context.model.currentMode.icon === App.modes.play.icon) ? App.modes.pause : App.modes.play;

            if (contex.model.currentMode === App.modes.play) {
                // Call the Web Service


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
            tweetDate: 'April 13, 2015',
            tweetTime: '9:00 PM',
        }
    });

    WinJS.UI.processAll().then(function () {
        WinJS.Binding.processAll(document.body, context);
        App.fetch(new Date(2015, 3, 14)).then(function (arg) {
            arg.response.forEach(function (entry) {
                App.list.unshift(entry);
            });
        });
    });
})();