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


#include "Net/SocketSender.h"

#include "Sockets.h"
#include "Common/FPackageFactory.h"
#include "Log/FConsole.h"
#include "Log/FoolishClientCategories.h"
#include "Net/FSocketWaitToSendMessage.h"

void USocketSender::SetMessageNumber(int64 InMessageNumber)
{
	//FPlatformAtomics::InterlockedExchange(&MessageNumber, InMessageNumber);
	MessageNumber.Set(InMessageNumber);
}

int64 USocketSender::GetMessageNumber()
{
	//return FPlatformAtomics::InterlockedIncrement(&MessageNumber);
	return MessageNumber.Increment();
}

bool USocketSender::GetIsSending()
{
	return IsSending;
}

bool USocketSender::bSendable()
{
	return IsSending = true;
}

void USocketSender::Initialize(UFSocket* FSocket)
{
	if (FSocket == nullptr)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
		                                  TEXT("Fail to create socket sender, because the FSocket is null."));
		return;
	}
	Socket = FSocket;
}

bool USocketSender::BeginSend()
{
	FScopeLock Lock(&Mutex);
	if (IsSending) { return false; }
	if (WaitToSendMessages.Num() > 0 && bSendable())
	{
		(new FAutoDeleteAsyncTask<FSocketSenderStartSendTask>(this))->StartBackgroundTask();
		return false;
	}
	return true;
}

void USocketSender::Send(UMessageWriter* Message)
{
	CheckIn(Message, false);
}

void USocketSender::SendImmediately(UMessageWriter* Message)
{
	CheckIn(Message, true);
}

void USocketSender::SendBytes(const TArray<uint8>& Data)
{
	CheckIn(Data, false);
}

void USocketSender::SendBytesImmediately(const TArray<uint8>& Data)
{
	CheckIn(Data, true);
}

void USocketSender::CheckIn(const TArray<uint8>& Data, const bool& IsImmediately)
{
	if (this == nullptr) { return; }
	UFSocketWaitToSendMessage* Worker = NewObject<UFSocketWaitToSendMessage>(this);
	Worker->AddToRoot();
	Worker->Message = Data;
	//Worker->MessageSize = DataSize;
	Worker->Sender = this;
	CheckIn(Worker, IsImmediately);
}

void USocketSender::CheckIn(UMessageWriter* Message, const bool& IsImmediately)
{
	if (!Socket.IsValid()) { return; }
	//if (Socket == nullptr) { return; }
	Message->SetMsgId(GetMessageNumber());
	//int32 DataSize;
	TArray<uint8> Data;
	UFPackageFactory::Pack(Data, Message, Socket->GetMessageOffset(), Socket->GetCompression(),
	                       Socket->GetCryptoProvider());

	//强制gc，防止无法退出
	Message->ClearGarbage();
	//Message->ClearFlags(EObjectFlags::RF_AllFlags);

	UFSocketWaitToSendMessage* Worker = NewObject<UFSocketWaitToSendMessage>(this);
	Worker->AddToRoot();
	Worker->Message = Data;
	//Worker->MessageSize = DataSize;
	Worker->Sender = this;
	CheckIn(Worker, IsImmediately);
}

void USocketSender::CheckIn(UFSocketWaitToSendMessage* Worker, const bool& IsImmediately)
{
	if (!Socket.IsValid()) { return; }
	//if (Socket == nullptr) { return; }
	FScopeLock SetLock(&Mutex);
	if (IsImmediately)
	{
		WaitToSendMessages.AddHead(Worker);
	}
	else
	{
		WaitToSendMessages.AddTail(Worker);
	}
	//未连接时返回
	if (!Socket->GetConnected())
	{
		return;
	}
	(new FAutoDeleteAsyncTask<FSocketSenderTask>(this))->StartBackgroundTask();
}

void USocketSender::Post(const TArray<uint8>& Data)
{
	if (Data.IsEmpty() || !Socket.IsValid())
	//if (Data.IsEmpty() || Socket == nullptr)
	{
		return;
	}
	int32 Size = Data.Num();

	FSocket* FSocket = Socket->GetSocket();
	int32 BytesSent;
	FScopeLock Lock(&Socket->Mutex);
	int32 NewSize;
	FSocket->SetSendBufferSize(Size, NewSize);
	FSocket->Send(Data.GetData(), Size, BytesSent);
	Lock.Unlock();
	SendCompleted();
}

void USocketSender::PostHeadMessage()
{
	FScopeLock SetLock(&Mutex);
	if (WaitToSendMessages.Num() > 0 && IsSending)
	{
		auto Node = WaitToSendMessages.GetHead();
		UWorker* Execution = Node->GetValue();
		WaitToSendMessages.RemoveNode(Node);
		Execution->Work();
	}
}

bool USocketSender::SendCompleted()
{
	if (bSendable())
	{
		FScopeLock SetLock(&Mutex);
		if (WaitToSendMessages.Num() == 0)
		{
			IsSending = false;
			return false;
		}
		PostHeadMessage();
		return true;
	}
	return false;
}

void USocketSender::Release()
{
	FScopeLock SetLock(&Mutex);
	ClearGarbage();
	MarkAsGarbage();
	for (UWorker* Worker : WaitToSendMessages)
	{
		UFSocketWaitToSendMessage* Message = Cast<UFSocketWaitToSendMessage>(Worker);
		if (Message != nullptr && Message->IsRooted())
		{
			Message->RemoveFromRoot();
			Message->ClearGarbage();
		}
	}
	WaitToSendMessages.Empty();
}

void FSocketSenderTask::DoWork()
{
	if (Sender.IsValid())
	{
		Sender->BeginSend();
	}
}

TStatId FSocketSenderTask::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(FSocketSenderTask, STATGROUP_ThreadPoolAsyncTasks);
}

void FSocketSenderStartSendTask::DoWork()
{
	if (Sender.IsValid())
	{
		Sender->PostHeadMessage();
	}
}

TStatId FSocketSenderStartSendTask::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(FSocketSenderStartSendTask, STATGROUP_ThreadPoolAsyncTasks);
}
