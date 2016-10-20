angular.module('TwitterFeedApp.services', []).
  factory('GettwitterInfoService', function ($http) {
      var GettwitterInfoService = {};

      GettwitterInfoService.getScreenNameTweetsData = function (screenName) {
          try {
              return $http({
                  method: 'GET',
                  url: 'http://localhost:51781/TwitterAPIService.svc/GetHomeTimelineData?strScreenName=salesforce'
              });
          }
          catch (exception) {
              console.log(exception);
          }
      };
      return GettwitterInfoService;
  });