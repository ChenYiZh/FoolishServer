## 示例Demo
[Example](../../raw/main/Samples/GameServer/GameServer)
## 执行文件选择
查看Release文件夹下的目录，其中只编译了Net4.6和NetCore2.1的版本。其他环境需要自行编译。

## 配置Visual Studio环境

1. [新建 .NetStandard 项目](https://learn.microsoft.com/zh-cn/nuget/quickstart/create-and-publish-a-package-using-the-dotnet-cli) / 创建 .Net 项目 （根据使用的执行文件）
2. 将FoolishServer的可执行代码拷贝到项目目录

![HelloWorld003](../../raw/main/Documents/Images/HelloWorld003.jpg)

3. 右键属性

![HelloWorld001](../../raw/main/Documents/Images/HelloWorld001.jpg)

4. 选择调试
5. 设置工作目录为执行文件所在目录，再选取可执行文件

![HelloWorld002](../../raw/main/Documents/Images/HelloWorld002.jpg)

## 服务器开发

1. 新建class，继承CustomRuntime

![HelloWorld004](../../raw/main/Documents/Images/HelloWorld004.jpg)

2. 重载函数，参考代码

![HelloWorld005](../../raw/main/Documents/Images/HelloWorld005.jpg)

3. 修改配置文件

![HelloWorld006](../../raw/main/Documents/Images/HelloWorld006.jpg)
![HelloWorld007](../../raw/main/Documents/Images/HelloWorld007.jpg)

4. 新建class，继承TcpServer，重载函数

![HelloWorld008](../../raw/main/Documents/Images/HelloWorld008.jpg)

5. 修改配置文件

![HelloWorld009](../../raw/main/Documents/Images/HelloWorld009.jpg)

6. 新建Model类，Test继承MajorEntity，Test2继承MinorEntity

![HelloWorld010](../../raw/main/Documents/Images/HelloWorld010.jpg)
![HelloWorld011](../../raw/main/Documents/Images/HelloWorld011.jpg)

7. 新建Action协议类，Action1000继承ServerAction，实现抽象函数

![HelloWorld011](../../raw/main/Documents/Images/HelloWorld012.jpg)

8. 修改配置文件

![HelloWorld013](../../raw/main/Documents/Images/HelloWorld013.jpg)

9. 启动Redis，检查MySql启动情况

![HelloWorld014](../../raw/main/Documents/Images/HelloWorld014.jpg)

10. 启动

![HelloWorld015](../../raw/main/Documents/Images/HelloWorld015.jpg)

## Unity开发
[Example](../../raw/main/Samples/Unity)

1. 新建项目，新建Plugins目录，将 [FoolishClient.Framework.dll](../../raw/main/Release/FoolishClient) 以及 [FoolishGames.Common.dll](../../raw/main/Release/FoolishClient) 拖入Plugins

![HelloWorld016](../../raw/main/Documents/Images/HelloWorld016.jpg)

2. 新建 MonoBehaviour 命名为 UpdateProxy，并实现 IBoss

![HelloWorld017](../../raw/main/Documents/Images/HelloWorld017.jpg)

3. 新建 MonoBehaviour 命名为 LogProxy，并实现 FoolishGames.Log.ILogger

![HelloWorld018](../../raw/main/Documents/Images/HelloWorld018.jpg)

4. 新建Action1000，并继承ClientAction

![HelloWorld019](../../raw/main/Documents/Images/HelloWorld019.jpg)

5. 新建 MonoBehaviour 命名为 Startup

![HelloWorld020](../../raw/main/Documents/Images/HelloWorld020.jpg)

6. 场景中新建GameObject，并将两个脚本拖上GameObject

![HelloWorld021](../../raw/main/Documents/Images/HelloWorld021.jpg)

7. 运行

![HelloWorld022](../../raw/main/Documents/Images/HelloWorld022.jpg)
