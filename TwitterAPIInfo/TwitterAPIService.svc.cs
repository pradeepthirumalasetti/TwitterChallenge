using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using TwitterAPIInfo.APIController;

namespace TwitterAPIInfo
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class TwitterAPIService : ITwitterAPIService
    {
        public List<TweetInfo> GetHomeTimelineData(string strScreenName, int messageCount = 10)
        {
            List<TweetInfo> lstTweetsData = null;
            try
            {
                var oauth = new OAuthInfo
                {
                    AccessToken = "3243686535-tNfNY6vh6b0KX1b0j4jj1tKNhMhgbbLqbq754JH",
                    AccessSecret = "a9x26TmHthF9hKEnAXBdYGepBRV4S18bnOEuNoQHnvj7Z",
                    ConsumerKey = "73NX1unppnzaD682Swqx82m3r",
                    ConsumerSecret = "HfA0hwbZUZz7WgTPjEjAQUWOpgnnFfGehTL8dIdQyJZaj2zwAj"
                };

                APIInfoProvider objApiInfo = new APIInfoProvider(oauth);
                 lstTweetsData = objApiInfo.GetHomeTimeline(count: messageCount, screenName: strScreenName);
            }
            catch(Exception ex)
            {
                ex.Message.ToString();
            }
            return lstTweetsData;
        }
    }
}
