$(function () {

    function numberWithCommas(x) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    // Declare a proxy to reference the hub. 
    var twitterHub = $.connection.twitterHub;
    // Create a function that the hub can call to broadcast messages.
    twitterHub.client.updateCounter = function (rowCount, tweet) {
        setTimeout(function () {
            $('#rowCounter').text(rowCount);
            $('#latestTweets').append('<li>' + tweet + '</li>');
            window.scrollTo(0, document.body.scrollHeight);
        }, 1000);
    };

    //For test purposes
    var tweetId = 0;
    function FakeTweet() {
        twitterHub.client.updateCounter(tweetId, 'Tweet' + tweetId);
        tweetId++;
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
        //setInterval(FakeTweet, 1000);
        //setTimeout(FakeTweet, 1000);
    });
});
