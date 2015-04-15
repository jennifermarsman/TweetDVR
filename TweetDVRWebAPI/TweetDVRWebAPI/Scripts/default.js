(function () {
    // http://localhost:5848/api/tweetsapi?topic=GameOfThrones&time=2015-04-14&maxCount=100
    window.doFetch = function () {
        debugger;
        WinJS.xhr({
            url: "http://localhost:5848/api/tweetsapi?topic=GameOfThrones&time=2015-04-14",
            responseType: "json"
        }).then(function (arg) {
            debugger;
        }, function () {
            debugger;
        });
    };

    WinJS.Namespace.define("App", {
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

        fetch: function (date) {
            var url = "http://localhost:5848/api/tweetsapi?topic=GameOfThrones&time=" + date.toISOString() + "&maxCount=100";
            return WinJS.xhr({
                url: url,
                responseType: "json"
            });
        }
    });

    WinJS.UI.processAll().then(function () {
        App.fetch(new Date(2015, 3, 14)).then(function (arg) {
            arg.response.forEach(function (entry) {
                App.list.unshift(entry);
            });
        });
    });
})();