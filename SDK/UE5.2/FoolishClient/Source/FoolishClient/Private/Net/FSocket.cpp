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


#include "Net/FSocket.h"

#include "Sockets.h"
#include "Log/FConsole.h"
#include "Log/FoolishClientCategories.h"
#include "Net/UserToken.h"

bool UFSocket::GetIsRunning_Implementation()
{
	return FPlatformAtomics::AtomicRead(&bIsRunning) != 0;
}

bool UFSocket::GetConnected_Implementation()
{
	return Socket != nullptr && Socket->GetConnectionState() == SCS_Connected;
}

FInternetAddr& UFSocket::GetAddress() const
{
	return Address.ToSharedRef().Get();
}

FIPv4Endpoint UFSocket::GetIpAddress() const
{
	return FIPv4Endpoint(Address);
}

FString UFSocket::GetHost() const
{
	return GetIpAddress().Address.ToString();
}

int32 UFSocket::GetPort() const
{
	return GetIpAddress().Port;
}

FSocket* UFSocket::GetSocket() const
{
	return Socket;
}

EFSocketType UFSocket::GetType()
{
	return Tcp;
}

const int& UFSocket::GetMessageOffset() const
{
	return MessageOffset;
}

UCompression* UFSocket::GetCompression() const
{
	return Compression;
}

UCryptoProvider* UFSocket::GetCryptoProvider() const
{
	return CryptoProvider;
}

void UFSocket::Close_Implementation(EOpCode OpCode)
{
	FScopeLock SetLock(&Mutex);

	FPlatformAtomics::InterlockedExchange(&bIsRunning, 0);

	RemoveFromRoot();
	ClearGarbage();
	MarkAsGarbage();

	if (Socket != nullptr)
	{
		try
		{
			Socket->Shutdown(ESocketShutdownMode::ReadWrite);
			Socket->Close();
		}
		catch (...)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
			                                  TEXT("There are some errors happened on the socket close."));
		}
		//delete Socket; /** Socket 在创建时已经被加入UE的GC管理中参考：FSocketSubsystemEOS::CreateSocket **/
		ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->DestroySocket(Socket);
		Socket = nullptr;
	}
}

UUserToken* UFSocket::MakeEventArgs(UFSocket* InSocket)
{
	if (InSocket == nullptr)
	{
		return nullptr;
	}
	UUserToken* NewUserToken = NewObject<UUserToken>(InSocket);
	NewUserToken->SetOriginalOffset(0, 8192);
	NewUserToken->Socket = InSocket;
	return NewUserToken;
}
