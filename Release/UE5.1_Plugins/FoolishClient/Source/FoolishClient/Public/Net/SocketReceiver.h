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
#include "ClientSocket.h"
#include "UObject/Object.h"
#include "SocketReceiver.generated.h"

/**
 * 收发消息处理
 */
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FMessageReceiveEventHandler, UClientSocket*, Socket, UMessageReader*,
                                             Message);

/**
 * 消息接收处理类
 */
UCLASS(NotBlueprintable, NotBlueprintType, DisplayName="Socket Receiver")
class FOOLISHCLIENT_API USocketReceiver final : public UObject
{
	GENERATED_BODY()
	friend class FSocketReceiverTask;
private:
	/**
	 * @brief 封装的套接字
	 */
	UPROPERTY()
	TWeakObjectPtr<UClientSocket> Socket = nullptr;
	/**
	 * @brief 是否正在接收数据
	 */
	FThreadSafeBool IsReceiving = false;
	/**
	 * @brief 解析包时解析不完的数据
	 */
	UPROPERTY()
	TArray<uint8> ReceivedBuffer; // = nullptr;
	/**
	 * @brief 解析包时解析不完的数据的长度
	 */
	//int32 ReceivedBufferLength = 0;
	/**
	 * @brief 已经接收的数据长度
	 */
	UPROPERTY()
	int32 ReceivedStartIndex = 0;
public:
	/**
	 * @brief 封装的套接字
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket Receiver", DisplayName="Socket")
	UClientSocket* GetSocket();
	/**
	 * @brief 是否正在接收数据
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket Receiver", DisplayName="IsReceiving")
	bool GetIsReceiving();
	/**
	 * @brief 判断是否可以接收，如果可以则切换至接收状态
	 */
	UFUNCTION(BlueprintPure)
	bool Receivable();

public:
	/**
	 * @brief 接收到数据包事件
	 */
	UPROPERTY(BlueprintAssignable, Category="Foolish Games|Socket Receiver")
	FMessageReceiveEventHandler OnMessageReceived;
	/**
	 * @brief 心跳探索事件
	 */
	UPROPERTY(BlueprintAssignable, Category="Foolish Games|Socket Receiver")
	FMessageReceiveEventHandler OnPing;
	/**
	 * @brief 心跳回应事件
	 */
	UPROPERTY(BlueprintAssignable, Category="Foolish Games|Socket Receiver")
	FMessageReceiveEventHandler OnPong;

private:
	/**
	 * @brief 接收到数据包事件
	 */
	void MessageReceived(UClientSocket* InSocket, UMessageReader* InMessage);
	/**
	 * @brief 心跳探索事件
	 */
	void Ping(UClientSocket* InSocket, UMessageReader* InMessage);
	/**
	 * @brief 心跳回应事件
	 */
	void Pong(UClientSocket* InSocket, UMessageReader* InMessage);

public:
	/**
	 * @brief 初始化
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Receiver")
	void Initialize(UClientSocket* FSocket);
	/**
	 * @brief 等待消息接收
	 * @return 是否维持等待状态
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Receiver")
	bool BeginReceive(bool bForce = false);
	/**
	 * @brief 处理数据接收回调
	 */
	bool ProcessReceive(bool bForce);
	/**
	 * @brief 接受类释放
	 */
	void Release();

private:
	/**
	 * @brief 线程事件
	 */
	void ReceiveTask(bool bForce);
};

/**
 * @brief 接收线程任务
 */
class FOOLISHCLIENT_API FSocketReceiverTask : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<FSocketReceiverTask>;

private:
	/**
	 * @brief 接收操作
	 */
	TWeakObjectPtr<USocketReceiver> Receiver = nullptr;
	/**
	 * @brief 强制执行
	 */
	bool bForce = false;

	/**
	 * @brief 初始化函数
	 */
	FSocketReceiverTask(USocketReceiver* InReceiver, bool bIsForce): Receiver(InReceiver), bForce(bIsForce)
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
