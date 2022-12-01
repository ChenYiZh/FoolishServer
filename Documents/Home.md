---

Foolish Server 是基于C#的平台，快速开发搭建网络连接的服务器框架。原型是参考[Scut](https://github.com/ScutGame/Scut)进行开发。基于多年开发总结下来的经验，有许多功能被删改，并且不会纳入服务器功能。例如，分布式需要开发者自己实现，热更功能被移除，Lua以及Python被移除，数据从完全依赖Redis，改为程序内部管理，等等。

## 库文件说明

- FoolishGames.Common.dll 客户端服务器通用库
- FoolishServer.Framework.dll 服务器关键逻辑库，包括开启关闭服务器，可用于客户端
- FoolishClient.Framework.dll 客户端关键逻辑库，建立连接等操作，可用于服务器
- FoolishServer.Common.dll 数据库连接，IL注入等逻辑
- FoolishGames.Compiler.CSharp.dll C#的编译工具

以上所有的库都基于NetStandard 2.0 编写，因此如果需要使用，就再另建一个入口项目，来调用指定启动函数 `RuntimeHost.Startup()`来启动服务器。客户端管理类为`Network`。

## 依赖的外部引用库

- [Microsoft.CodeAnalysis.CSharp](https://github.com/dotnet/roslyn)
- [Mono.Cecil](https://github.com/jbevain/cecil/)
- [MySql.Data](https://dev.mysql.com/downloads/)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [NLog](https://nlog-project.org/)
- [protobuf-net](https://github.com/protobuf-net/protobuf-net)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/)

## 服务器框架结构图
[![Framework](Documents/Images/Framework.jpg)]

- Sockets层 根据Config的配置开启多组服务器
- Action层 用于执行Socket收到的协议，根据Socket中配置进行管理
- DataContext层 数据管理，数据进入DataContext以DbSet缓存在内部，分为冷热数据。热数据根据配置，在一段时间未进行修改，推入冷数据。冷数据需要动态加载，并且保留15秒（可配置），超时后从内存中卸载。
- Database层 数据落地层，热数据的修改会直接推送给Redis，以及冷数据库。当热数据变为冷数据时，会从Redis中删除。服务器启动默认只加载热数据。

## 注意事项
...
## 服务器生命周期
...

## 稳定性与性能
...
