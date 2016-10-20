using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TwitterAPIInfo
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(Namespace="TwitterAPIInfo")]
    public interface ITwitterAPIService
    {
        [OperationContract]
        [WebInvoke(ResponseFormat=WebMessageFormat.Json,Method="GET")]
        List<TweetInfo> GetHomeTimelineData(string strScreenName,int messageCount);
    }

    [DataContract]
    public class TweetInfo
    {
        [DataMember]
        public long TweetID { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string ScreenName { get; set; }
        [DataMember]
        public string ProfileImageUrl { get; set; }
        [DataMember]
        public string TweetText { get; set; }
        [DataMember]
        public int RetweetCount { get; set; }
    }
}
