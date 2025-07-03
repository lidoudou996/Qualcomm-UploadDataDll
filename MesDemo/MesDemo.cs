using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QC.QTMDotNetKernelInterface;
using MTMSDll;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using QC.QsprTestObjects;

namespace MesDemo
{

    public class LXYTestClass
    {
        public bool combine(string a, string b, out string c)
        {
            string sn = "";
            GlobalVariable.GetGlobalVariable("SN", out sn);
            c = a + b;
            DebugMessage.Write("LXYTest", "SN:" + sn, System.Diagnostics.TraceLevel.Verbose);
            DebugMessage.Write("LXYTest", "c:" + c, System.Diagnostics.TraceLevel.Verbose);
            return true;
        }
    }

    // 定义一个类来封装测试数据信息
    public class TestDataInfo
    {
        public string TestName { get; set; }
        public string DataSetCollectionName1 { get; set; }
        public string DataName1 { get; set; }
        public string DataSetCollectionName2 { get; set; }
        public string DataName2 { get; set; }
    }

    [QTMTestClass]
    public class MesDemoClass
    {
        public string _logFolderPath = @"C:\Qualcomm\MesDemoLog";
        public string _logFileName_UploadData = "UploadDataLog.log";
        public string _logFileName_UploadProceedId = "UploadProceedIdLog.log";

        public bool UploadData()
        {
            string currentDate = DateTime.Now.ToString("yyyy_MM_dd");
            //防止多次运行时创建嵌套文件夹
            _logFolderPath = @"C:\Qualcomm\MesDemoLog";
            _logFolderPath = Path.Combine(_logFolderPath, currentDate);
            Logger.Initialize(_logFolderPath, _logFileName_UploadData);

            string sn = "";
            GlobalVariable.GetGlobalVariable("SN", out sn);

            //获取MES系统信息
            Ini Settings = new Ini();
            Settings.load(@"C:\Databases\UploadDataConfig.ini");
            string User = Settings.getValue("UserName");
            string Ip = Settings.getValue("IP");
            string Port = Settings.getValue("Port");
            string Password = Settings.getValue("Password");
            string content = User + "\n" + Password + "\n" + Ip + "\n" + Port + "\n"
                + "产品序列号：" + sn + "\n";
            Logger.Log(content);

            //获取xml文件路径，所有log文件夹C:\Qualcomm\Log，
            //具体xttlog文件夹规则：用户名-日期-xtt名称  例：LIXY1-2025_06_24-Qualcomm_P30_EM_250624
            string baseDir = @"C:\Qualcomm\Log";
            string username = Environment.MachineName.Trim();
            string treePath = "";
            GlobalVariable.GetGlobalVariable("TreeFileName", out treePath);
            string xttName = Path.GetFileNameWithoutExtension(treePath);
            string targetDir = baseDir + $"\\{username}-{currentDate}-{xttName}";
            Logger.Log("目标文件夹："+targetDir);

            string xmlPath= FindAndParseXml.FindLatestXml(targetDir,sn);
            if (string.IsNullOrEmpty(xmlPath))
            {
                Logger.Log("未找到数据xml文件，停止上传");
                return false;
            }

            // 获取需要上传的数据信息
            string dataLog = "";            
            List<TestDataInfo> testDataInfos_NET = new List<TestDataInfo>();
            List<TestDataInfo> testDataInfos_GPS = new List<TestDataInfo>();

            for (int i = 1; i <= 10; i++)
            {
                string testName = Settings.getValue($@"{i}\TestName");
                if (string.IsNullOrEmpty(testName))
                    break;

                string dataSetCollectionName1 = Settings.getValue($@"{i}\1\DataSetCollectionNames");
                string dataName1 = Settings.getValue($@"{i}\1\DataName");
                string dataSetCollectionName2 = Settings.getValue($@"{i}\2\DataSetCollectionNames");
                string dataName2 = Settings.getValue($@"{i}\2\DataName");

                var testDataInfo = new TestDataInfo
                {
                    TestName = testName,
                    DataSetCollectionName1 = dataSetCollectionName1,
                    DataName1 = dataName1,
                    DataSetCollectionName2 = dataSetCollectionName2,
                    DataName2 = dataName2
                };
                // 根据testName内容判断应该添加到哪个列表
                if (testName.Contains("GSM") || testName.Contains("WCDMA") || testName.Contains("LTE"))
                {
                    testDataInfos_NET.Add(testDataInfo);
                    content = $"从UploadDataConfig.ini中获取到NET数据格式：\n {i}\\{testName}\n";
                }
                else if (testName.Contains("GPS"))
                {
                    testDataInfos_GPS.Add(testDataInfo);
                    content = $"从UploadDataConfig.ini中获取到GPS数据格式：\n {i}\\{testName}\n";
                }
                else
                {
                    //处理不匹配任何条件的情况
                    Logger.Log($"警告：未识别的测试类型 {testName}，不添加到任何集合");
                    continue;
                }
                
                content += $"{i}\\1\\{dataSetCollectionName1}\n" +
                          $"{i}\\1\\{dataName1}\n" +
                          $"{i}\\2\\{dataSetCollectionName2}\n" +
                          $"{i}\\2\\{dataName2}\n";
                Logger.Log(content);
            }
            if(testDataInfos_NET.Count > 0)
                FindAndParseXml.ParseXml_NET(ref dataLog, xmlPath, testDataInfos_NET);
            if(testDataInfos_GPS.Count > 0)
                FindAndParseXml.ParseXml_GPS(ref dataLog, xmlPath, testDataInfos_GPS);

            Logger.Log("找到数据如下：\n"+dataLog);

            if(string.IsNullOrEmpty(dataLog))
            {
                Logger.Log("数据为空，停止上传");
                return false;
            }

            //上传数据
            /*输入参数：string account(用户名), string password(密码)， string productSN（整机序列号），string equipmentID（工装ID）,string softwareName（配置文件名称）,string softwareVer（配置文件版本）, string testPattern（测试模式）, string logs（日志信息）
            测试结果为布尔型，0：表示失败，1：表示成功。
            测试日期时间格式：yyyy - MM - dd hh: mm: ss
              备注：客户端提交的logs可以是多条记录组成，每条记录之间以半角“$”间隔，单条记录内各属性之间以“@”间隔；格式如下：
            测试日期时间 @测试项名称@测试指标 @测试记录@0（测试结果0：失败 1：成功）$测试日期时间1 @测试项名称1@测试指标1 @测试记录1@1（测试结果0：失败 1：成功）
            */
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string uploadLog = $"{currentTime}@RF Test@@{dataLog}@0";
            ProductDebugLogUpload_Params param = new ProductDebugLogUpload_Params
            {
                Ip = Ip,
                Port = Port,
                Account = User,
                Password = Password,
                ProductSN = sn,
                Logs = uploadLog
            };

            int statusCode;
            string errMsg;
            (statusCode, errMsg) = STHttp.UploadProductDebugLog(param);
            if(statusCode != 0)
            {
                Logger.Log("上传数据失败，失败原因：" + errMsg);
                return false;
            }

            return true;
        }

        public bool UploadProceedId()
        {
            string currentDate = DateTime.Now.ToString("yyyy_MM_dd");
            //防止多次运行时创建嵌套文件夹
            _logFolderPath = @"C:\Qualcomm\MesDemoLog";
            _logFolderPath = Path.Combine(_logFolderPath, currentDate);
            Logger.Initialize(_logFolderPath, _logFileName_UploadProceedId);

            string sn = "";
            GlobalVariable.GetGlobalVariable("SN", out sn);

            //获取MES系统信息
            Ini Settings = new Ini();
            Settings.load(@"C:\Databases\UploadDataConfig.ini");
            string User = Settings.getValue("UserName");
            string Ip = Settings.getValue("IP");
            string Port = Settings.getValue("Port");
            string Password = Settings.getValue("Password");
            string content = User + "\n" + Password + "\n" + Ip + "\n" + Port + "\n"
                + "产品序列号：" + sn + "\n";
            Logger.Log(content);

            GetConfigFileByProductSN_Params paramGetSummon = new GetConfigFileByProductSN_Params
            {
                Ip = Ip,
                Port = Port,
                Account = User,
                Password = Password,
                ProductSN = sn
            };

            GetBomInfoByBoardSN_Res resGetSummon = new GetBomInfoByBoardSN_Res();
            int res = STHttp.GetConfigFileByProductSN(paramGetSummon, resGetSummon);
            if(res != 0)
            {
                Logger.Log("获取传票号失败：" + resGetSummon.Msg);
                return false;
            }

            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            TISWipTrackEx_Params paramUploadId = new TISWipTrackEx_Params
            {
                Ip = Ip,
                Port = Port,
                Account = User,
                Password = Password,
                ProductSN = sn,
                SummonsNumber = resGetSummon.SummonsNumber,
                WorkProcessID = "da91867f-4b67-4e72-ab35-495d2b62afe3",
                StartTime = currentTime,
                EndTime = currentTime,
                IsFlowBind = "1"
            };

            int statusCode;
            string errMsg;
            (statusCode, errMsg) = STHttp.UploadProcessId(paramUploadId);
            if(statusCode != 0)
            {
                Logger.Log("上传工序id失败：" + errMsg);
                return false;
            }

            return true;
        }
    }

    public class FindAndParseXml
    {
        static public void ParseXml_NET(ref string log, string xmlPath, List<TestDataInfo> testDataInfos)
        {
            //var logger = new Logger(_logFolderPath, _logFileName);
            Logger.Log("开始解析NET数据");

            // 加载 XML 文件
            XDocument xmlDoc = XDocument.Load(xmlPath);

            if (testDataInfos.Count < 1)
                Logger.Log("数据配置信息异常，请检查C:\\Databases\\UploadDataConfig.ini");

            // 循环获取符合要求的数据 - 根据索引位置一一对应
            int i = 0;
            foreach (var testDataInfo in testDataInfos)
            {
                i++;
                string testName = testDataInfo.TestName;
                string dataSetCollectionName1 = testDataInfo.DataSetCollectionName1;
                string diName1 = testDataInfo.DataName1;
                string dataSetCollectionName2 = testDataInfo.DataSetCollectionName2;
                string diName2 = testDataInfo.DataName2;

                string content = $"第{i}次查找：\n  {testName}:{dataSetCollectionName1}:{diName1}" +
                    $"\n  {testName}:{dataSetCollectionName2}:{diName2}";
                Logger.Log(content);

                //1为Band信息，2为power信息
                var bands = xmlDoc.Descendants("Test")
                                   .Where(t => (string)t.Element("Name") == testName)
                                   .Descendants("DataSetCollection")
                                   .Where(dsc => (string)dsc.Element("Name") == dataSetCollectionName1)
                                   .Descendants("DI")
                                   .Where(di => (string)di.Element("N") == diName1)
                                   .ToList();

                var valueResults = xmlDoc.Descendants("Test")
                    .Where(t => (string)t.Element("Name") == testName)
                    .Descendants("DataSetCollection")
                    .Where(dsc => (string)dsc.Element("Name") == dataSetCollectionName2)
                    .Descendants("Result")
                    .Where(r => r.Descendants("DI").Any(di => (string)di.Element("N") == diName2))
                    .ToList();

                content = "";
                for (int j = 0; j < Math.Min(bands.Count, valueResults.Count); j++)
                {
                    //string value = values[j].Element("V")?.Value ?? "未找到值";
                    var result = valueResults[j];

                    // 获取AvgTxPwr值
                    string value = result.Descendants("DI")
                        .FirstOrDefault(di => (string)di.Element("N") == diName2)?
                        .Element("V")?.Value ?? "未找到值";

                    // 获取Status值
                    string status = result.Descendants("PassFail")
                        .Descendants("DI")
                        .FirstOrDefault(di => (string)di.Element("N") == "Status")?
                        .Element("V")?.Value ?? "未找到值";
                    string band = bands[j].Element("V")?.Value ?? "未找到值";
                    //string status = statuss[j].Element("V")?.Value ?? "未找到值";
                    log += $"{testName}, {diName1}_{band},{diName2}_{value},{status};\n";
                    content += $"找到数据：{testName}, {diName1}_{band},{diName2}_{value},{status};\n";
                }

                if (bands.Count == 0 && valueResults.Count == 0)
                {
                    content += $"未找到{diName1}和{diName2}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}:{dataSetCollectionName2}:{diName2}";
                }
                else if (valueResults.Count == 0)
                {
                    content += $"未找到{diName2}的匹配值 {testName}:{dataSetCollectionName2}:{diName2}";
                }
                else if (bands.Count == 0)
                {
                    content += $"未找到{diName1}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}";
                }
                else if (valueResults.Count != bands.Count)
                {
                    content += $"  警告: {diName1}和{diName2}的匹配数量不一致 ({diName1}={bands.Count}, {diName1}={valueResults.Count})";
                }
                Logger.Log(content);
            }
        }

        static public void ParseXml_GPS(ref string log, string xmlPath, List<TestDataInfo> testDataInfos)
        {
            Logger.Log("开始解析GPS数据");

            // 加载 XML 文件
            XDocument xmlDoc = XDocument.Load(xmlPath);

            if (testDataInfos.Count < 1)
                Logger.Log("数据配置信息异常，请检查C:\\Databases\\UploadDataConfig.ini");

            // 循环获取符合要求的数据 - 根据索引位置一一对应
            int i = 0;
            foreach (var testDataInfo in testDataInfos)
            {
                i++;
                string testName = testDataInfo.TestName;
                string dataSetCollectionName1 = testDataInfo.DataSetCollectionName1;
                string diName1 = testDataInfo.DataName1;
                string dataSetCollectionName2 = testDataInfo.DataSetCollectionName2;
                string diName2 = testDataInfo.DataName2;

                string content = $"第{i}次查找：\n  {testName}:{dataSetCollectionName1}:{diName1}" +
                    $"\n  {testName}:{dataSetCollectionName2}:{diName2}";
                Logger.Log(content);

                //1为Band信息，2为power信息
                var freqs = xmlDoc.Descendants("Test")
                                   .Where(t => (string)t.Element("ExtendedName") == testName)
                                   .Descendants("DataSetCollection")
                                   .Descendants("Result")
                                   //.Where(dsc => (string)dsc.Element("Name") == dataSetCollectionName1)
                                   .Descendants("DI")
                                   .Where(di => (string)di.Element("N") == diName1)
                                   .ToList();


                // 获取测试值数据及其所在Result中的Status
                var valueResults = xmlDoc.Descendants("Test")
                    .Where(t => (string)t.Element("ExtendedName") == testName)
                    .Descendants("DataSetCollection")
                    //.Where(dsc => (string)dsc.Element("Name") == dataSetCollectionName2)
                    .Descendants("Result")
                    .Where(r => r.Descendants("DI").Any(di => (string)di.Element("N") == diName2))
                    .ToList();

                content = "";
                for (int j = 0; j < Math.Min(freqs.Count, valueResults.Count); j++)
                {
                    //string value = values[j].Element("V")?.Value ?? "未找到值";
                    var result = valueResults[j];

                    // 获取Power值
                    string value = result.Descendants("DI")
                        .FirstOrDefault(di => (string)di.Element("N") == diName2)?
                        .Element("V")?.Value ?? "未找到值";

                    // 获取Status值
                    string status = result.Descendants("PassFail")
                        .Descendants("DI")
                        .FirstOrDefault(di => (string)di.Element("N") == "Status")?
                        .Element("V")?.Value ?? "未找到值";

                    string freq = freqs[j].Element("V")?.Value ?? "未找到值";
                    log += $"{testName}, {diName1}_{freq},{diName2}_{value},{status};\n";
                    content += $"{testName}, {diName1}_{freq},{diName2}_{value},{status};\n";
                }

                if (freqs.Count == 0 && valueResults.Count == 0)
                {
                    content += $"未找到{diName1}和{diName2}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}:{dataSetCollectionName2}:{diName2}";
                }
                else if (valueResults.Count == 0)
                {
                    content += $"未找到{diName2}的匹配值 {testName}:{dataSetCollectionName2}:{diName2}";
                }
                else if (freqs.Count == 0)
                {
                    content += $"未找到{diName1}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}";
                }
                else if (valueResults.Count != freqs.Count)
                {
                    content += $"  警告: {diName1}和{diName2}的匹配数量不一致 ({diName1}={freqs.Count}, {diName1}={valueResults.Count})";
                }
                Logger.Log(content);
            }
        }

        static public string FindLatestXml(string targetDir, string sn)
        {
            //var logger = new Logger(_logFolderPath, _logFileName);
            string xmlPath = "";
            try
            {
                // 获取当前时间和两分钟前的时间点
                DateTime now = DateTime.Now;
                DateTime twoMinutesAgo = now.AddMinutes(-2);

                // 获取指定文件夹中的所有HTML文件
                DirectoryInfo directory = new DirectoryInfo(targetDir);
                FileInfo[] htmlFiles = directory.GetFiles($"*{sn}*.xml");

                // 筛选出修改时间在两分钟以内的文件
                FileInfo[] recentFiles = Array.FindAll(htmlFiles,
                    file => file.LastWriteTime >= twoMinutesAgo && file.LastWriteTime <= now);

                if (recentFiles.Length > 0)
                {
                    // 按修改时间排序，最新的文件排在前面
                    Array.Sort(recentFiles, (a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

                    // 获取最新的文件
                    FileInfo newestFile = recentFiles[0];
                    xmlPath = newestFile.FullName;

                    Logger.Log($"找到修改时间在两分钟以内的最新xml文件：{newestFile.FullName}");
                }
                else
                {
                    Logger.Log($"在指定文件夹中未找到修改时间在两分钟以内，序列号为{sn}的xml文件。");
                }
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Log("错误：指定的文件夹不存在。");
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Log("错误：没有权限访问该文件夹或文件。");
            }
            catch (Exception ex)
            {
                Logger.Log($"发生错误：{ex.Message}");
            }
            return xmlPath;
        }
    }


}
