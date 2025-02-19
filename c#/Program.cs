using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SDKIntegrationDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string pid = "JP-ABC-TEST"; // （注意，要和RSA公钥对应!）
            // RSA公钥（注意，要和PID对应!）
            string publicKey = @"-----BEGIN PUBLIC KEY-----
你的公钥内容
你的公钥内容
你的公钥内容
你的公钥内容
你的公钥内容
你的公钥内容
你的公钥内容
-----END PUBLIC KEY-----"; // 替换为实际的RSA公钥

            string uid = "123123"; // 用户登录SDK的UID
            string token = "123123"; // 用户登录SDK的Token

            // 构建Head和Data
            var head = new
            {
                Channel = "official", // 渠道参数
                GUID = "123123", // 游戏服UID
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            Console.WriteLine("Head: " + head);

            // 这里构建具体接口的业务参数,以检查用户uid和token接口为例
            var data = new
            {
                UID = uid,
                Token = token
            };
            Console.WriteLine("Data: " + data);
            
            // 合并Head和Data
            var requestBody = new
            {
                Head = head,
                Data = data
            };
            Console.WriteLine("RequestBody: " + data);
            // 序列化为JSON字符串
            string jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            // RSA加密
            string encryptedData = RSAEncrypt(jsonRequestBody, publicKey);
            Console.WriteLine("EncryptedData: " + data);
            // 构建最终请求体
            var finalRequestBody = new
            {
                Data = encryptedData,
                PID = pid
            };
            Console.WriteLine("FinalRequestBody: " + finalRequestBody+"\n");
            string finalJsonRequestBody = JsonConvert.SerializeObject(finalRequestBody);

            // 发送HTTP请求 todo 线上环境必须使用内网域名
            string apiUrl = ""; // todo 替换为实际的API URL
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(finalJsonRequestBody, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response: " + responseBody);
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                }
            }
        }

        static string RSAEncrypt(string plainText, string publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportFromPem(publicKey);

                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = rsa.Encrypt(plainTextBytes, RSAEncryptionPadding.Pkcs1);

                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }
}