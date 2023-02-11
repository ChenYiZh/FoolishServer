/****************************************************************************
THIS FILE IS PART OF Foolish Server PROJECT
THIS PROGRAM IS FREE SOFTWARE, IS LICENSED UNDER MIT

Copyright (c) 2022-2030 ChenYiZh
https://space.bilibili.com/9308172

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
****************************************************************************/

#pragma once

#include "CoreMinimal.h"
#include "FSocket.h"
#include "Proxy/ClientActionDispatcher.h"
#include "UObject/Object.h"
#include "ClientSocket.generated.h"


class UMessageWriter;

/**
 * 连接的回调函数
 */
DECLARE_DYNAMIC_DELEGATE_OneParam(FClientSocketConnectCallback, bool, bSuccess);

/**
 * 套接字父类
 */
UCLASS(NotBlueprintable, BlueprintType, DisplayName="Client Socket")
class FOOLISHCLIENT_API UClientSocket : public UFSocket
{
	GENERATED_BODY()
	friend class FClientSocketConnectAsyncTask;
	friend class FClientSocketMessageProcessor;
	friend class UMessageWorker;
	friend class FClientSocketHeartbeatRunnable;

private:
	/**
	 * @brief 标识名称
	 */
	UPROPERTY()
	FName Name;
	/**
	 * @brief 运行的标识
	 */
	//UPROPERTY()
	FThreadSafeCounter ReadyFlag = 0;

	/**
	 * @brief 心跳间隔
	 */
	UPROPERTY()
	int32 HeartbeatInterval;

	// /**
	//  * @brief 计时器管理类
	//  */
	// FTimerManager& TimerManager;

	class FClientSocketHeartbeatRunnable* HeartbeatRunnable;

	/**
	 * @brief 心跳包线程
	 */
	FRunnableThread* HeartbeatTimer = nullptr;

	/**
	 * @brief 心跳包
	 */
	TArray<uint8> HeartbeatBuffer;
	/**
	 * @brief Action生成类
	 */
	UPROPERTY()
	UClientActionDispatcher* ActionProvider = nullptr;
	/**
	 * @brief 消息处理的中转站
	 */
	UPROPERTY()
	UObject* MessageContractor = nullptr;

protected:
	/**
	 * @brief 发送的管理类
	 */
	UPROPERTY()
	class USocketSender* Sender = nullptr;
	/**
	 * @brief 接收管理类
	 */
	UPROPERTY()
	class USocketReceiver* Receiver = nullptr;

public:
	/**
	 * @brief 标识名称
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Client Socket", DisplayName="Name")
	FName GetName();
	/**
	 * @brief 数据是否已经初始化了
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Client Socket")
	bool IsReady() const;
	/**
	 * @brief 心跳间隔
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Client Socket", DisplayName="Heartbeat Interval")
	int32 GetHeartbeatInterval();
	/**
	 * @brief 消息Id，
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Client Socket")
	void SetMessageNumber(const int64 InMessageNumber);
	/**
	 * @brief 消息Id，
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Client Socket")
	int64 GetMessageNumber() const;
	/**
	 * @brief Action生成类
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Client Socket")
	void SetActionProvider(UClientActionDispatcher* Provider);
	/**
	 * @brief Action生成类
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Client Socket")
	UClientActionDispatcher* GetActionProvider() const;
	/**
	 * @brief 消息处理的中转站
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Client Socket")
	void SetMessageContractor(UObject* InMessageContractor);
	/**
	 * @brief 消息处理的中转站，IBoss
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Client Socket")
	UObject* GetMessageContractor() const;

public:
	/**
	 * @brief 初始化Socket基本信息
	 * @param InName 连接名称
	 * @param Host 地址
	 * @param InPort 端口
	 * @param InHeartbeatInterval 心跳间隔
	 * @param ActionClassFullNames Action协议类的完整名称
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void Ready(const FName& InName, const FString& Host, const int32& InPort,
	           const TArray<FString>& ActionClassFullNames, const int32& InHeartbeatInterval = 10000);
	/**
	 * @brief 自动连接
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void AutoConnect();

	/**
	 * @brief 连接函数[内部异步实现]
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void ConnectAsync(const FClientSocketConnectCallback& Callback);
	/**
	 * @brief 设置消息的偏移值
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void SetMessageOffset(const int32 Offset);
	/**
	 * @brief 设置压缩方案
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void SetCompression(UCompression* InCompression);
	/**
	 * @brief 设置解密方案
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void SetCryptoProvider(UCryptoProvider* InCryptoProvider);
protected:
	/**
	 * @brief 同步连接
	 */
	virtual bool Connect();
	/**
	 * @brief 初始化执行
	 */
	virtual void Awake();
	/**
	 * @brief 创建原生套接字
	 */
	virtual FSocket* MakeSocket();
	/**
	 * @brief 发送心跳包
	 */
	virtual void SendHeartbeatPackage();
	/**
	 * @brief 创建默认心跳包数据
	 */
	virtual void BuildHeartbeatBuffer(TArray<uint8>& HeartbeatBuffer);

public:
	/**
	 * @brief 重新构建心跳包数据
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Client Socket")
	void RebuildHeartbeatPackage();
	/**
	 * @brief 关闭函数
	 */
	virtual void Close_Implementation(EOpCode opCode) override;

public:
	/**
	 * @brief 数据发送 异步 
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Client Socket")
	void Send(UMessageWriter* Message);

	/**
	 * @brief 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用 
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Client Socket")
	void SendImmediately(UMessageWriter* Message);

private:
	/**
	 * @brief 消息处理
	 */
	UFUNCTION()
	void OnMessageReceived(UClientSocket* InSocket, UMessageReader* Message);
	/**
	 * @brief 消息处理
	 */
	virtual void ProcessMessage(UMessageReader* Message);

protected:
	/**
	 * @brief 内部函数，直接传bytes，会影响数据解析
	 */
	virtual void SendBytes(const TArray<uint8>& Data);
	/**
	 * @brief 内部函数，直接传bytes，会影响数据解析，以及解析顺序
	 */
	virtual void SendBytesImmediately(const TArray<uint8>& Data);

private:
	// TODO: 消息处理的几个函数
};

/**
 * @brief 心跳线程
 */
class FOOLISHCLIENT_API FClientSocketHeartbeatRunnable : public FRunnable
{
private:
	/**
	 * @brief Socket
	 */
	UClientSocket* Socket;
	/**
	 * @brief 是否执行
	 */
	FThreadSafeBool IsRunning;

public:
	/**
	 * @brief 起始有效性
	 */
	//virtual bool Init() override { return true; }
	/**
	 * @brief 初始化操作
	 */
	FClientSocketHeartbeatRunnable(UClientSocket* InSocket): Socket(InSocket), IsRunning(true)
	{
	}

	/**
	 * @brief 线程逻辑
	 */
	virtual uint32 Run() override;
	/**
	 * @brief 强制关闭时执行
	 */
	virtual void Stop() override;
	// /**
	//  * @brief 退出时执行
	//  */
	// virtual void Exit() override;
};

/**
 * @brief 异步连接的任务逻辑
 */
class FOOLISHCLIENT_API FClientSocketConnectAsyncTask : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<FClientSocketConnectAsyncTask>;
private:
	/**
	 * @brief 连接成功的回调
	 */
	//UPROPERTY()
	FClientSocketConnectCallback Callback;
	/**
	 * @brief ClientSocket
	 */
	//UPROPERTY()
	TWeakObjectPtr<UClientSocket> Socket;
	/**
	 * @brief 初始化函数
	 */
	FClientSocketConnectAsyncTask(UClientSocket* InSocket, const FClientSocketConnectCallback& InCallback):
		Callback(InCallback), Socket(InSocket)
	{
		//Callback = MakeShared<FClientSocketConnectCallback>(InCallback);
	}

public:
	/**
	 * @brief 需要执行的操作
	 */
	void DoWork();
	/**
	 * @brief 固定写法，本类将作为函数参数
	 */
	FORCEINLINE TStatId GetStatId() const;
};

/**
 * @brief 异步连接的任务逻辑
 */
class FOOLISHCLIENT_API FClientSocketMessageProcessor : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<FClientSocketMessageProcessor>;
private:
	/**
	 * @brief ClientSocket
	 */
	//UPROPERTY()
	TWeakObjectPtr<UClientSocket> Socket;
	/**
	 * @brief 消息
	 */
	UMessageReader* Message;
	/**
	 * @brief 初始化函数
	 */
	FClientSocketMessageProcessor(UClientSocket* InSocket, UMessageReader* InMessage):
		Socket(InSocket), Message(InMessage)
	{
	}

public:
	/**
	 * @brief 需要执行的操作
	 */
	void DoWork();
	/**
	 * @brief 固定写法，本类将作为函数参数
	 */
	FORCEINLINE TStatId GetStatId() const;
};
