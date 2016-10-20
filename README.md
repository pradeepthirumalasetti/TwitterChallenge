
Project : Twitter challenge
Summary: Reads twitter api home time line from sales force screen and displays top 10 tweets to the user for every minute
API : https://api.twitter.com/1.1/statuses/user_timeline.json
Language: C#, Angular.js and WCF Restful Service
Tools Required to view code : Visual studio 2013. or open below files in txt
Files : APIInfoProvider WCF Service -> TwitterAPIInfo,TwitterAPI.cs,
web service url : http://localhost:51781/TwitterAPIService.svc/GetHomeTimelineData?strScreenName=salesforce
Note: web service should be running inorder to view display tweets information

Steps followed:

1: create twitter account and got keys  consumer key and secret access key and tokens
2) used the tokens for my login crendentails in the code.
APP Layer:
3) created WCF Restful service which can be invoked using http. This service exposed GetHomeTimeline method to outside world
   which can be invoked using http protocal. This service internally used OAUTH and encoded the request using symmetric SHA1 
   and make calls to twitter api and returns data to the client in JSON format
UI Layer:
4) created angular js single page application which will have controller,service and view page. angular service makes call to the 
  http web get call to app layer JSON Service and updates the view.
5) $timeout angular service used to repopulate the tweets for each 1 minute and updates the view. filter added besied TweetContent allows to search tweets



