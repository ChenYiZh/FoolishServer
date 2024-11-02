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
#include "Proxy/Worker.h"
#include "UObject/Object.h"
#include "FSocketWaitToSendMessage.generated.h"

/**
 * 待发送的消息
 */
UCLASS(NotBlueprintable, NotBlueprintType, DisplayName="Wait Sending Message")
class FOOLISHCLIENT_API UFSocketWaitToSendMessage : public UWorker
{
	GENERATED_BODY()
	friend class USocketSender;
private:
	/**
	 * @brief 消息
	 */
	TArray<uint8> Message;
	/**
	 * @brief 消息长度
	 */
	//int32 MessageSize;
	/**
	 * @brief 发送接口的套接字
	 */
	UPROPERTY()
	USocketSender* Sender;
public:
	/**
	 * @brief 执行函数
	 */
	virtual void Work_Implementation() override;
};
