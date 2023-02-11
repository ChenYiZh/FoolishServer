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
#include "Interfaces/IPv4/IPv4Address.h"
#include "Interfaces/IPv4/IPv4Endpoint.h"
#include "IO/Compression.h"
#include "IO/CryptoProvider.h"
#include "Proxy/DirectMessageProcessor.h"
#include "UObject/Object.h"
#include "FSocket.generated.h"

/**
 * 操作码
 */
UENUM(BlueprintType)
enum EOpCode
{
	/**
	 * @brief 空数据
	 */
	Empty = -1,
	/**
	 * @brief 连续性数据
	 */
	Continuation = 0,
	/**
	 * @brief 文本数据
	 */
	Text = 1,
	/**
	 * @brief 二进制数据
	 */
	Binary = 2,
	/**
	 * @brief 关闭操作数据
	 */
	Close = 8,
	/**
	 * @brief Ping数据
	 */
	Ping = 9,
	/**
	 * @brief Pong数据
	 */
	Pong = 10,
};

/**
 * 套接字类型
 */
UENUM(BlueprintType)
enum EFSocketType
{
	/**
	 * @brief Tcp Socket
	 */
	Tcp = 0,
	/**
	 * @brief Http Socket
	 */
	Http = 1,
	/**
	 * @brief Udp Socket
	 */
	Udp = 2,
	/**
	 * @brief Web Socket
	 */
	Web = 3,
};

/**
 * 套接字管理基类
 */
UCLASS(BlueprintType, NotBlueprintable)
class FOOLISHCLIENT_API UFSocket : public UObject
{
	GENERATED_BODY()

public:
	/**
	 * @brief 线程锁
	 */
	FCriticalSection Mutex;

protected:
	/**
	 * @brief 是否在运行
	 */
	//UPROPERTY()
	FThreadSafeBool IsRunning = false;

	/**
	 * @brief 地址
	 */
	//UPROPERTY()
	TSharedPtr<FIPv4Endpoint> Address;

	/**
	 * @brief 原生套接字
	 * 在UE的SocketSubsystem中已经做了引用以及对象池
	 */
	//UPROPERTY()
	FSocket* Socket = nullptr;

	/**
	 * @brief 消息偏移值
	 */
	UPROPERTY()
	int32 MessageOffset = 2;

	/**
	 * @brief 压缩工具
	 */
	UPROPERTY()
	UCompression* Compression = nullptr;

	/**
	 * @brief 加密工具
	 */
	UPROPERTY()
	UCryptoProvider* CryptoProvider = nullptr;

public:
	/**
	 * @brief 消息处理方案
	 */
	UPROPERTY(BlueprintReadWrite)
	UObject* MessageEventProcessor = nullptr;
	//CreateDefaultSubobject<UDirectMessageProcessor>(TEXT("Direct Message Processor"));

public:
	/**
	 * @brief 是否在运行
	 */
	UFUNCTION(BlueprintPure, BlueprintNativeEvent, Category="Foolish Games|Socket", DisplayName="Is Running")
	bool GetIsRunning();
	/**
	 * @brief 是否在运行
	 */
	virtual bool GetIsRunning_Implementation();
	/**
	 * @brief 是否已经开始运行
	 */
	UFUNCTION(BlueprintPure, BlueprintNativeEvent, Category="Foolish Games|Socket", DisplayName="Connected")
	bool GetConnected();
	/**
	 * @brief 是否已经开始运行
	 */
	virtual bool GetConnected_Implementation();
	/**
	 * @brief 地址
	 */
	UFUNCTION(BlueprintPure, BlueprintNativeEvent, Category="Foolish Games|Socket", DisplayName="Host")
	FString GetHost();
	/**
	 * @brief 地址
	 */
	virtual FString GetHost_Implementation();
	/**
	 * @brief 端口
	 */
	UFUNCTION(BlueprintPure, BlueprintNativeEvent, Category="Foolish Games|Socket", DisplayName="Address")
	int32 GetPort();
	/**
	 * @brief 端口
	 */
	virtual int GetPort_Implementation();
	/**
	 * @brief 地址
	 */
	//UFUNCTION(BlueprintPure, DisplayName="Socket")
	FSocket* GetSocket() const;

	/**
	 * @brief 类型
	 */
	UFUNCTION(BlueprintPure, BlueprintNativeEvent, Category="Foolish Games|Socket", DisplayName="Type")
	EFSocketType GetType();

	/**
	 * @brief 类型
	 */
	virtual EFSocketType GetType_Implementation();

	/**
	 * @brief 消息偏移值
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket", DisplayName="Message Offset")
	const int& GetMessageOffset() const;

	/**
	 * @brief 消息偏移值
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket", DisplayName="Compression")
	UCompression* GetCompression() const;

	/**
	 * @brief 加密工具
	 */
	UFUNCTION(BlueprintPure, Category="Foolish Games|Socket", DisplayName="CryptoProvider")
	UCryptoProvider* GetCryptoProvider() const;

	/**
	 * @brief 关闭函数
	 */
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Foolish Games|Socket", DisplayName="CryptoProvider")
	void Close(EOpCode opCode = EOpCode::Close);
	/**
	 * @brief 关闭函数
	 */
	virtual void Close_Implementation(EOpCode opCode = EOpCode::Close);
};
