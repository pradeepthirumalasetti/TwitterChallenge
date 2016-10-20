//twitter api controller handles the service calls and updates scope vraibles
angular.module('TwitterFeedApp.controllers', []).
  controller('twitterinfocontroller', function ($scope, $timeout,GettwitterInfoService) {
      $scope.screenNameTweetsData = [];

      //Gets and displays top 10 tweets information from salesforce screen
      var GetSalesForceTweets = function () {
          GettwitterInfoService.getScreenNameTweetsData('salesforce').success(function (response) {
              $scope.screenNameTweetsData = response;
          }).failure()
          {
              console.log('System error occured');
          };
      };

      //reloads the sales forces tweets information for every 1 minute
      $scope.reloadTweetsInfo = function () {
          try{
              $timeout(function () {
                  GetSalesForceTweets();
                  $scope.realoadTweetsInfo();
              }, 60000);
          }
          catch(exception)
          {
              console.log(exception);
          }
      };

      // Kick off the interval
      $scope.reloadTweetsInfo();
  });