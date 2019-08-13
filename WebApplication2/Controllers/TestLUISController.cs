using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication2.Controllers
{
    public class TestLUISController : isRock.LineBot.LineWebHookControllerBase
    {
        const string AdminUserId = "U02cc8b56f94e6ea73cfeb1c548f86f6c";
        const string channelAccessToken = "L0VY1/rrudIQHhZ8hfrIzRqzs3pF2dqQ+FbTSEPbLeWn9dKEq3biPEbtwiQpQz5IhUnOgJIirGVI/y1oq6GoSvLKbx6/3JVhdlDgXN1tlsRb6NispLrHC005RX9b7SNH6ujIOBdCWuuCOqaLpIbnJAdB04t89/1O/w1cDnyilFU=";
        const string LuisAppId = "27c60253-e2d9-47f6-98da-6647a755b8ac";
        const string LuisAppKey = "ce5afac599e243ceb446a5bb5c87dbbb";
        const string Luisdomain = "westus"; //ex.westus

        [Route("api/TestLUIS")]
        [HttpPost]
        public IHttpActionResult POST()
        {
            try
            {
              
                //設定ChannelAccessToken(或抓取Web.Config)
                this.ChannelAccessToken = channelAccessToken;

                
                //取得Line Event(範例，只取第一個)
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                DateTime dt = DateTime.Now.ToLocalTime();
                

                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                //回覆訊息
                if (LineEvent.type == "message")
                {
                    var repmsg = "";
                    if (LineEvent.message.type == "text") //收到文字
                    {
                        //建立LuisClient
                        Microsoft.Cognitive.LUIS.LuisClient lc =
                          new Microsoft.Cognitive.LUIS.LuisClient(LuisAppId, LuisAppKey, true, Luisdomain);

                        //Call Luis API 查詢
                        var ret = lc.Predict(LineEvent.message.text).Result;

                        if (ret.Intents.Count() <= 0)
                            repmsg = $"你說了 '{LineEvent.message.text}' ，但我看不懂喔!";
                        else if (ret.TopScoringIntent.Name == "None")
                            repmsg = $"嗨~你有什麼話想說嗎?";
                        else
                        {
                            if (ret.TopScoringIntent.Name == "投訴")
                                repmsg = $"抱歉~造成您的困擾，敬請見諒，已將您的意見記錄下來，我們將在3-5天內由專人回覆您：";
                            if (ret.TopScoringIntent.Name == "建議")
                                repmsg = $"感謝您寶貴的意見，讓我們未來能提供更優質的服務：";
                            if (ret.TopScoringIntent.Name == "讚揚")
                                repmsg = $"您的肯定是我們最大的動力，希望能有榮幸再次為您服務：";
                            repmsg += $"\n ";
                            repmsg += $"\n時間：{dt.ToLongDateString().ToString()}";
                            repmsg += $"\n類別：{ret.TopScoringIntent.Name}";
                            if (ret.Entities.Count > 0)
                                repmsg += $"\n項目：{ret.Entities.FirstOrDefault().Key}";
                            else 
                                repmsg += $"\n項目：其他";
                            repmsg += $"\n意見：{LineEvent.message.text}";

                            //ret.Entities => Dict
                            //ret.Entities.FirstOrDefault() => KeyValuePair
                            //ret.Entities.FirstOrDefault().Key => type
                            //repmsg += $"，你在說的是{ret.Entities.FirstOrDefault().Value.FirstOrDefault().Value}";
                        }
                        //回覆
                        this.ReplyMessage(LineEvent.replyToken, repmsg);
                    }
                    if (LineEvent.message.type == "sticker") //收到貼圖
                        this.ReplyMessage(LineEvent.replyToken, 1, 2);
                }
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果發生錯誤，傳訊息給Admin
                this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }
    }
}
