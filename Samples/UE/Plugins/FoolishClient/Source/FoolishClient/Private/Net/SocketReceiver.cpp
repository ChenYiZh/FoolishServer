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


#include "Net/SocketReceiver.h"

#include "Sockets.h"
#include "Common/ByteUtil.h"
#include "Common/FPackageFactory.h"
#include "Common/SizeUtil.h"
#include "Log/FConsole.h"
#include "Log/FoolishClientCategories.h"

UClientSocket* USocketReceiver::GetSocket()
{
	return Socket.Get();
}

bool USocketReceiver::GetIsReceiving()
{
	return IsReceiving;
}

bool USocketReceiver::Receivable()
{
	return IsReceiving = true;
}

void USocketReceiver::MessageReceived(UClientSocket* InSocket, UMessageReader* InMessage)
{
	OnMessageReceived.Broadcast(InSocket, InMessage);
}

void USocketReceiver::Ping(UClientSocket* InSocket, UMessageReader* InMessage)
{
	OnPing.Broadcast(InSocket, InMessage);
}

void USocketReceiver::Pong(UClientSocket* InSocket, UMessageReader* InMessage)
{
	OnPong.Broadcast(InSocket, InMessage);
}

void USocketReceiver::Initialize(UClientSocket* FSocket)
{
	if (FSocket == nullptr)
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
		                                  TEXT("Fail to create socket receiver, because the FSocket is null."));
		return;
	}
	Socket = FSocket;
}

bool USocketReceiver::BeginReceive(bool bForce)
{
	if (!Socket.IsValid() || !Socket->GetConnected())
	{
		UFConsole::WriteWarnWithCategory(UFoolishClientCategories::SOCKET,
		                                 "System.Net.Socket is abnormal, and display disconnected.");
		if (Socket.IsValid())
		{
			Socket->Close();
		}
		return false;
	}
	if (IsReceiving) { return false; }
	if (Receivable())
	{
		// uint32 PendingDataSize;
		// if (!bForce && !Socket->GetSocket()->HasPendingData(PendingDataSize))
		// {
		// 	IsReceiving = false;
		// 	return true;
		// }
		(new FAutoDeleteAsyncTask<FSocketReceiverTask>(this, bForce))->StartBackgroundTask();
		return false;
	}
	return false;
}

bool USocketReceiver::ProcessReceive(bool bForce)
{
	if (!Socket.IsValid() || Socket->GetSocket() == nullptr) { return false; }
	FSocket* FSocket = Socket->GetSocket();
	uint32 PendingDataSize;
	if (!bForce)
	{
		if (!FSocket->HasPendingData(PendingDataSize))
		{
			//Socket->Close(Empty);
			IsReceiving = false;
			return false;
		}
	}
	else
	{
		PendingDataSize = 1024;
	}
	TArray<uint8> Buffer;
	Buffer.SetNumUninitialized(PendingDataSize);
	int32 BytesRead;
	if (!FSocket->Recv(Buffer.GetData(), PendingDataSize, BytesRead))
	{
		UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
		                                  FString::Printf(TEXT("Process Receive IP %s SocketError:%s, bytes len:%d"),
		                                                  ToCStr(Socket->GetHost() + TEXT(":") + FString::FromInt(
			                                                  Socket->GetPort())), TEXT(""), BytesRead));
		Socket->Close();
		return false;
	}

	if (BytesRead > 0)
	{
		int32 ReceivedBufferLength = ReceivedBuffer.Num();

		TArray<uint8> ArgsBuffer = Buffer;
		int32 ArgsOffset = 0;
		int32 ArgsLength = BytesRead;

		int32 Offset = ArgsOffset;

		TArray<UMessageReader*> Messages;
		try
		{
			//继续接收上次未接收完毕的数据
			//if (ReceivedBuffer != nullptr)
			if (ReceivedBuffer.Num() > 0)
			{
				//上次连报头都没接收完
				if (ReceivedStartIndex < 0)
				{
					int32 DataLength = ArgsLength + ReceivedBufferLength;
					TArray<uint8> Data;
					Data.SetNum(DataLength);
					UByteUtil::BlockCopy(ReceivedBuffer, 0, Data, 0, ReceivedBufferLength);
					UByteUtil::BlockCopy(ArgsBuffer, ArgsOffset, Data, ReceivedBufferLength, ArgsLength);
					//ReceivedBuffer = nullptr;
					ReceivedBuffer.Empty();
					ArgsBuffer = Data;
					Offset = ArgsOffset = 0;
					ArgsLength = DataLength;
				}
				//数据仍然接收不完
				else if (ReceivedStartIndex + ArgsLength < ReceivedBufferLength)
				{
					UByteUtil::BlockCopy(ArgsBuffer, ArgsOffset, ReceivedBuffer, ReceivedStartIndex, ArgsLength);
					ReceivedStartIndex += ArgsLength;
					Offset += ArgsLength;
				}
				//这轮数据可以接受完
				else
				{
					int32 DeltaLength = ReceivedBufferLength - ReceivedStartIndex;
					UByteUtil::BlockCopy(ArgsBuffer, ArgsOffset, ReceivedBuffer, ReceivedStartIndex, DeltaLength);
					UMessageReader* BigMessage = UFPackageFactory::Unpack(
						ReceivedBuffer, Socket->GetMessageOffset(), Socket->GetCompression(),
						Socket->GetCryptoProvider());
					//ReceivedBuffer = nullptr;
					ReceivedBuffer.Empty();
					Messages.Push(BigMessage);
					Offset += DeltaLength;
				}
			}

			//针对接收到的数据进行完整解析
			while (Offset - ArgsOffset < ArgsLength)
			{
				int32 TotalLength = UFPackageFactory::GetTotalLength(
					ArgsBuffer, Offset + Socket->GetMessageOffset());
				//包头解析不全
				if (TotalLength < 0)
				{
					ReceivedStartIndex = -1;
					ReceivedBufferLength = ArgsLength + ArgsOffset - Offset;
					TArray<uint8> Array;
					Array.SetNumUninitialized(ReceivedBufferLength);
					UByteUtil::BlockCopy(ArgsBuffer, Offset, Array, 0, ReceivedBufferLength);
					ReceivedBuffer = Array;
					break;
				}

				//包体解析不全
				if (TotalLength > ArgsLength)
				{
					ReceivedStartIndex = ArgsLength + ArgsOffset - Offset;
					ReceivedBufferLength = TotalLength - Offset;
					TArray<uint8> Array;
					Array.SetNumUninitialized(ReceivedBufferLength);
					UByteUtil::BlockCopy(ArgsBuffer, Offset, Array, 0, ReceivedBufferLength);
					ReceivedBuffer = Array;
					break;
				}

				Offset += Socket->GetMessageOffset();
				UMessageReader* Message = UFPackageFactory::Unpack(ArgsBuffer, Offset, Socket->GetCompression(),
				                                                   Socket->GetCryptoProvider());
				Messages.Push(Message);
				Offset = TotalLength;
			}
		}
		catch (...)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,TEXT("Process Receive error."));
		}

		for (int i = 0; i < Messages.Num(); i++)
		{
			UMessageReader* Message = Messages[i];
			try
			{
				if (Message->IsError())
				{
					UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET, Message->GetError());
					continue;
				}

				switch (Message->GetOpCode())
				{
				case static_cast<int8>(EOpCode::Close):
					{
						Socket->Close(EOpCode::Empty);
					}
					break;
				case static_cast<int8>(EOpCode::Ping):
					{
						Ping(Socket.Get(), Message);
					}
					break;
				case static_cast<int8>(EOpCode::Pong):
					{
						Pong(Socket.Get(), Message);
					}
					break;
				default:
					{
						MessageReceived(Socket.Get(), Message);
					}
					break;
				}
			}
			catch (...)
			{
				UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
				                                  TEXT("An exception occurred when resolve the message."));
			}
		}
	}
	//else if (ReceivedBuffer != nullptr)
	else if (!ReceivedBuffer.IsEmpty())
	{
		//数据错乱
		//ReceivedBuffer = nullptr;
		ReceivedBuffer.Empty();
		return false;
	}

	if (!Socket.IsValid() || Socket->GetSocket() == nullptr || !Socket->GetSocket()->HasPendingData(PendingDataSize))
	{
		IsReceiving = false;
		return false;
	}

	if (Socket.IsValid() && Socket->GetIsRunning())
	{
		//(new FAutoDeleteAsyncTask<FSocketReceiverTask>(this, false))->StartBackgroundTask();
		//return false;
		return ProcessReceive(false);
	}
	//延迟关闭
	if (!Socket.IsValid() || !Socket->GetIsRunning())
	{
		if (Socket.IsValid())
		{
			Socket->Close();
		}
	}
	return true;
}

void USocketReceiver::Release()
{
	//FScopeLock SetLock(&Mutex);
	ClearGarbage();
	MarkAsGarbage();
}

void USocketReceiver::ReceiveTask(bool bForce)
{
	if (!Socket.IsValid()) { return; }
	try
	{
		uint32 PendingDataSize;
		if (Receivable() && (Socket->GetSocket()->HasPendingData(PendingDataSize) || bForce))
		{
			ProcessReceive(PendingDataSize == 0);
		}
		else
		{
			//(new FAutoDeleteAsyncTask<FSocketReceiverTask>(this, bForce))->StartBackgroundTask();
			IsReceiving = false;
		}
	}
	catch (...)
	{
		if (Socket.IsValid())
		{
			Socket->Close();
		}
	}
}

void FSocketReceiverTask::DoWork()
{
	if (Receiver.IsValid())
	{
		Receiver->ReceiveTask(bForce);
	}
}

TStatId FSocketReceiverTask::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(FSocketReceiverTask, STATGROUP_ThreadPoolAsyncTasks);
}
