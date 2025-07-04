using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;
using System.Runtime.InteropServices;


namespace MesDemo
{
    public static class Constants
    {
        public const string TisGeneralWebService = "/XNet.TISWebService/BLL/General/GeneralWebService.asmx/NewTisGeneralWebService";
        public const string TisProduceWebService = "/XNet.TISWebService/BLL/Produce/ProduceWebService.asmx/TisProduceWebService";
        public const string TisProductDebugLogUpload = "/XNet.TISWebService/BLL/Produce/ProduceWebService.asmx/ProductDebugLogUpload";
        public const string TisWipTrackEx = "/XNet.TISWebService/BLL/Produce/ProduceWebService.asmx/TISWipTrackEx";
        public const string TisGetConfigFileByProductSN = "/XNet.TISWebService/BLL/Produce/ProduceWebService.asmx/GetConfigFileByProductSN";
        public const string TisGetSoftwareSecretkey = "/XNet.TISWebService/BLL/CustomerService/CustomerServiceWebService.asmx/GetSoftwareSecretkey";

        public const string RC4_PASSWORD_KEY = "LdRBS2014.";
        public const string RC4_CONTENT_KEY = "landi1234.,!@#";
    }

    public struct HttpInfo
    {
        public string port;
        public string ip;
        public string url;
    }

    // 定义通用数据收发接口
    public static class IFHttp
    {
        // 修改：同步方法，使用元组返回结果
        public static (int StatusCode, string RetMsg) HttpPostRequest(HttpInfo httpInfo, string postData)
        {
            string retmsg = "";
            try
            {
                string scheme = "http";
                string requestHeader = $"{scheme}://{httpInfo.ip}:{httpInfo.port}";
                string fullRequest = $"{requestHeader}{httpInfo.url}";

                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
                    
                    HttpResponseMessage response = client.PostAsync(fullRequest, content).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return ((int)response.StatusCode, $"errCode:{(int)response.StatusCode}, retMsg:{response.ReasonPhrase}");
                    }
                    
                    string xmlString = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        JObject rootObj = ConvertXmlToJson(xmlString);

                        bool isOk = rootObj["IsOK"]?.ToString().ToLower() == "true" || rootObj["IsOK"]?.ToString() == "1";
                        int statuscode = isOk ? 0 : -1;
                        retmsg = rootObj.ToString(Formatting.None);
                        return (statuscode, retmsg);
                    }
                    catch (Exception ex)
                    {
                        return (-1, $"XML parsing error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (-1, $"Request error: {ex.Message}");
            }
        }

        private static JObject ConvertXmlToJson(string xmlString)
        {
            try
            {
                // 移除XML声明，避免解析问题
                if (xmlString.StartsWith("<?xml"))
                {
                    int index = xmlString.IndexOf("?>");
                    if (index >= 0)
                    {
                        xmlString = xmlString.Substring(index + 2).Trim();
                    }
                }

                // 解析XML并转换为JSON
                var doc = System.Xml.Linq.XDocument.Parse(xmlString);
                var root = new JObject();

                // 处理根元素，直接将子元素添加到root中
                foreach (var element in doc.Root.Elements())
                {
                    ProcessElement(element, root);
                }

                return root;
            }
            catch (Exception ex)
            {
                // 如果XML解析失败，尝试将原始XML作为字符串返回
                return new JObject
                {
                    ["rawXml"] = xmlString,
                    ["parseError"] = ex.Message
                };
            }
        }

        // 递归处理XML元素 - 修改版
        private static void ProcessElement(System.Xml.Linq.XElement element, JObject parent)
        {
            // 处理元素属性
            var attributes = new JObject();
            foreach (var attr in element.Attributes())
            {
                attributes[attr.Name.LocalName] = attr.Value;
            }

            // 处理子元素
            var children = new JObject();
            var elementList = new List<JObject>();

            // 检查是否有子元素
            bool hasChildren = element.Elements().Any();

            // 处理元素值
            string elementValue = element.Value.Trim();

            // 如果有子元素，递归处理
            if (hasChildren)
            {
                foreach (var child in element.Elements())
                {
                    // 特殊处理：如果子元素名称与父元素相同，创建嵌套结构
                    if (child.Name.LocalName == element.Name.LocalName)
                    {
                        var childObj = new JObject();
                        ProcessElement(child, childObj);
                        elementList.Add(childObj);
                    }
                    else
                    {
                        ProcessElement(child, children);
                    }
                }

                // 如果有同名子元素，转为数组
                if (elementList.Any())
                {
                    children[element.Name.LocalName] = JArray.FromObject(elementList);
                }

                // 将子元素添加到父对象
                if (children.Count > 0)
                {
                    parent[element.Name.LocalName] = children;
                }
            }
            // 如果没有子元素但有属性，将属性和值合并
            else if (attributes.Count > 0)
            {
                if (!string.IsNullOrEmpty(elementValue))
                {
                    attributes["value"] = elementValue;
                }
                parent[element.Name.LocalName] = attributes;
            }
            // 否则直接添加值
            else if (!string.IsNullOrEmpty(elementValue))
            {
                parent[element.Name.LocalName] = elementValue;
            }
        }
    }

    // 定义业务数据收发接口
    public static class STHttp
    {
        const bool LOGTOWINDOW = true;

        [DllImport("LandiSecurity.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, EntryPoint = "RC4_EncryptData")]
        private static extern IntPtr RC4_EncryptData(IntPtr pcInData, IntPtr pcKey);

        private static string IRC4_EncryptData(string password, ref string encPassword)
        {
            string retMsg = "";
            string strEncPassword = "";
            try
            {
                IntPtr ipt1 = Marshal.StringToHGlobalAnsi(password);
                IntPtr ipt2 = Marshal.StringToHGlobalAnsi(Constants.RC4_PASSWORD_KEY);
                IntPtr ipt3 = RC4_EncryptData(ipt1, ipt2);  // 调用c接口对密码进行加密
                strEncPassword = Marshal.PtrToStringAnsi(ipt3);
            }
            catch (System.Exception ex)
            {
                retMsg = "密码加密失败:" + ex.Message;
                return retMsg;
            }
            if (!strEncPassword.StartsWith("00"))
            {
                retMsg = "密码加密失败！";
                return retMsg;
            }
            encPassword = strEncPassword.Substring(3);

            return retMsg;
        }

        public static int GetConfigFileByProductSN()
        {
            HttpInfo httpInfo = new HttpInfo
            {
                url = Constants.TisGetConfigFileByProductSN,
                ip = "10.10.30.38",
                port = "80"
            };
            string currentDate = DateTime.Now.ToString("yyyy_MM_dd");
            string _logFolderPath = @"C:\Qualcomm\UploadDataLog";
            _logFolderPath = Path.Combine(_logFolderPath, currentDate);
            string _logFileName = "UploadDataLog.log";
            

            // 这里需要实现 RC4 加密逻辑
            // _params.Password = ...;
            string encPassword = "";
            string encRes = IRC4_EncryptData("11111111", ref encPassword);
            if (string.IsNullOrEmpty(encRes))
                Logger.Log(encRes);
            Logger.Log(encPassword);
            string post = $"account=lixy1&password={encPassword}&productSN=24A03600028&materialTypeCode=&";
            string retMsg = "";
            int statusCode;
            (statusCode, retMsg) = IFHttp.HttpPostRequest(httpInfo, post);

            Logger.Log($"statusCode:{statusCode}\n" +retMsg);
            if (statusCode == 0)
            {
                try
                {
                    JObject obj = JObject.Parse(retMsg);
                    string SummonsNumber = obj["SucceedDescription"].ToString();
                    Logger.Log(SummonsNumber);
                    return 0;
                }
                catch (Exception ex)
                {
                    string Msg = $"JSON parsing error1: {ex.Message}";
                    Logger.Log(Msg);
                    return -1;
                }
                //logger.Log(message);
            }
            else
            {
                try
                {
                    JObject obj = JObject.Parse(retMsg);
                    string Msg = obj["Description"].ToString();
                    Logger.Log(Msg);
                    return -1;
                }
                catch (Exception ex)
                {
                    string Msg = $"JSON parsing error2: {ex.Message}";
                    Logger.Log(Msg);
                    return -1;
                }
                //logger.Log(message);
            }

            return 0;
        }

        public static int GetBomInfo(TisProduceWebService_Params _params, GetBomInfoByBoardSN_Res _res)
        {
            HttpInfo httpInfo = new HttpInfo
            {
                url = Constants.TisProduceWebService,
                ip = _params.Ip,
                port = _params.Port
            };

            _params.ApiName = "GetBomInfoByBoardSN";
            string post = _params.ToQueryString();

            // 修改：同步调用
            var (status, retMsg) = IFHttp.HttpPostRequest(httpInfo, post);
            if (status != 0)
            {
                _res.Msg = retMsg;
                return -1;
            }

            try
            {
                // 修改：注意这里的赋值可能需要特殊处理，因为 _res 是引用类型
                // 这里可能需要使用反射或其他方法来正确赋值
                GetBomInfoByBoardSN_Res tempRes = JsonConvert.DeserializeObject<GetBomInfoByBoardSN_Res>(retMsg);
                _res.Esim = tempRes.Esim;
                _res.SummonsNumber = tempRes.SummonsNumber;
                _res.MaterialCode = tempRes.MaterialCode;
                _res.BdtName = tempRes.BdtName;
                _res.SoftwareList = tempRes.SoftwareList;
                _res.Msg = tempRes.Msg;
                return 0;
            }
            catch (Exception ex)
            {
                _res.Msg = $"Deserialization error: {ex.Message}";
                return -1;
            }
        }

        public static int GetSoftwareDownloadURL(GetSoftwareDownloadURL_Params _params, GetBomInfoByBoardSN_Res _res)
        {
            foreach (var software in _res.SoftwareList)
            {
                software.DlURL = $"http://{_params.Ip}:{_params.Port}/XNet.TISWebService/BLL/CustomerService/FileDownload.aspx?account={_params.UserName}&FileName={software.LocalFileName}";
            }
            return 0;
        }

        // 修改：同步方法
        public static int GetSoftwareSecretkey(GetSoftwareSecretkey_Params _params, GetBomInfoByBoardSN_Res _res)
        {
            HttpInfo httpInfo = new HttpInfo
            {
                url = Constants.TisGetSoftwareSecretkey,
                ip = _params.Ip,
                port = _params.Port
            };

            //_params.Account = FrmMain.GetToolArgv().UserName;
            //_params.Password = FrmMain.GetToolArgv().UserPassword;

            foreach (var software in _res.SoftwareList)
            {
                _params.AccessoryFileName = software.LocalFileName;
                string post = _params.ToQueryString();

                // 修改：同步调用
                var (status, retMsg) = IFHttp.HttpPostRequest(httpInfo, post);
                if (status != 0)
                {
                    _res.Msg = retMsg;
                    return -1;
                }

                try
                {
                    JObject obj = JObject.Parse(retMsg);
                    software.SecretKey = obj["extdata"].ToString().ToUpper();
                    // 这里需要实现 RC4 解密逻辑
                    // software.DecryptKey = ...;

                    if (string.IsNullOrEmpty(software.SecretKey))
                    {
                        _res.Msg = $"File:{software.AccessoryFileName}, Decrypted data is empty.";
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    _res.Msg = $"JSON parsing error: {ex.Message}";
                    return -1;
                }
            }
            return 0;
        }

        // 修改：同步方法
        public static int GetConfigFileByProductSN(GetConfigFileByProductSN_Params _params, GetBomInfoByBoardSN_Res _res)
        {
            Logger.Log("开始获取传票号", LOGTOWINDOW);
            HttpInfo httpInfo = new HttpInfo
            {
                url = Constants.TisGetConfigFileByProductSN,
                ip = _params.Ip,
                port = _params.Port
            };

            // 这里需要实现 RC4 加密逻辑
            // _params.Password = ...;
            string encPassword = "";
            string encRes = IRC4_EncryptData(_params.Password, ref encPassword);
            if (!string.IsNullOrEmpty(encRes))
                Logger.Log(encRes);
            Logger.Log("密码加密："+encPassword);
            _params.Password = encPassword;

            string post = _params.ToQueryString();

            // 修改：同步调用
            var (status, retMsg) = IFHttp.HttpPostRequest(httpInfo, post);
            if (status != 0)
            {
                try
                {
                    JObject obj = JObject.Parse(retMsg);
                    _res.Msg = obj["Description"].ToString();
                    return -1;
                }
                catch (Exception ex)
                {
                    _res.Msg = $"JSON parsing error: {ex.Message}";
                    return -1;
                }
            }

            try
            {
                JObject obj = JObject.Parse(retMsg);
                _res.SummonsNumber = obj["SucceedDescription"].ToString();
                Logger.Log("获取到传票号：" + _res.SummonsNumber, LOGTOWINDOW);
                return 0;
            }
            catch (Exception ex)
            {
                _res.Msg = $"JSON parsing error: {ex.Message}";
                return -1;
            }
        }

        public static (int StatusCode, string ErrMsg) UploadProductDebugLog(ProductDebugLogUpload_Params _params)
        {
            Logger.Log("开始上传数据", LOGTOWINDOW);
            HttpInfo httpInfo = new HttpInfo
            {
                url = Constants.TisProductDebugLogUpload,
                ip = _params.Ip,
                port = _params.Port
            };

            // 这里需要实现 RC4 加密逻辑
            string encPassword = "";
            string encRes = IRC4_EncryptData(_params.Password, ref encPassword);
            if (!string.IsNullOrEmpty(encRes))
            {
                Logger.Log(encRes);
            }
            
            Logger.Log("密码加密：" + encPassword);
            _params.Password = encPassword;
            string post = _params.ToQueryString();

            var (status, retMsg) = IFHttp.HttpPostRequest(httpInfo, post);
            Logger.Log(retMsg);
            if (status != 0)
            {
                try
                {
                    JObject obj = JObject.Parse(retMsg);
                    return (-1, obj["Description"].ToString());
                }
                catch (Exception ex)
                {
                    return (-1, $"JSON parsing error: {ex.Message}");
                }
            }
            Logger.Log("数据上传成功：\n" + _params.Logs, LOGTOWINDOW);
            return (0, "");
        }

        // 修改：同步方法，使用元组返回结果
        public static (int StatusCode, string ErrMsg) UploadProcessId(TISWipTrackEx_Params _params)
        {
            Logger.Log("开始上传工序id", LOGTOWINDOW);
            HttpInfo httpInfo = new HttpInfo
            {
                url = Constants.TisWipTrackEx,
                ip = _params.Ip,
                port = _params.Port
            };

            // 这里需要实现 RC4 加密逻辑
            string encPassword = "";
            string encRes = IRC4_EncryptData(_params.Password, ref encPassword);
            if (!string.IsNullOrEmpty(encRes))
                Logger.Log(encRes);
            Logger.Log("密码加密：" + encPassword);
            _params.Password = encPassword;
            string post = _params.ToQueryString();

            var (status, retMsg) = IFHttp.HttpPostRequest(httpInfo, post);
            if (status != 0)
            {
                try
                {
                    JObject obj = JObject.Parse(retMsg);
                    return (-1, obj["Description"].ToString());
                }
                catch (Exception ex)
                {
                    return (-1, $"JSON parsing error: {ex.Message}");
                }
            }
            Logger.Log("工序id上传成功,id:"+ _params.WorkProcessID, LOGTOWINDOW);
            return (0, "");
        }
    }

    public struct TisProduceWebService_Params
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string ApiName { get; set; }
        public string BoardSN { get; set; }
        public string UkeyID { get; set; }
        public string Token { get; set; }

        public string ToQueryString()
        {
            // 实现转换为查询字符串的逻辑
            return $"apiName={ApiName}&boardSN={BoardSN}&ukeyID={UkeyID}&token={Token}";
        }
    }

    public class GetBomInfoByBoardSN_Res
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public int Esim { get; set; }
        public string SummonsNumber { get; set; }
        public string MaterialCode { get; set; }
        public string BdtName { get; set; }
        public List<SoftwareInfo> SoftwareList { get; set; }
        public string Msg { get; set; }

        public void Clear()
        {
            Ip = "";
            Port = "";
            Esim = 0;
            SummonsNumber = "";
            MaterialCode = "";
            BdtName = "";
            SoftwareList = new List<SoftwareInfo>();
        }
    }

    public class SoftwareInfo
    {
        public string AccessoryCode { get; set; }
        public string AccessoryFileName { get; set; }
        public string LocalFileName { get; set; }
        public string MaterialTypeCode { get; set; }
        public string EncrytedMD5 { get; set; }
        public long AccessorySize { get; set; }
        public string Path { get; set; }
        public string DecryptPath { get; set; }
        public string UnzipPath { get; set; }
        public string SecretKey { get; set; }
        public string DecryptKey { get; set; }
        public string DlURL { get; set; }
        public bool Ret { get; set; }

        public void Clear()
        {
            AccessoryCode = "";
            AccessoryFileName = "";
            LocalFileName = "";
            MaterialTypeCode = "";
            EncrytedMD5 = "";
            AccessorySize = 0;
            Path = "";
            SecretKey = "";
            DecryptKey = "";
            UnzipPath = "";
            DlURL = "";
            Ret = false;
        }
    }

    public class GetSoftwareDownloadURL_Params
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string FileName { get; set; }
    }

    public struct GetSoftwareSecretkey_Params
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string AccessoryFileName { get; set; }

        public string ToQueryString()
        {
            // 实现转换为查询字符串的逻辑
            return $"account={Account}&password={Password}&accessoryFileName={AccessoryFileName}";
        }
    }

    public struct ProductDebugLogUpload_Params
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string ProductSN { get; set; }
        public string SoftwareName { get; set; }
        public string EquipmentID { get; set; }
        public string SoftwareVer { get; set; }
        public string TestPattern { get; set; }
        public string Logs { get; set; }

        public string ToQueryString()
        {
            // 实现转换为查询字符串的逻辑
            return $"account={Account}&password={Password}&productSN={ProductSN}&softwareName={SoftwareName}&equipmentID={EquipmentID}&softwareVer={SoftwareVer}&testPattern={TestPattern}&logs={Logs}";
        }
    }

    public struct TISWipTrackEx_Params
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string SummonsNumber { get; set; }
        public string ProductSN { get; set; }
        public string WorkProcessID { get; set; }
        public string FaultBarCode { get; set; }
        public string FaultDescribe { get; set; }
        public string Remark { get; set; }
        public string LocalFileName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string BoardSN { get; set; }
        public string BoardCPUID { get; set; }
        public string IsFlowBind { get; set; }

        public string ToQueryString()
        {
            // 实现转换为查询字符串的逻辑
            return $"account={Account}&password={Password}&summonsNumber={SummonsNumber}&productSN={ProductSN}&workProcessID={WorkProcessID}&faultBarCode={FaultBarCode}&faultDescribe={FaultDescribe}&remark={Remark}&localFileName={LocalFileName}&startTime={StartTime}&endTime={EndTime}&boardSN={BoardSN}&boardCPUID={BoardCPUID}&isFlowBind={IsFlowBind}";
        }
    }
    public struct GetConfigFileByProductSN_Params
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string ProductSN { get; set; }
        public string MaterialTypeCode { get; set; }
        public string ToQueryString()
        {
            return $"account={Account}&password={Password}&productSN={ProductSN}&materialTypeCode={MaterialTypeCode}";
        }
    };
}
