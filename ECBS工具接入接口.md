# ECBS工具接入接口文档

## 更新历史

    * 2025-04-21 创建文档 吴木荣
    * 2025-06-19 统一生产接口请求参数去掉账号密码 吴木荣
    * 2025-06-19 新增烧片激活专用接口 吴木荣
    * 2025-07-10 新增用户密码修改（updatePassword）接口 林杰
    * 2025-07-15 新增单用户登录（singleUserLogin）接口 林杰
    * 2025-07-21 优化reworkSnCollect接口数据操作说明 吴木荣

## 全局约定

    * $HOST 代表子系统的地址
    * json结果集合需写明类型，如含多类型用“||”进行分隔  eg:"str||null" 表示可能为null和 string类型
    * 类型缩写如下：
    * str : string
    * int : integer
    * dec : bigDecimal
    * bool : boolean
    * dbl : double
    * file : file文件流
    * [] : array
    * {} : dataect

## 接口返回消息形式(JSON)

```json
{
     "code":"int",//执行结果，成功200，失败99999
     "msg":"str",//结果说明
     "data":"{}||null",//结果返回数据
     "stateCodeId":"int"//状态参数标识属性
}
```

---

## 工具接入接口

### 登录

#### 1、统一登录接口

##### 统一登录接口地址
`POST` 

```
$HOST/ecbs/api/v1/auth/execute
```
##### 统一登录接口请求参数

```json
{
    "account":"str",//帐号
    "password": "str",//密码
    "token": "str",//token
    "apiName":"str",//接口名称
    "jsonValue":"str",//请求json
}
```

```
帐号密码和token必须存在一个并进行校验
```

##### 统一登录接口成功返回结果

```json
{
    "code":200,
    "msg":"成功",
    "data":{ }//接口应答数据
}
```

---

#### 2、API

##### getUserInfo

```
获取用户信息
```

###### 请求jsonValue

```json
{
    "account":"str"//帐号
}
```

###### 应答data

```json
{
    "username":"str",//帐号
    "nickname":"str"//昵称
}
```

---

##### unifiedLogin

```
ecbs帐号登录
```

###### 请求jsonValue
```json
{
    "account":"str",//帐号
    "authAccount":"str",//授权员帐号
    "authAccountPwd":"str",//授权员密码
    "remark":"str"//备注
}
```
###### 应答data
```json
{
    "userPermission": [
        {
            "userId":"str",//用户ID
            "username":"str",//帐号
            "roleName":"str",//角色名称
            "moduleId":"str",//模块ID
            "permissionCodeValue":"str",//权限值
            "moduleName":"str",//模块名称
            "moduleDllName":"str"//模块DLL名称
        }
    ],//权限列表
    "initInfo": [
        {
            "id":"str",//时区ID
            "baseUtcOffset":"int",//时差
            "displayName":"str",//时区显示名称
            "serviceTime":"str",//服务器时间
            "utcTime":"str",//UTC时间
            "city":"str",//城市
            "gateWayAndTerminalAddress":"str",//终端网关建链地址
            "gateWayAndOneWayAddress":"str",//TLS网关单向
            "gateWayAndTwoWayAddress":"str",//TLS网关双向
            "errorCodeSystemAddress":"str",//错误码地址
            "factoryCode":"str"//工厂代码
        }
    ],//初始化信息
    "operatorExpiredTime": "2025-10-02 14:00:25",//操作员密码过期时间
    "authExpiredTime": "2025-10-08 20:07:43",//授权员密码过期时间
    "jks":"str"//jks文件内容(加密)
}
```

**应答字段说明**：

| 字段             | 说明                                      |
| ---------------- | ----------------------------------------- |
| `userPermission` | 用户ID,帐号,角色名,模块ID,模块名称,权限值 |
| `initInfo`       | 初始化信息                                |

---

##### updatePassword

```
修改密码
```

###### 请求jsonValue

```json
{
    "newPassword":"str"//新密码（需要加密）
}
```

###### 应答data

```json
{
}
```

---

##### singleUserLogin

```
单用户登录
```

###### 请求jsonValue

```json
{
}
```

###### 应答data

```json
{
    "userPermission": [
        {
            "userId":"str",//用户ID
            "username":"str",//帐号
            "roleName":"str",//角色名称
            "moduleId":"str",//模块ID
            "permissionCodeValue":"str",//权限值
            "moduleName":"str",//模块名称
            "moduleDllName":"str"//模块DLL名称
        }
    ]
}
```



### 生产

#### 1、统一生产接口

##### 统一生产口地址
`POST` 

```
$HOST/ecbs/api/v1/produce/execute
```
##### 统一生产接口请求参数

```json
{
    "token": "str",//token
    "apiName":"str",//接口名称
    "jsonValue":"str",//请求json
}
```

##### 统一生产接口成功返回结果

```json
{
    "code":200,
    "msg":"成功",
    "data":{ }//接口应答数据
}
```

---

#### 2、API

##### checkProductSNValidityBySummonsNumber

```
验证整机SN有效性
```

###### 请求jsonValue

```json
{
    "productSN": "str",//整机SN
    "summonsNumber": "str"//传票号
}
```

###### 应答data

```json
{
  
}
```

---

##### productDebugLogUpload

```
整机调试日志批量上传接口
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "productSN": "str",//整机SN
    "equipmentID": "str",//工装ID
    "softwareName": "str",//配置文件名称
    "softwareVer": "str",//配置文件版本
    "testPattern": "str",//测试模式
    "logs": "str"//调试日志
}
```
###### 应答data
```json
{
    
}
```

---

##### checkCPUIDAndGetProductInfo

```
通过cpuid和summonsNumber获取整机SN
对应：
CheckCPUIDAndGetProductSNAndGetSummonsNumberWithoutProcess
CheckCPUIDAndGetProductSNWithoutProcess
```

###### 请求jsonValue
```json
{
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    "productSN": "str",//整机SN
    "summonsNumber": "str"//传票号
}
```

---

##### boardDebugLogBatchUpload

```
板卡调试烧片日志批量上传接口
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "boardSN": "str",//主板SN
    "equipmentID": "str",//工装ID
    "configFileName": "str",//配置文件名称
    "configFileVersion": "str",//配置文件版本
    "checkSum": "str",//校验和
    "testPattern": "str",//测试模式
    "testFrequency": "int",//测试轮数
    "logs": "str"//调试日志
}
```
###### 应答data
```json
{
    
}
```


**应答字段说明**：

| 字段           | 说明                                                         |
| -------------- | ------------------------------------------------------------ |
| `logs` | 每个测试结果数据之间使用$分隔，测试结果数据由五个子数据组成，由@分隔，每个子数据表示内容为："时间戳@测试项名称@测试指标@测试记录@测试结果"。示例："2020-05-28 11:06:37.000@GATS-触屏@@@1$2020-05-18 13:30:01.000@测试结束@@37 s;Config File Sign:Chenhs;@1" |

---

##### tisWipTrackEx

```
TIS生产采集动作
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "summonsNumber": "str",//传票号
    "productSN": "str",//整机SN
    "workProcessID": "str",//工序ID
    "faultBarCode": "str",//故障代码,多个代码之间以半角逗号间隔
    "faultDescribe": "str",//故障现象描述
    "remark": "str",//备注
    "localFileName": "str",//下载本地文件名称；为空表示为本地导入，不增加软件下载日志及SN与软件对应关系
    "startTime": "str",//起始下载时间
    "endTime": "str",//结束下载时间
    "boardSN": "str",//主板SN
    "boardCPUID": "str",//主板cpuid
    "isFlowBind": "int",//备注
}
```
###### 应答data
```json
{
    
}
```

---

##### checkProductSNValidity

```
验证整机SN有效性
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "summonsNumber": "str",//传票号
    "workProcessID": "str" //工序id
}
```
###### 应答data
```json
{
    
}
```



---

##### getIMEIAndMac

```
获取IMEI号与mac地址
```

###### 请求jsonValue
```json
{
    "productSN": "str"//整机SN
}
```
###### 应答data
```json
{
    "imeiAndMacList":"str"//IMEI/MAC列表
}
```

**应答字段说明**：

| 字段             | 说明                                                         |
| ---------------- | ------------------------------------------------------------ |
| `imeiAndMacList` | "分群码1,值1;分群码2,值2..."，例"YM04,990017747819171;YM06,4C-91-57-AD-5C-59;YM05,4C-91-57-B5-B8-4E;YM01,868632058817488" |

---

##### getSoftNameListByBoardSN

```
通过主板SN获取对应传票的软件下载列表（板卡烧片）
```

###### 请求jsonValue
```json
{
    "boardSN": "str"//主板SN
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "burningFileName":"str",//烧片文件bdt名称
    "manufacturerTypeID":"int",//制造商标识[0联迪，1ODM-WPay]
    "softwareList":"str"//软件列表
}
```

**应答字段说明**：

| 字段           | 说明                                                         |
| -------------- | ------------------------------------------------------------ |
| `softwareList` | "[软件料号] 软件名称,本地存储文件名称,软件大小,软件分群码,加密后文件MD5校验码;..."，例"[SS62000010] 板卡AP测试程序文件-P-系列-板卡-V2.5.12-250307,92056f1e-0172-405d-ae7a-6e1e3c720581,5865532,SF62,E2FB09D3190875871EFF4FC7D8867E4F;[SS04000170] 生产烧片-P20-SE-Build20241113-ECC,84e7d60e-087d-40f8-9492-9b9cd4b94d1f,306364,SF03,511FBDD46F734A592D4E9E1DC25FDCE3;[SS61000014] SE特殊CTRL-P20-RBLVCTRL-build20240716-ECC,701af6ac-bb11-4ce9-8c38-26f72a31b600,161721,SF61,7DEA4659F73169B64D168372B6790487;[SS97000001] P20高通镜像烧片-AN-GTW-P00-1.1.0MM-20250114-00,cdd89916-4149-4a73-9e6f-f25248ff0eef,866797228,SF97,BA051FE92012E18B7339A37A02EC5E7B;[SS03000650] SW-GPES-P20-ME2SY1LEWSA2SI2CH5PW1-V1.0-240809,726deed4-c721-449a-9f3f-927b1d861562,1828,SF02,18D47AE0B17B5FC30FEE69905B04B473" |

---

##### getSoftwareDownloadURL

```
获取文件下载路径
```

###### 请求jsonValue
```json
{
    "fileName": "str",//文件名
    "fileType": "int"//文件类型[0终端软件；1校准文件；2谷歌KEY文件；3霍尼韦尔解码库文件]
}
```
###### 应答data
```json
{
    "fileUrl":"str"//文件下载url
}
```

---

##### checkDebugBoardSNValidity

```
验证调试段主板SN有效性
```

###### 请求jsonValue
```json
{
    "boardSN": "str",//主板SN
    "summonsNumber": "str"//传票号
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "materialCode": "str"//料号
}
```

---

##### getBdtNameByBoardSNAndCoreID

```
通过主板SN获取烧片Bdt命名（板卡烧片）
```

###### 请求jsonValue
```json
{
    "boardSN": "str",//主板SN
    "coreID": "str||null"//校准/备份文件ID
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "burningFileName":"str",//烧片文件bdt名称
    "materialCode": "str",//料号
}
```

---

##### coreFileIsExist

```
判断CoreID是否存在
```

###### 请求jsonValue
```json
{
    "coreID": "str"//校准/备份文件ID
}
```
###### 应答data
```json
{
    
}
```

---

##### checkCpuidIsExist

```
判断主板对应的cpuid是否存在
```

###### 请求jsonValue
```json
{
    "boardSN": "str"//主板SN
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "materialCode": "str"//料号
}
```

---

##### getBomInfoByBoardSN

```
通过主板SN获取对应传票的软件下载列表（板卡烧片）
```

###### 请求jsonValue
```json
{
    "boardSN": "str"//主板SN
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "materialCode": "str",//料号
    "burningFileName":"str",//烧片文件bdt名称
    "manufacturerTypeID":"int",//制造商标识[0联迪，1ODM-WPay]
    "softwareList":[
        {
            "accessoryCode":"str",//软件料号
            "accessoryFileName":"str",//软件名称
            "localFileName":"str",//本地存储文件名
            "materialTypeCode":"str",//软件分群码
            "accessorySize":"long",//软件大小
            "encrytedMD5":"str",//加密后文件MD5校验码
            "zh11":"str", //0-没有zh11  1-有zh11 
            "esim":"str",//0-没有esim  1-有esim
        }
    ]//软件列表
}
```

---

##### getProduceBurnSoftNameByBoardSN

```
通过主板SN获取对应的烧片文件名称及BDT名称(生产用成品维修烧片）
```

###### 请求jsonValue
```json
{
    "boardSN": "str"//主板SN
}
```
###### 应答data
```json
{
    "bdtName":"str",//烧片文件bdt名称
    "cpuid":"str",//cpuid
    "coreID":"str",//校准/备份文件ID
    "softwareList":[
        {
            "accessoryCode":"str",//软件料号
            "accessoryFileName":"str",//软件名称
            "localFileName":"str",//本地存储文件名
            "materialTypeCode":"str",//软件分群码
            "accessorySize":"long",//软件大小
            "encrytedMD5":"str",//加密后文件MD5校验码
            "type":"int"//类型[2文件]
        }
    ]//软件列表
}
```

---

##### checkBoardSNValidity

```
验证主板序列号的有效性
```

###### 请求jsonValue
```json
{
    "boardSN": "str"//主板SN
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
}
```

---

##### bindBoardSNCPUIDRelation

```
绑定主板序列号与主板特征码
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "boardSN": "str",//主板SN
    "boardCPUID": "str"//主板CPUID
}
```
###### 应答data
```json
{
    
}
```

---

##### bindCalibrationParameter

```
绑定校准参数
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "boardCPUID": "str",//主板CPUID
    "coreID": "str"//校准/备份文件ID
}
```
###### 应答data
```json
{
    
}
```

---

##### getBoardSNByCPUID

```
通过CPUID查询对应主板序列号
```

###### 请求jsonValue
```json
{
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    "boardSN":"str"//主板SN
}
```

---

##### getWorkProcessList

```
通过整机序列号获取对应的生产工序列表
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "materialCode": "str",//料号
    "workProcessList": "str"//工序列表
}
```

**应答字段说明**：

| 字段             | 说明                                                  |
| ---------------- | ----------------------------------------------------- |
| `workProcessList` | "工序ID,工序名称;..."，例"51FABF3B-AB19-4CFA-AF66-40F4CC25B4D7,TIS-烧片;31B6A63A-CBBE-43E5-9883-45B2EEB3FDA9,TIS-配置/补丁/校准;49777DEF-3FB4-49EE-A840-C8F3E6C8AEF1,SN下装终检;D54B55F2-EFCA-4165-AA13-36F2D81A9133,TIS-二次烧片;8A724265-7280-45A1-BF09-90CC13663914,TIS-POS模态转换;9DC24EB6-8359-4310-B557-7DBEDCB6C974,TIS-Launcher切换;283F4FDB-690C-40CC-9FA7-A071A6A07AAD,TIS-联网清攻击" |

---

##### getDataListByWorkProcessID

```
通过工序id获取数据
```

###### 请求jsonValue
```json
{
    "summonsNumber": "str",//传票号
    "workProcessID": "str"//工序ID
}
```
###### 应答data
```json
 {
      "newModelData":
            {
                "tagIDInfoList":[
                    {
                        "tagID":"str",//tagId
                        "tagValue":"str",//tag值
                        "downloadIndex":"int",//下载顺序
                        "channelType":"int",//通道类型[1安全通道,2运维通道]
                        "tagValueSetType":"int",//tagId值生成类型[0动态下载,1静态数据,2文件名称,3动态采集]
                        "systemClassURL":"str",//tag处理类
                        "tagValueType":"str",//tagValue类型[0-Int,1-String,2-HexToByte]
                        "terminalPlatformType":"str"//终端平台类型[SmartTerminal,LiteTerminal]
                    }
                ],//tag信息列表
                "disPlayList":[
                    {
                        "materialCode":"str",//料号
                        "materialName":"str",//物料名称
                        "downloadIndex":"int"//下载顺序
                    }
                ]//显示信息列表
            },//新模式数据
       "oldModelData":[
                {
                    "materialCode":"str",//料号/分群码
                    "materialName":"str",//物料/分群码名称
                    "materialValue":"str",//物料/分群码值
                    "materialTypeCode":"str",//物料分群码
                    "downloadIndex":"int",//下载顺序
                    "materialValueType":"int",//物料/分群码值生成类型[0动态下载,1静态数据,2文件名称,3动态采集]
                    "type":"int"//物料/分群码类型[0分群码 1料号]
                }
            ]//旧模式数据
        }
```

---

##### getDataListByProductSN

```
通过序列号获取数据
```

###### 请求jsonValue
```json
{
    "summonsNumber": "str",//传票号
    "workProcessID": "str",//工序ID
    "productSN": "str",//整机SN
    "boardCPUID": "str",//主板CPUID
    "isFlowBind": "str"//是否绑定流程[1流程绑定,0脱离流程]
}
```
###### 应答data
```json
{
    "tagIDInfoList":[
        {
            "tagID":"str",//tagId
            "tagValue":"str",//tag值
            "downloadIndex":"int",//下载顺序
            "channelType":"int",//通道类型[1安全通道,2运维通道]
            "tagValueSetType":"int",//tagId值生成类型[0动态下载,1静态数据,2文件名称,3动态采集]
            "systemClassURL":"str",//tag处理类
            "tagValueType":"str",//tagValue类型[0-Int,1-String,2-HexToByte]
            "terminalPlatformType":"str"//终端平台类型[SmartTerminal,LiteTerminal]
        }
    ],//新模式tag数据
    "softwaretypeCodeInfoList":[
        {
            "materialCode":"str",//料号/分群码
            "materialName":"str",//物料/分群码名称
            "materialValue":"str",//物料/分群码值
            "materialTypeCode":"str",//物料分群码
            "downloadIndex":"int",//下载顺序
            "materialValueType":"int",//物料/分群码值生成类型[0动态下载,1静态数据,2文件名称,3动态采集]
            "type":"int"//物料/分群码类型[0分群码 1料号]
        }
    ],//旧模式软件分群码数据
    "productInfoDic":{
        "boardSN":"str",//主板SN
        "productSN":"str",//整机SN
        "bdtName":"str",//烧片bdt文件名称
        "coreID":"str",//校准/备份文件ID
        "commercialCertFlag":"str",//是否下载过安全通道ap(非金)证书[1是,0否]
        "APCert":"str",//ap(非金)证书列表[例: 01,02,03,04]
        "keyLengthType":"str",//TK长度类型[0-16字节长度,1-24字节长度][密钥能力为2时长度为24字节，其他密钥能力长度为16字节]
    }//产品信息
}
```

---

##### getCoreidIsDownloadFlag

```
返回校准文件是否有实体文件
```

###### 请求jsonValue
```json
{
    "coreid": "str"//校准/备份文件ID
}
```
###### 应答data
```json
{
    "isDownloadFlage":"str"//是否要下文件[1要,0不要][在黑名单里不要下载]
}
```

---

##### checkAndroidAuthByCpuid

```
根据cpuid验证Android授权
```

###### 请求jsonValue
```json
{
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    "authInfo":"str"//安卓授权信息
}
```

---

##### uploadAndroidAuth

```
上传Android授权信息
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "cpuid": "str",//cpuid
    "authInfo": "str"//安卓授权信息
}
```
###### 应答data
```json
{
    
}
```

---

##### googleCsrFileUpload

```
谷歌CSR文件上传
```

###### 请求jsonValue
```json
{
    "fileJson": "str",//谷歌CSR文件内容
    "productSN": "str"//整机SN
}
```
###### 应答data
```json
{
    
}
```

---

##### checkAndUploadICCIDInfo

```
采集校验ICCID
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "boardSN": "str",//主板SN
    "iccid": "str"//iccid
}
```
###### 应答data
```json
{
    
}
```

---

##### upLoadTKData

```
上传TK并验证KCV
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "productSN": "str",//整机SN
    "opCRTSN": "str",//操作员证书sn
    "opCRTData":"str",//操作员证书内容
    "tkSource":[
        {
            "paymentType":"str",//支付类型[SE-金融,AP-非金]
            "tkArithmeticType":"str",//TK算法类型[AES32,SM416,TDES16,TDES24]
            "workCRTID":"str",//工作证书ID
            "tkData":"str"//tk数据
        }
    ]
}
```
###### 应答data
```json
{
    
}
```

---

##### getEncryptionTUSNByPN

```
通过整机SN获取加密的TUSN数据
```

###### 请求jsonValue
```json
{
    "imode": "int",//预留参数，默认填0
    "iType": "int",//0-联迪,1-H8
    "productSN": "str",//整机SN
    "cpuid": "str",//cpuid
    "strRndHex": "str",//TUSN随机数
    "terminalTUSN": "str",//TUSN
    "strKeyHash": "str",//key索引信息
    "strUkeyAuthCrtId": "str",//U-KEY 身份认证证书ID
    "strNCOperCrtId": "str",//无卡操作员证书ID
    "strActTime": "str",//激活时间
    "keyPlatfromInfo": "str"//终端平台信息
}
```
###### 应答data
```json
{
    "isDownLoadTUSNFlag":"str",//是否下载TUSN标识[1是,0否]
    "enTUSNFlag":"str",//TUSN敏感数据
    "isDownLoadTUSN":"str",//是否下载TUSN标识[1是,0否]
    "TUSN":"str"//TUSN
}
```

---

##### getSerEncTickDataNew

```
终检获取敏感数据和TK数据
```

###### 请求jsonValue
```json
{
    "strKeyHash": "str",//Key 索引信息
    "productSN": "str",//整机SN
    "cpuIDHex": "str",//cpuid
    "sensDataHex": "str",//待加密敏感数据
    "crtDataHex": "str",//根证书数据
    "strRndHex": "str",//随机数
    "keyPlatfromInfo": "str"//终端平台信息
}
```
###### 应答data
```json
{
    "tkData":"str",//tk数据
    "senData":"str"//加密的敏感数据
}
```

---

##### getSoftwareCheckInfo

```
获取软件检查信息
```

###### 请求jsonValue
```json
{
    "productSN": "str"//整机SN
}
```
###### 应答data
```json
{
    "SN":"str",//整机SN
    "CSN":"str",//CSN
    "TUSN":"str",//TUSN
    "PN":"str",//PN(主机料号)
    "IMEI":"str",//IMEI
    "ICCID":"str",//ICCID
    "YM02":"str"//以太网-MAC
}
```

```
以上YM02为示例，如果整机下有多个YM物料，将下发多个，如同时下发"YM02"和"YM06"
```

---

##### getAuthCodeNew

```
获取认证码[用于模态转换]
```

###### 请求jsonValue
```json
{
    "imode": "int",//预留值，默认为0
    "iType": "int",//0-联迪,1-H8
    "opType": "str",//操作类型
    "productSN": "str",//整机SN
    "cpuid": "str",//cpuid
    "authData": "str",//验证数据
    "sensData": "str",//待加密敏感数据
    "strRndHex": "str",//随机数
    "way": "int",//方式（0:表示旧方式，1:新方式）
    "strKeyHash": "str",//key索引信息
    "strUkeyAuthCrtId": "str",//U-KEY身份认证证书ID
    "strNCOperCrtId": "str",//无卡操作员证书ID
    "strActTime": "str",//激活时间
    "keyPlatfromInfo": "str"//终端平台信息
}
```
###### 应答data
```json
{
    "authData":"str",//认证码
    "sensData":"str"//加密的敏感数据
}
```

---

##### launcherGetAuthData

```
切换厂商launcher认证
```

###### 请求jsonValue
```json
{
    "iType": "int",//0-联迪1-H8
    "imode": "str",//预留，默认填写0
    "strInDataHex": "str"//输入数据，包括用途号、版本号、认证随机数，HEX字符串
}
```
###### 应答data
```json
{
    "sensData":"str",//加密的敏感数据
    "sensDataLen":"int"//解码后字节长度
}
```

---

##### snCollect

```
SN采集（类似之前的序列号转换，cpuid与整机SN首次绑定）
整机SN反查传票号，校验与上送的传票号是否一致；校验整机SN是否已绑定cpuid，已有绑定关系，且cpuid不为上送的cpuid，报错；没有绑定关系则新增绑定关系，绑定关系一致则提示成功
```

**业务表说明**：

| 表名           | 字段名    |  说明                                                     |
| -------------- | ------------|------------------------------------------------ |
| t_product_sn_cpuid_relation |  | 整机序列号CPUID关系表（新增整机SN与CPUID关系数据） |

###### 请求jsonValue
```json
{
    "summonsNumber":"str",//传票号
    "productSN": "str",//整机SN
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    
}
```

---

##### customizedSnCollect

```
定制SN采集（SN不变的定制重工，无需解绑，做特定的工序，如下客户预装）
校验定制传票号的成品料号是否与定制SN原成品料号是否一致，不一致则报错；比对整机SN的CPUID是否与上送的CPUID一致；成品料号一致，采集后，更新整机SN的传票号为定制传票号，后续工序以定制传票号的BOM进行
```

**业务表说明**：

| 表名           | 字段名    |  说明                                                     |
| -------------- | ------------|------------------------------------------------ |
| t_aps | actual_material_code | 传票号表（新传票号查询成品料号）     |
| t_barcode_sn | material_code、summons_number | 整机SN表（比对料号，更新定制传票号） |
| t_product_sn_cpuid_relation | product_sn、board_cpuid | 整机序列号CPUID关系表（比对cpuid） |

###### 请求jsonValue
```json
{
    "summonsNumber":"str",//定制传票号
    "productSN": "str",//整机SN
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    
}
```

---

##### reworkSnCollect

```
重工SN采集（SN改变，旧SN绑定信息解绑，新SN进行绑定）
重工整机SN反查传票号，校验与上送的重工传票号是否一致；不需要校验新旧传票的成品料号，比对旧SN的CPUID与上送的CPUID是否一致；重工采集后，替换整机SN与CPUID关系为重工SN、删除旧整机SN采集数据、解绑旧整机SN与ICCID关系数据。
```

**业务表说明**：

| 表名           | 字段名    |  说明                                                     |
| -------------- | ------------|------------------------------------------------ |
| t_product_sn_cpuid_relation | product_sn、board_cpuid | 整机序列号CPUID关系表（比对cpuid，更新SN为重工SN） |
| t_terminal_firmware_info | product_sequence_no、summons_number | 终端固件信息采集表（解绑，删除） |
| t_customer_sim_sn | product_sequence_no、summons_number | 客户SIM序列号表（解绑，更新传票号和整机SN为空） |

###### 请求jsonValue
```json
{
    "summonsNumber":"str",//重工传票号
    "productSN": "str",//新整机SN
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    
}
```

---

##### bindProductUnionpaySNRelation

```
银联二维码绑定
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "unionpayBarcode": "str",//银联二维码
    "account": "str"//操作用户
}
```
###### 应答data
```json
{
    
}
```

---

##### getProductSNInfo

```
获取整机SN信息
```

###### 请求jsonValue
```json
{
    "productSN": "str"//整机SN
}
```
###### 应答data
```json
{
    "barCodeSn":"str",//整机SN
    "summonsNumber":"str",//传票号
    "materialCode":"str"//整机料号
}
```

---

##### uploadDataInfo

```
上传工序操作日志
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "summonsNumber": "str",//传票号
    "productSN": "str",//整机SN
    "workProcessID": "str",//工序ID
    "boardSN": "str",//主板SN
    "boardCPUID": "str",//主板CPUID
    "isFlowBind": "str",//是否流程绑定
    "startTime": "str",//开始时间
    "endTime": "str",//结束时间
    "remark": "str",//备注
    "tagIDInfoList": [
        {
            "tagID":"str",//tagid
            "tagValue":"str",//tag值
            "tagValueType":"int",//tagId值生成类型[0动态下载,1静态数据,2文件名称,3动态采集]
            "channelType":"int",//通道类型[1安全通道,2运维通道]
            "downloadIndex":"int",//下载顺序
            "systemClassURL":"str"//工具处理类
        }
    ],//tag数据(新模式数据)
    "softwaretypeCodeInfoList": [
        {
            "materialCode":"str",//料号/分群码
            "materialName":"str",//物料/分群码名称
            "materialValue":"str",//物料/分群码值
            "materialTypeCode":"str",//物料分群码
            "downloadIndex":"int",//下载顺序
            "materialValueType":"int",//物料/分群码值生成类型[0动态下载,1静态数据,2文件名称,3动态采集]
            "type":"int"//物料/分群码类型[0分群码 1料号]
        }
    ]//软件分群码数据(旧模式数据)
}
```
###### 应答data
```json
{
    
}
```

---

##### uploadAPOSCustomerID

```
APOS客户标识采集
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "productSN": "str",//整机SN
    "customerID": "str",//客户标识
    "firmManager": "str",//厂商2.0通道标识
    "pinganSecurityCode": "str"//平安安全码
}
```
###### 应答data
```json
{
    
}
```

---

##### workProcessCheckPass

```
生产工序过站接口
```

###### 请求jsonValue
```json
{
    "sn": "str",//序列号
    "snType": "str",//序列号类型(1主板,2主机,3整机)
    "workProcessID": "str",//工序ID
    "testProjectName": "str",//测试项名称
    "result": "str",//工序执行结果(0失败,1成功)
    "resultMessage": "str",//结果描述
    "account": "str"//帐号
}
```
###### 应答data
```json
{
    
}
```

---

##### encSpecialFileByFileName

```
加密敏感配置文件（通过文件名获取）
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "summonsNumber": "str",//传票号
    "cpuid": "str",//cpuid
    "localFileName": "str",//本地加密文件guid文件名
    "strRndHex": "str"//随机数
}
```
###### 应答data
```json
{
    "sensData":"str"//加密的敏感配置文件数据
}
```

---

##### encSpecialFile

```
加密敏感配置文件（上送文件内容）（没看到调用记录）
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "summonsNumber": "str",//传票号
    "cpuid": "str",//cpuid
    "fileContent": "str",//文件内容
    "strRndHex": "str"//随机数
}
```
###### 应答data
```json
{
    "sensData":"str"//加密的敏感配置文件数据
}
```

---

##### checkBoardCPUIDValidity

```
验证主板特征码合法性
```

###### 请求jsonValue
```json
{
    "boardSN": "str",//主板SN
    "boardCUPID": "str",//主板CPUID
    "account": "str"//帐号
}
```
###### 应答data
```json
{
    
}
```

---

##### getBurnAuthData

```
获取烧片授权数据
```

###### 请求jsonValue
```json
{
    "strVer": "str",//烧片认证协议版本 “01”-旧协议，仅用私钥加密；“02”-新协议，使用工作证书的私钥加密，加上工作证书组包
    "strCPUIDHex": "str"//CPUID待认证数据，BCD码字符串
}
```
###### 应答data
```json
{
    "authData":"str",//授权数据
    "cpuidData":"str"//cpuid数据
}
```

---

##### getServerDateTime

```
返回服务器当前时间
```

###### 请求jsonValue

```json
{

}
```

###### 应答data

```json
{
    "serverTime":"str",//服务器当前时间[yyyy-MM-dd HH:mm:ss]
    "serverUtcTime":"str"//服务器当前utc时间[yyyy-MM-dd HH:mm:ss]
}
```

---

##### getSoftwareSecretkey

```
通过文件名获取对应解密密钥接口
```

###### 请求jsonValue
```json
{
    "accessoryFileName": "str"//软件文件名称
}
```
###### 应答data
```json
{
    "encrytKey":"str",//文件加密密钥
    "encrytedMD5":"str",//加密后文件MD5校验码
    "fileExtension":"str"//文件扩展名
}
```

---

##### softwareLogUpload

```
软件下装日志上传 - 对应：BurnFileLogUpload 烧片软件下载日志上传
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "logs": "str"//下装日志[整机SN,主板SN,CPUID,烧片文件名称,开始下载时间,结束下载时间;]
}
```
###### 应答data
```json
{
    
}
```

**请求字段说明**：

| 字段           | 说明                                                         |
| -------------- | ------------------------------------------------------------ |
| `logs` | "整机SN,主板SN,CPUID,烧片文件名称,开始下载时间,结束下载时间;..."，例"188WCA8W2162,188A09KPJ,c43f825b3030303138384130394b504a51434d32,54ccd450-7230-4e64-8757-8684deb62366,\n                2025-04-23 09:23:07,2025-04-23 09:24:25;188WCA8W2162,188A09KPJ,c43f825b3030303138384130394b504a51434d32,fb749c4d-199d-456e-ab68-97d03ec29568,\n                2025-04-23 09:23:07,2025-04-23 09:24:25" |

---

##### gateWayRegister

```
网关Tag注册接口
```

###### 请求jsonValue
```json
{
    "sn": "str",//序列号
    "cpuId": "str",//cpuid
    "workProcessID": "str",//工序ID
    "tagList": [
        {
            "tagId":"str",//tagID
            "tagValue":"str",//tag值
            "tagIndex":"int",//tag顺序
            "localfile":"str"//本地文件名
        }
    ]
}
```
###### 应答data
```json
{
    
}
```

---

##### getCrtAuthDataAndCrtData

```
获取安全配置文件和认证数据
```

###### 请求jsonValue
```json
{
    "strAuthRndHex": "str",//认证随机数
    "strKeyHash": "str",//证书哈希值
    "strUkeyAuthCrtId": "str",//ukey身份认证证书ID
    "strNCOperCrtId": "str",//无卡操作员证书id
    "strRndHex": "str",//随机数
    "productSN": "str",//整机SN
    "cpuid": "str",//cpuid
    "account": "str",//帐号
    "macAddress": "str"//mac地址
}
```
###### 应答data
```json
{
    "flag": "int",//是否有安全配置文件标识[0否，1是]
    "localFileName": "str",//证书文件名
    "fileData": "str",//文件内容
    "crtAuthData": "str"//证书认证数据
}
```

---

##### getUseTimesByEquipmentID

```
通过工装ID获取到本工装的使用次数
```

###### 请求jsonValue
```json
{
    "equipmentID": "str"//工装ID
}
```
###### 应答data
```json
{
    "useTimes": "int"//使用次数
}
```

---

##### newCheckProductSNValidity

```
验证整机SN有效性
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "summonsNumber": "str",//传票号
    "workProcessID": "str",//工序ID
    "cpuid": "str"//cpuid
}
```
###### 应答data
```json
{
    "boardSN":"str",//主板SN
    "bdtName":"str",//烧片bdt名称
    "coreid": "str",//校准/备份文件ID
    "cpuid":"str",//cpuid
    "gatsFlag":"str",//GATS标识[0-没做GATS,1-已做GATS,99-不需要GATS]
    "productSN":"str",//整机SN
    "commercialCertFlag":"str",//非金证书下载标识[0-未下载,1-已下载]
    "customerInfo":[
        "str"//客户ID&客户序列号
    ]//主板SN
}
```

---

##### lkmsDataRequest

```
LKMS数据请求
```

###### 请求jsonValue
```json
{
    "data": "str",//请求数据
    "requestURL": "str"//lkms请求url
}
```
###### 应答data
```json
{
    "response": "str"//应答数据
}
```

---

##### getWorkCRTInfo

```
获取加密机工作证书
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "typeList": [
        {
            "paymentType":"str",//支付类型[SE-金融,AP-非金]
            "tkArithmeticType":"str"//TK算法[AES32,SM416,TDES16,TDES24]
        }
    ]//工作证书类型列表
}
```
###### 应答data
```json
{
    "crtList": [
        {
            "paymentType":"str",//支付类型[SE-金融,AP-非金]
            "tkArithmeticType":"str",//TK算法[AES32,SM416,TDES16,TDES24]
            "workCrtId":"str",//工作证书ID
            "CrtContent":"str"//工作证书内容
        }
    ]//工作证书
}
```

---

##### getLKMSVendorCertDownloadInfo

```
LKMS厂商私有数据下载信息获取
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "channelAP": "str",//非金证书通道 1：标识走新，0标识走旧，如果不走默认走旧
    "typeList": [
        {
            "paymentType":"str",//支付类型[SE-金融,AP-非金]
            "tkArithmeticType":"str"//TK算法[AES32,SM416,TDES16,TDES24]
        }
    ]//工作证书类型列表
}
```
###### 应答data
```json
{
    "downloadInfo": [
        {
            "paymentType":"str",//支付类型[SE-金融,AP-非金]
            "tkArithmeticType":"str",//TK算法[AES32,SM416,TDES16,TDES24]
            "terminalCertCode":"str",//证书编号
            "tkData":"str",//TK数据
            "url":"str",//url(socket)
            "requestURL":"str",//url(http)
            "certCNType":"str"//证书CN域类型[默认为0；0-SN、1-CSN、2-TUSN]
        }
    ]//下载信息
}
```

---

##### getAndroidAuthInfo

```
ISCOSv2平台设备授权
```

###### 请求jsonValue
```json
{
    "account": "str",//帐号
    "cpuid": "str",//cpuid
    "platformInfo": "str",//终端平台信息
    "platformType": "str"//平台类型
}
```
###### 应答data
```json
{
    "isAuth": "str",//是否授权[0否,1是]
    "authInfo":"str"//授权信息
}
```

---

##### getFileDownloadPath

```
获取工具更新下载地址
```

###### 请求jsonValue
```json
{
    "toolCode": "str",//工具代码
    "port": "str"//端口
}
```
###### 应答data
```json
{
    "downloadUsername": "str",//帐号
    "downloadPassword": "str",//密码、
    "downloadAddress": "str",//下载地址
    "downloadPort": "str"//下载端口
}
```

---

##### recordTerminalAbilityInfo

```
记录终端能力相关信息
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "account": "str",//帐号
    "certMode": "str",//证书模式
    "commercialCertFlag": "str",//安全通道是否下载过ap证书标识，0标识否，1标识是
    "customerId": "str",//客户编号
    "manufacturerCert": [
        {
            "paymentType":"str",//支付类型[SE-金融,AP-非金]
            "cert":"str"//证书编号列表
        }
    ]//厂商证书
}
```
###### 应答data
```json
{
    
}
```

---

##### getLKMSCustomKeyDownloadInfo

```
LKMS客户密钥下载信息获取
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "customerIDList": "str"//客户编号列表，多个以,逗号隔开
}
```
###### 应答data
```json
{
    "downloadInfo": [
        {
            "customerId":"str",//客户编号
            "tkData":"str",//TK数据
            "url":"str",//url(socket)
            "requestURL":"str",//url(http)
            "tusn":"str"//tusn
        }
    ]//下载信息
}
```

---

##### setEquipmentUseLifetime

```
设置工装测试次数
```

###### 请求jsonValue
```json
{
    "equipmentID": "str",//工装ID
    "useTimes": "str"//使用次数
}
```
###### 应答data
```json
{
    
}
```
---

##### getConfigFileByProductSN

```
通过整机序列号获取配置文件信息
```

###### 请求jsonValue
```json
{
    "productSN": "str",//整机SN
    "materialTypeCode": "str"//软件分群码，多个以,逗号隔开
}
```
###### 应答data
```json
{
    "summonsNumber": "str",//传票号
    "configFileInfo": "str"//配置文件信息
}
```

**应答字段说明**：

| 字段             | 说明                                                         |
| ---------------- | ------------------------------------------------------------ |
| `configFileInfo` | [料号] 文件名称,本地文件名,文件大小,分群码,加密后文件MD5校验码,文件拓展名,下载顺序；示例：[SS08000515] 生产补丁-C20-生产测试-V1.0.41-20250325,34ef2290-f946-4ca0-90fe-4ef84b3a56d0,8421933,SF09,20B8A7C9701E09D00A09DE52F40D69D8,zip,0;[SS28000517] GATS配置-SingleScreen-Cal-C20系列-色温-V1.0-20250325,11e35301-8288-4ede-b42e-bef96eeadcaf,724,SF28,8E56848EF7BBAEE9823FDE84078DCAC2,zip,0 |

---

### 烧片激活

#### 1、列出用户所拥有的所有证书信息

##### 列出用户所拥有的所有证书信息接口地址
`POST` 

```
$HOST/CoreCommonSevice/api/v1/UkeyManage/FacListCrts
```
##### 列出用户所拥有的所有证书信息接口请求参数

```json
{
    "action": "str",//方法
    "version":"str",//版本
    "token":"str",//token
    "infoForRight":"str",//权限信息
    "operationLog":"str",//操作日志
    "clientId":"str"//客户端ID
}
```

##### 列出用户所拥有的所有证书信息接口成功返回结果

```json
{
    "IsOK":boolean,//true,false
    "Description":"str/null",
    "Response":"str",//应答数据，字符串
    "Code":null,//应答码
    "Token":null" //token
}
```

```
Response数据说明
```
```json
{
    "crtList":[
        {
            "keyId":"str",//证书ID
            "certName":"str",//证书名称
            "certOrgNum":"str",//组号
            "certInOrgNum":"str",//组内号
            "certType":"str"//证书类型，00旧格式、01新格式
        }
    ],//证书列表
    "errorCode":"str",//错误码
    "info":"str",//信息
    "signature":"str",//签名
    "status":"str",//状态码，00成功，其他失败
    "success":boolean//true,false
}
```

---

#### 2、描述用户指定的keyId的信息

##### 描述用户指定的keyId的信息接口地址
`POST` 

```
$HOST/CoreCommonSevice/api/v1/UkeyManage/FacDescribeKey
```
##### 描述用户指定的keyId的信息接口请求参数

```json
{
    "action": "str",//方法
    "version":"str",//版本
    "token":"str",//token
    "keyId":"str",//证书ID
    "infoForRight":"str",//权限信息
    "operationLog":"str",//操作日志
    "clientId":"str"//客户端ID
}
```

##### 描述用户指定的keyId的信息接口成功返回结果

```json
{
    "IsOK":boolean,//true,false
    "Description":"str/null",
    "Response":"str",//应答数据，字符串
    "Code":null,//应答码
    "Token":null" //token
}
```
```
Response数据说明
```
```json
{
    "certificateData":"str",//证书数据
    "description":"str",//描述
    "errorCode":"str",//错误码
    "info":"str",//信息
    "keyId":"str",//证书ID
    "keyUsage":"str",//证书用途
    "resultFlag":"str",//结果标识，默认01
    "signature":"str",//签名
    "status":"str",//状态码，00成功，其他失败
    "success":boolean//true,false
}
```
---

#### 3、使用用户指定keyid对应的私钥进行运算

##### 使用用户指定keyid对应的私钥进行运算接口地址
`POST` 

```
$HOST/CoreCommonSevice/api/v1/UkeyManage/FacPrivateCalc
```
##### 使用用户指定keyid对应的私钥进行运算接口请求参数

```json
{
    "action": "str",//方法
    "version":"str",//版本
    "token":"str",//token
    "keyId":"str",//证书ID
    "dataToCalc":"str",//运算数据
    "calcOperate":"str",//运算操作
    "algrithom":"str",//算法
    "infoForRight":"str",//权限信息
    "operationLog":"str",//操作日志
    "clientId":"str"//客户端ID
}
```

##### 使用用户指定keyid对应的私钥进行运算接口成功返回结果

```json
{
    "IsOK":boolean,//true,false
    "Description":"str/null",
    "Response":"str",//应答数据，字符串
    "Code":null,//应答码
    "Token":null" //token
}
```
```
Response数据说明
```
```json
{
    "errorCode":"str",//错误码
    "info":"str",//信息
    "resultData":"str",//结果数据
    "signature":"str",//签名
    "status":"str",//状态码，00成功，其他失败
    "success":boolean//true,false
}
```

---

#### 4、tk签名

`POST` 

```
$HOST/CoreCommonSevice/api/v1/UkeyManage/TkSign
```

##### tk签名接口请求参数

请求头

```
将token放入请求头TISAuth中
```

请求体

```json
[{
	"id": "str",
	"tkData": "str"  //待签名数据
}]
```

##### tk签名接口成功返回结果

```json
{
    "IsOK":boolean,//true,false
    "Description":"str/null",
    "Response":{
                  "status":"str",//状态码，0成功
                  "message":"str",//信息
                  "code":"str",//结果数据
                  "data":{
                      "crtId"："",    //证书ID
                      "crtData"："",  //证书数据
                  },
                  "signDataArray":[{
                                    "id":"str",  //
                                    "signData":"str" //签名数据
                                   }]
              },//应答数据
    "Code":null,//应答码
    "Token":null" //token
}
```

---