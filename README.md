# TweetDVR
Using Azure's big data capabilities, we grab a tweet stream for a show and then you can watch the twitter stream as it was in realtime when you are watching a DVRed show.  For more description and an architecture diagram, please see http://aka.ms/TweetDVR.  

# Set up instructions 
In the Azure portal, create an HBase cluster and a Storm cluster.  
(There is a script for setup coming soon.)  

# Social Archive Application
This is a basic driver to upload and prime HBase with tweet data.  This is the historical pull (to grab the data from the time that the Game of Thrones season premiere aired).    

# StormTweetDVRDemo
This is the Storm real-time data pull which pulls current #GoT and #GameOfThrones tweets from Twitter into HBase and SignalR.  

# StormTwitterSampleWebApp1
This is a SignalR web frontend, used for testing.  

# TweetDVRWebAPI
This is our production web frontend.  You can see this code running live at http://datadvr.com.  
