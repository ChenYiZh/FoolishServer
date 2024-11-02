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
#include "IO/MessageWriter.h"
#include "UObject/Object.h"
#include "SocketSender.generated.h"

/**
 * 消息发送处理类
 */
UCLASS(NotBlueprintable, NotBlueprintType, DisplayName="Socket Sender")
class FOOLISHCLIENT_API USocketSender final : public UObject
{
	GENERATED_BODY()
	friend class UFSocketWaitToSendMessage;
	friend class FSocketSenderStartSendTask;
	friend class UClientSocket;
private:
	/**
	 * @brief 套接字管理类
	 */
	UPROPERTY()
	TWeakObjectPtr<UFSocket> Socket = nullptr;
	//UFSocket* Socket = nullptr;
	/**
	 * @brief 线程锁
	 */
	FCriticalSection Mutex;
	/**
	 * @brief 待发送的消息列表
	 */
	TDoubleLinkedList<UWorker*> WaitToSendMessages;
	/**
	 * @brief 消息Id，需要加原子锁
	 */
	//UPROPERTY()
	//int64 MessageNumber = FDateTime::Now().GetTicks();
	FThreadSafeCounter64 MessageNumber = FThreadSafeCounter64(FDateTime::Now().GetTicks());
	/**
	 * @brief 是否正在发送
	 */
	FThreadSafeBool IsSending = false;

public:
	/**
	 * @brief 消息Id，
	 * get 返回时会自动 +1
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Sender")
	void SetMessageNumber(int64 InMessageNumber);
	/**
	 * @brief 消息Id，
	 * get 返回时会自动 +1
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket Sender")
	int64 GetMessageNumber();
	/**
	 * @brief 是否正在发送
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket Sender", DisplayName="IsSending")
	bool GetIsSending();
	/**
	 * @brief 判断是否可以发送，如果可以则切换至发送状态
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket Sender")
	bool bSendable();

public:
	/**
	 * @brief 消息发送处理类
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Sender")
	void Initialize(UFSocket* FSocket);
	/**
	 * @brief 开始执行发送
	 * @return 是否维持等待状态
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Sender")
	bool BeginSend();
	/**
	 * @brief 消息发送
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Sender")
	void Send(UMessageWriter* Message);
	/**
	 * @brief 立即发送消息，会打乱消息顺序。只有类似心跳包这种及时的需要用到。一般使用Send就满足使用
	 */
	UFUNCTION(BlueprintCallable, Category="Foolish Games|Socket Sender",
		meta=(DeprecatedFunction="This is uncommend to use 'immediately', beacuse it will change the queue."))
	void SendImmediately(UMessageWriter* Message);
	/**
	 * @brief 内部函数，直接传bytes，会影响数据解析
	 */
	void SendBytes(const TArray<uint8>& Data);
	/**
	 * @brief 内部函数，直接传bytes，会影响数据解析，以及解析顺序
	 */
	//UE_DEPRECATED(4.xx, "This is uncommend to use 'immediately', beacuse it will change the queue.")
	void SendBytesImmediately(const TArray<uint8>& Data);

private:
	/**
	 * @brief 挤入消息队列
	 */
	void CheckIn(const TArray<uint8>& Data, const bool& IsImmediately);

	/**
	 * @brief 挤入消息队列
	 */
	void CheckIn(UMessageWriter* Message, const bool& IsImmediately);

	/**
	 * @brief 挤入消息队列
	 */
	void CheckIn(UFSocketWaitToSendMessage* Worker, const bool& IsImmediately);
	/**
	 * @brief 最后的消息推送
	 */
	void Post(const TArray<uint8>& Data);
	/**
	 * @brief 发送缓存队列中的顶部数据
	 */
	void PostHeadMessage();
	/**
	 * @brief 消息执行完后，判断还有没有需要继续发送的消息
	 */
	bool SendCompleted();
public:
	/**
	 * @brief 在释放时操作
	 */
	void Release();
};

/**
 * @brief 发送线程任务
 */
class FOOLISHCLIENT_API FSocketSenderTask : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<FSocketSenderTask>;

private:
	/**
	 * @brief 发送操作
	 */
	TWeakObjectPtr<USocketSender> Sender = nullptr;

	/**
	 * @brief 初始化函数
	 */
	FSocketSenderTask(USocketSender* InSender): Sender(InSender)
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

/**
 * @brief 在BeginSend中的线程任务
 */
class FOOLISHCLIENT_API FSocketSenderStartSendTask : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<FSocketSenderStartSendTask>;
private:
	/**
	 * @brief 发送操作
	 */
	TWeakObjectPtr<USocketSender> Sender = nullptr;
	/**
	 * @brief 初始化函数
	 */
	FSocketSenderStartSendTask(USocketSender* InSender): Sender(InSender)
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
