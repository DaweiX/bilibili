# bilibili  
## 哔哩哔哩动画第三方客户端（UWP平台）  
### 摘要：  
哔哩哔哩动画的第三方客户端，同时兼容`Windows™ 10桌面版`、`Windows™ 10 Mobile`、`XBOX`、`Surface Hub`与`Hololens`.本应用
主要出于学习、交流目的而开发，没有应用商店发行版.  
详细的更新记录请查阅Notes/Version.txt 文档  
从1.7.5版本开始，提供应用安装包（x64/x86/ARM,位于bilibili/Appx目录下）.安装前请将证书安装于“受信任的根证书颁发机构”目录下）

### 软件截图：  
![](https://github.com/DaweiX/bilibili/tree/master/ScreenShots/Concern.jpg)  
![](https://github.com/DaweiX/bilibili/tree/master/ScreenShots/Detail.jpg)  
![](https://github.com/DaweiX/bilibili/tree/master/ScreenShots/Fav.jpg)  
![](https://github.com/DaweiX/bilibili/tree/master/ScreenShots/Home.jpg)  
![](https://github.com/DaweiX/bilibili/tree/master/ScreenShots/Space.jpg)  
![](https://github.com/DaweiX/bilibili/tree/master/ScreenShots/Topic.jpg)  
  
### 主要功能：  
* 视频分类查询、播放  
* 番剧订阅、播放（MP4：低清，清晰；flv：低清，清晰，超清；[hdmp4因清晰度不高，弃用]）、放送表、分类索引  
* 用户信息   
* 话题、活动浏览  
* 弹幕浏览、发送  

### 特有功能&网页版移植：  
* 经验记录、登录记录  
* 收藏夹设置、管理，网页版收藏夹封面  
* 本地拖拽视频播放  
* 首页栏目排序  
* 固定番剧磁贴到桌面  
* 分享视频获取经验奖励(移植自安卓)  
  
### 现有bug及待加功能：  
#### 现有bug：  
* 下载的稳定性待改进  
* 列表项没有铺满窗口列表不会增量加载  
  
#### 待加功能：  
* 观看直播  
* 取关、互粉等       
* 黑科技  
  
### 引用及参考：  
* [https://github.com/xiaoyaocz/BILIBILI-UWP](cycz：Bilibili-UWP)：基本弹幕实现  
* [www.telerik.com/fiddler](Fiddler)：请求APi获取  
* [Bilibili API-wiki](https://github.com/Qixingchen/MD-BiliBili/wiki/API:-%E6%A6%82%E8%A7%88): 非官方Api文档    
  
### 备注：  
* 运行时系统版本要求：  
  最低：10586；推荐：14393及以上  
* 开发最低要求：  
`Windows™ 10 SDK Version 14393`，`Visual Studio 2015`
* 如遇到问题或想要与我交流，我的邮箱：`DaweiX@outlook.com`
* 本应用从2016.8开始开发，【划掉：今后将一直持续更新】之前apikey被官方封了，现在好像又有了(?)本人是菜鸡电子专业学生
一枚，大三也很忙，停止更新了，回头看代码也有很多不足和bug，只能用于学习交流，如果想编译使用的话需要自行编译ffmpeg和一些其他的东西，
apikey也需要更换一下.（突然发现高斯模糊的那个效果好像可以自己实现了，感觉本应用也就这个来得比网页版要酷炫，嘿嘿）
