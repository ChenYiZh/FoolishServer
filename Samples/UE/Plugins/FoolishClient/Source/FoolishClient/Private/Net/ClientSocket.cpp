﻿/****************************************************************************
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


#include "Net/ClientSocket.h"

#include "Sockets.h"
#include "Common/FPackageFactory.h"
#include "IO/MessageWriter.h"

#include "Log/FConsole.h"
#include "Log/FoolishClientCategories.h"
#include "Net/SocketReceiver.h"
#include "Net/SocketSender.h"
#include "Proxy/ActionBoss.h"
#include "Proxy/MessageWorker.h"

FName UClientSocket::GetName()
{
	return Name;
}

bool UClientSocket::IsReady() const
{
	return ReadyFlag.GetValue() == 1;
}

int UClientSocket::GetHeartbeatInterval()
{
	return HeartbeatInterval;
}

void UClientSocket::SetMessageNumber(const int64 InMessageNumber)
{
	Sender->SetMessageNumber(InMessageNumber);
}

int64 UClientSocket::GetMessageNumber() const
{
	return Sender->GetMessageNumber();
}

void UClientSocket::SetActionProvider(UClientActionDispatcher* Provider)
{
	ActionProvider = Provider;
}

UClientActionDispatcher* UClientSocket::GetActionProvider() const
{
	return ActionProvider;
}

void UClientSocket::SetMessageContractor(UObject* InMessageContractor)
{
	if (InMessageContractor == nullptr)
	{
		return;
	}
	if (!InMessageContractor->Implements<UIBoss>())
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
		                                  TEXT("The type of InMessageContractor is not IBoss"));
		return;
	}
	MessageContractor = InMessageContractor;
}

UObject* UClientSocket::GetMessageContractor() const
{
	return MessageContractor;
}

void UClientSocket::SetMessageOffset_Implementation(const int32 Offset)
{
	MessageOffset = Offset;
}

void UClientSocket::SetCompression_Implementation(UCompression* InCompression)
{
	Compression = InCompression;
}

void UClientSocket::SetCryptoProvider_Implementation(UCryptoProvider* InCryptoProvider)
{
	CryptoProvider = InCryptoProvider;
}

void UClientSocket::Ready_Implementation(const FName& InName, const FString& Host, const int32& InPort,
                                         const TArray<FString>& ActionClassFullNames, const int32& InHeartbeatInterval)
{
	Name = InName;
	FIPv4Endpoint Endpoint;
	FString RemoteAddress = Host + TEXT(":") + FString::FromInt(InPort);
	if (!FIPv4Endpoint::Parse(RemoteAddress, Endpoint))
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
		                                  FString::Printf(
			                                  TEXT("The host: %s is not a correct address."), ToCStr(RemoteAddress)));
		return;
	}
	Address = MakeShared<FIPv4Endpoint>(Endpoint);
	ActionProvider = NewObject<UClientActionDispatcher>(this);
	ActionProvider->ActionNameFormats = ActionClassFullNames;
	HeartbeatInterval = InHeartbeatInterval;
	UFConsole::WriteInfoWithCategory(UFoolishClientCategories::SOCKET,TEXT("Socket is ready..."), false);
	ReadyFlag.Set(1);
	//FPlatformAtomics::InterlockedExchange(&ReadyFlag, 1);
}

void UClientSocket::AutoConnect_Implementation()
{
	if (!IsRunning)
	{
		FClientSocketConnectCallback Callback;
		ConnectAsync(Callback);
	}
}

void UClientSocket::ConnectAsync_Implementation(const FClientSocketConnectCallback& Callback)
{
	IsRunning = true;
	Awake();
	(new FAutoDeleteAsyncTask<FClientSocketConnectAsyncTask>(this, Callback))->StartBackgroundTask();
}

bool UClientSocket::Connect()
{
	if (!IsReady())
	{
		IsRunning = false;
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,TEXT("Socket is not ready!"));
		return false;
	}
	IsRunning = true;
	Awake();
	UFConsole::WriteInfoWithCategory(UFoolishClientCategories::SOCKET, TEXT("Socket is starting..."), false);

	try
	{
		bool bSuccess = Socket->Connect(Address->ToInternetAddr().Get());
		if (!bSuccess || !GetConnected())
		{
			IsRunning = false;
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, TEXT("Socket connect failed!"));
			Close();
			return false;
		}
	}
	catch (...)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, TEXT("Fail to connect the server!"));
		Close();
		return false;
	}

	if (HeartbeatTimer == nullptr)
	{
		RebuildHeartbeatPackage();

		// FTimerManager& TimerManager = GEngine->GetCurrentPlayWorld()->GetTimerManager();
		// FTimerHandle HeartbeatHandle;
		// TimerManager.SetTimer(HeartbeatHandle, this, &UClientSocket::SendHeartbeatPackage, HeartbeatInterval, true,
		//                       HeartbeatInterval);
		// HeartbeatTimer = MakeShared<FTimerHandle>(HeartbeatHandle);

		HeartbeatRunnable = new FClientSocketHeartbeatRunnable(this);
		HeartbeatTimer = FRunnableThread::Create(HeartbeatRunnable, ToCStr(Name.ToString()));
	}

	// TODO: 循环线程是否需要?

	if (Sender != nullptr)
	{
		Sender->BeginSend();
	}

	if (Receiver != nullptr)
	{
		Receiver->BeginReceive();
	}
	UFConsole::WriteInfoWithCategory(UFoolishClientCategories::SOCKET, TEXT("Socket connected."), false);
	return true;
}

void UClientSocket::Awake()
{
	if (Socket == nullptr)
	{
		Socket = MakeSocket();
	}
	if (Sender == nullptr)
	{
		Sender = NewObject<USocketSender>(this);
		Sender->Initialize(this);
	}
	if (Receiver == nullptr)
	{
		Receiver = NewObject<USocketReceiver>(this);
		Receiver->Initialize(this);
		Receiver->OnMessageReceived.AddDynamic(this, &UClientSocket::OnMessageReceived);
	}
}

FSocket* UClientSocket::MakeSocket()
{
	return nullptr;
}

void UClientSocket::SendHeartbeatPackage()
{
	try
	{
		if (Socket != nullptr && Socket->GetConnectionState() != SCS_Connected)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, TEXT("The socket is not connected."));
		}
		// 心跳包
		if (!HeartbeatBuffer.IsEmpty())
		{
			Sender->SendBytesImmediately(HeartbeatBuffer);
		}
	}
	catch (...)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, TEXT("Send heartbeat package failed."));
	}
}

void UClientSocket::BuildHeartbeatBuffer(TArray<uint8>& Buffer)
{
	UMessageWriter* Message = NewObject<UMessageWriter>(this);
	Message->SetOpCode(static_cast<int8>(EOpCode::Pong));
	return UFPackageFactory::Pack(Buffer, Message, MessageOffset, nullptr, nullptr);
}

void UClientSocket::RebuildHeartbeatPackage_Implementation()
{
	HeartbeatBuffer.Empty();
	BuildHeartbeatBuffer(HeartbeatBuffer);
}

void UClientSocket::Close_Implementation(EOpCode opCode)
{
	if (!IsRunning) { return; }
	IsRunning = false;
	FScopeLock LockStep1(&Mutex);
	if (Sender != nullptr)
	{
		UMessageWriter* Message = NewObject<UMessageWriter>(this);
		Message->SetOpCode(static_cast<int8>(EOpCode::Close));
		TArray<uint8> CloseMessage;
		UFPackageFactory::Pack(CloseMessage, Message, MessageOffset, nullptr, nullptr);
		try
		{
			Sender->Post(CloseMessage);
		}
		catch (...)
		{
		}
	}
	LockStep1.Unlock();
	Super::Close_Implementation(opCode);
	FScopeLock LockStep2(&Mutex);
	if (HeartbeatTimer != nullptr /*&& HeartbeatTimer.IsValid()*/)
	{
		try
		{
			// FTimerHandle& Handle = *HeartbeatTimer.Get();
			// if (Handle.IsValid())
			// {
			// 	FTimerManager& TimerManager = GEngine->GetCurrentPlayWorld()->GetTimerManager();
			// 	TimerManager.ClearTimer(Handle);
			// }
			HeartbeatTimer->Kill(true);
			delete HeartbeatRunnable;
			delete HeartbeatTimer;
			HeartbeatRunnable = nullptr;
		}
		catch (...)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,TEXT("Fail to dispose HeartbeatTimer."));
		}
		HeartbeatTimer = nullptr;
	}
	//管理类回收
	if (Sender != nullptr)
	{
		try
		{
			Sender->Release();
		}
		catch (...)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,TEXT("Fail to release sender."));
		}
		Sender = nullptr;
	}
	if (Receiver != nullptr)
	{
		try
		{
			Receiver->Release();
		}
		catch (...)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,TEXT("Fail to release receiver."));
		}
		Receiver = nullptr;
	}
	UFConsole::WriteInfoWithCategory(UFoolishClientCategories::SOCKET, TEXT("Socket Closed."), false);
}

void UClientSocket::Send(UMessageWriter* Message)
{
	AutoConnect();
	if (Sender != nullptr)
	{
		Sender->Send(Message);
	}
}

void UClientSocket::SendImmediately(UMessageWriter* Message)
{
	AutoConnect();
	if (Sender != nullptr)
	{
		Sender->SendImmediately(Message);
	}
}

void UClientSocket::SendBytes(const TArray<uint8>& Data)
{
	AutoConnect();
	if (Sender != nullptr)
	{
		Sender->SendBytes(Data);
	}
}

void UClientSocket::SendBytesImmediately(const TArray<uint8>& Data)
{
	AutoConnect();
	if (Sender != nullptr)
	{
		Sender->SendBytesImmediately(Data);
	}
}

uint32 FClientSocketHeartbeatRunnable::Run()
{
	while (IsRunning && Socket != nullptr && Socket->IsRunning)
	{
		Socket->SendHeartbeatPackage();
		FDateTime StartTime = FDateTime::Now();
		for (int i = 0; i < Socket->HeartbeatInterval; i++)
		{
			if (!IsRunning || (FDateTime::Now() - StartTime).GetTotalMilliseconds() > Socket->HeartbeatInterval)
			{
				break;
			}
			FPlatformProcess::Sleep(1.f / 1000.f); //毫秒转秒
			//每隔50毫秒检查一下数据接收
			if (i % 50 == 0)
			{
				Socket->Receiver->BeginReceive(true);
			}
		}
	}
	return 0;
}

void FClientSocketHeartbeatRunnable::Stop()
{
	IsRunning = false;
}

//
// void FClientSocketHeartbeatRunnable::Exit()
// {
// 	delete this;
// }

void UClientSocket::OnMessageReceived(UClientSocket* InSocket, UMessageReader* Message)
{
	try
	{
		if (MessageContractor != nullptr)
		{
			UMessageWorker* Worker = NewObject<UMessageWorker>(this);
			Worker->Message = Message;
			Worker->Socket = this;
			IIBoss::Execute_CheckIn(MessageContractor, Worker);
		}
		else
		{
			Message->AddToRoot();
			(new FAutoDeleteAsyncTask<FClientSocketMessageProcessor>(this, Message))->StartBackgroundTask();
		}
	}
	catch (...)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, TEXT("Process message error."));
	}
}

void UClientSocket::ProcessMessage(UMessageReader* Message)
{
	if (Message == nullptr)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
		                                  TEXT("ClientSocket receive empty message."));
		return;
	}
	try
	{
		ActionProvider->Provide(Message);
	}
	catch (...)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, TEXT("ActionProvider error."));
	}
}

void FClientSocketConnectAsyncTask::DoWork()
{
	if (Socket.IsValid())
	{
		bool bSuccess = Socket->Connect();
		Callback.ExecuteIfBound(bSuccess);
	}
}

TStatId FClientSocketConnectAsyncTask::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(FClientSocketConnectAsyncTask, STATGROUP_ThreadPoolAsyncTasks);
}

void FClientSocketMessageProcessor::DoWork()
{
	// if (Message != nullptr && Message->IsRooted())
	// {
	// 	Message->RemoveFromRoot();
	// 	Message->ClearGarbage();
	// 	Message->MarkAsGarbage();
	// }
	if (Socket.IsValid())
	{
		Socket->ProcessMessage(Message);
	}
}

TStatId FClientSocketMessageProcessor::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(FClientSocketMessageProcessor, STATGROUP_ThreadPoolAsyncTasks);
}
