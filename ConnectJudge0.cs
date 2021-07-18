using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System;
using System.IO;

public class ConnectJudge0 : MonoBehaviour
{
    public InputField chatInput;
    public Text chatText;
    public ScrollRect scrollRect;

    // 轉成json的儲存型態
    public class ToJson
    {
        public string source_code;
        public string language_id;

        public ToJson(string code, string id)
        {
            source_code = code;
            language_id = id;
        }
        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    // 將json裡stdout的資料提取
    public class OriginalText
    {
        public string stdout;
    }

    // 點選提交按鈕
    public void OnClick()
    {
        if (chatInput.text != "")
        {
            string inputString = chatInput.text;    //取得輸入資料
            chatInput.text = "";    //清空輸入框
            // 加密
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(inputString);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true&fields=stdout";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); //建立 HTTP request 物件
            request.Timeout = 100000000;    //傳送逾時時間
            request.Method = "POST";    //設定傳送方式 (POST)
            request.ContentType = "application/json; charset=utf-8";    //設定內容類型
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                string json = new ToJson(returnValue, "54").SaveToString();
                sw.Write(json);     //寫入BODY的JSON 檔案 到 request 物件內
                sw.Flush();         //執行FLUSH 將所有資料存入資料流中
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)  //抓取錯誤訊息
            {
                Console.WriteLine("This program is expected to throw WebException on successful run." +
                            "\n\nException Message :" + e.Status.ToString());
                Console.ReadKey();  //暫停程式 以便讀取錯誤訊息
                System.Environment.Exit(0); //結束程式
            }
            string addText = "";
            // 取出API回傳訊息
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                OriginalText NameObject = JsonUtility.FromJson<OriginalText>(result); //獲取base64加密字串
                string originalText = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(NameObject.stdout)); //解密
                addText = originalText;
            }

            chatText.text += addText;
            chatInput.ActivateInputField();
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}
