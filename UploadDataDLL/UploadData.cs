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

namespace UploadData
{


    [QTMTestClass]
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
    public class UploadDataClass
    {
        public string _logFolderPath = @"C:\Qualcomm\UploadDataLog";
        public string _logFileName = "UploadDataLog.log";
        Logger logger;

        public bool UploadData()
        {
            //获取MES系统信息
            Ini Settings = new Ini();
            Settings.load(@"C:\Databases\UploadDataConfig.ini");
            string mUser = Settings.getValue("UserName");
            string mIpAddress = Settings.getValue("IPAddress");
            string mPassword = Settings.getValue("Password");
            string sSummon = "";
            string mEncPassword = "";
            string startTime = "";
            string sn = "";
            GlobalVariable.GetGlobalVariable("SN", out sn);

            string currentDate = DateTime.Now.ToString("yyyy_MM_dd");
            _logFolderPath = Path.Combine(_logFolderPath, currentDate);
            logger = new Logger(_logFolderPath, _logFileName);
            string content = mUser + "\n" + mPassword + "\n" + mIpAddress + "\n"
                + "产品序列号："+ sn + "\n";
            logger.Log(content);            

            //获取xml文件路径，所有log文件夹C:\Qualcomm\Log，
            //具体xttlog文件夹规则：用户名-日期-xtt名称  例：LIXY1-2025_06_24-Qualcomm_P30_EM_250624
            string baseDir = @"C:\Qualcomm\Log";
            string username = Environment.MachineName.Trim();
            string treePath = "";
            GlobalVariable.GetGlobalVariable("TreeFileName", out treePath);
            string xttName = Path.GetFileNameWithoutExtension(treePath);
            string targetDir = baseDir + $"\\{username}-{currentDate}-{xttName}";
            logger.Log("目标文件夹："+targetDir);

            string xmlPath=FindLatestXml(targetDir,sn);            

            // 获取需要上传的数据信息
            string dataLog = "";            
            List<TestDataInfo> testDataInfos_NET = new List<TestDataInfo>();
            List<TestDataInfo> testDataInfos_GPS = new List<TestDataInfo>();

            for (int i = 1; i <= 100; i++)
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
                    content = $"NET数据 {i}\\{testName}";
                }
                else if (testName.Contains("GPS"))
                {
                    testDataInfos_GPS.Add(testDataInfo);
                    content = $"GPS数据 {i}\\{testName}";
                }
                else
                {
                    //处理不匹配任何条件的情况
                    logger.Log($"警告：未识别的测试类型 {testName}，不添加到任何集合");
                    continue;
                }
                
                logger.Log(content);
                content = $"{i}\\1\\{dataSetCollectionName1}\n" +
                          $"{i}\\1\\{dataName1}\n" +
                          $"{i}\\2\\{dataSetCollectionName2}\n" +
                          $"{i}\\2\\{dataName2}\n";
                logger.Log(content);
            }
            if(testDataInfos_NET.Count > 0)
                ParseXml_NET(ref dataLog, xmlPath, testDataInfos_NET);
            if(testDataInfos_GPS.Count > 0)
                ParseXml_GPS(ref dataLog, xmlPath, testDataInfos_GPS);

            logger.Log(dataLog);

            if(string.IsNullOrEmpty(dataLog))
            {
                logger.Log("数据为空，停止上传");
                //防止多次运行时创建嵌套文件夹
                _logFolderPath = @"C:\Qualcomm\UploadDataLog";
                return false;
            }
            //上传数据
            CMTMSDll mtms = new CMTMSDll();
            MTMSDll.ProduceWebService.Result Rst = new MTMSDll.ProduceWebService.Result();
            content = "create mtms&Rst obj";
            logger.Log(content);

            mtms._sAddressInput = mIpAddress;
            mtms._sEncPassword = mPassword;
            mtms._sUserName = mUser;
            content = "mtms登录:\n  " + mtms._sUserName + "\n  " + mtms._sEncPassword + "\n  " + mtms._sAddressInput + "\n";
            logger.Log(content);

            bool res = false;
            string RetMsg = "";
            res = mtms.EA_Login(ref RetMsg);
            if (res)
            { // login MES
                mEncPassword = mtms.mStrEncPassword;
                content = "mtms登录成功\n  " + mEncPassword;
                logger.Log(content);
            }
            else
            {
                mEncPassword = mtms.mStrEncPassword;
                content = "mtms登录失败\n 原因： " + RetMsg;
                logger.Log(content);
            }

            /*输入参数：string account(用户名), string password(密码)， string productSN（整机序列号），string equipmentID（工装ID）,string softwareName（配置文件名称）,string softwareVer（配置文件版本）, string testPattern（测试模式）, string logs（日志信息）
        测试结果为布尔型，0：表示失败，1：表示成功。
        测试日期时间格式：yyyy - MM - dd hh: mm: ss
              备注：客户端提交的logs可以是多条记录组成，每条记录之间以半角“$”间隔，单条记录内各属性之间以“@”间隔；格式如下：
        测试日期时间 @测试项名称@测试指标 @测试记录@0（测试结果0：失败 1：成功）$测试日期时间1 @测试项名称1@测试指标1 @测试记录1@1（测试结果0：失败 1：成功）
        */
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ET_UploadTestLogReq Req = new ET_UploadTestLogReq();

            Req.sUserName = mUser;
            Req.sPassword = mEncPassword;
            Req.sIP = mIpAddress;
            Req.sProductSN = sn;
            Req.sEquipmentID = "";
            Req.sSoftwareName = "";
            Req.sSoftwareVer = "";
            Req.sTestPattern = "";
            Req.sLogs = currentDateTime + "@" + "RF Test" + "@@" + dataLog +"@";

            content = "create ET_UploadTestLogReq上传数据，信息如下：\n  " + Req.sUserName + "\n  " + mPassword + "\n  "
                + Req.sPassword + "\n  " + Req.sIP + "\n  " + Req.sProductSN + "\n  " + dataLog;
            logger.Log(content);
            //防止多次运行时创建嵌套文件夹
            _logFolderPath = @"C:\Qualcomm\UploadDataLog";

            if (mtms.EA_iProductDebugLogUpload(Req, ref Rst) == true)
            {
                if (Rst.IsOK == true)
                {
                    content = "Upload succeed";
                    logger.Log(content);
                    return true;
                }
                else
                {
                    content = "Upload failed\n reason:" + Rst.Description;
                    logger.Log(content);
                    return false;
                }
            }
            else
            {
                content = "UploadTestLogReq use failed";
                logger.Log(content);
                return false;
            }

            return true;
        }

        public void ParseXml_NET(ref string log, string xmlPath, List<TestDataInfo> testDataInfos)
        {
            //var logger = new Logger(_logFolderPath, _logFileName);
            logger.Log("开始解析NET数据");

            if (string.IsNullOrEmpty(xmlPath))
                return;

            // 加载 XML 文件
            XDocument xmlDoc = XDocument.Load(xmlPath);

            if (testDataInfos.Count < 1)
                logger.Log("数据配置信息异常，请检查C:\\Databases\\UploadDataConfig.ini");

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
                logger.Log(content);

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
                }

                if (bands.Count == 0 && valueResults.Count == 0)
                {
                    content = $"未找到{diName1}和{diName2}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}:{dataSetCollectionName2}:{diName2}";
                    logger.Log(content);
                }
                else if (valueResults.Count == 0)
                {
                    content = $"未找到{diName2}的匹配值 {testName}:{dataSetCollectionName2}:{diName2}";
                    logger.Log(content);
                }
                else if (bands.Count == 0)
                {
                    content = $"未找到{diName1}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}";
                    logger.Log(content);
                }
                else if (valueResults.Count != bands.Count)
                {
                    content = $"  警告: {diName1}和{diName2}的匹配数量不一致 ({diName1}={bands.Count}, {diName1}={valueResults.Count})";
                    logger.Log(content);
                }
            }
        }

        public void ParseXml_GPS(ref string log, string xmlPath, List<TestDataInfo> testDataInfos)
        {            
            logger.Log("开始解析GPS数据");
            if (string.IsNullOrEmpty(xmlPath))
                return;

            // 加载 XML 文件
            XDocument xmlDoc = XDocument.Load(xmlPath);
            
            if (testDataInfos.Count < 1)
                logger.Log("数据配置信息异常，请检查C:\\Databases\\UploadDataConfig.ini");

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
                logger.Log(content);

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
                }

                if (freqs.Count == 0 && valueResults.Count == 0)
                {
                    content = $"未找到{diName1}和{diName2}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}:{dataSetCollectionName2}:{diName2}";
                    logger.Log(content);
                }
                else if (valueResults.Count == 0)
                {
                    content = $"未找到{diName2}的匹配值 {testName}:{dataSetCollectionName2}:{diName2}";
                    logger.Log(content);
                }
                else if (freqs.Count == 0)
                {
                    content = $"未找到{diName1}的匹配值 {testName}:{dataSetCollectionName1}:{diName1}";
                    logger.Log(content);
                }
                else if (valueResults.Count != freqs.Count)
                {
                    content = $"  警告: {diName1}和{diName2}的匹配数量不一致 ({diName1}={freqs.Count}, {diName1}={valueResults.Count})";
                    logger.Log(content);
                }
            }
        }
        public string FindLatestXml(string targetDir, string sn)
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

                    logger.Log($"找到修改时间在两分钟以内的最新xml文件：{newestFile.FullName}");
                }
                else
                {                    
                    logger.Log("在指定文件夹中未找到修改时间在两分钟以内的xml文件。");
                }
            }
            catch (DirectoryNotFoundException)
            {
                logger.Log("错误：指定的文件夹不存在。");
            }
            catch (UnauthorizedAccessException)
            {
                logger.Log("错误：没有权限访问该文件夹或文件。");
            }
            catch (Exception ex)
            {
                logger.Log($"发生错误：{ex.Message}");
            }
            return xmlPath;
        }
    }


}
