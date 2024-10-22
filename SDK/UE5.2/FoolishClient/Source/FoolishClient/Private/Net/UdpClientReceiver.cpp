// Fill out your copyright notice in the Description page of Project Settings.


#include "Net/UdpClientReceiver.h"

#include "Sockets.h"
#include "Log/FConsole.h"

void UUdpClientReceiver::PostReceive(UFSocket* InSocket)
{
	if (!GetSocket()->TryReceive())
	{
		return;
	}
	uint32 PendingSize;
	bool bHasData = GetSocket()->GetSocket()->HasPendingData(PendingSize);
	int32 DataSize;
	if (bHasData && GetSocket()->GetSocket()->RecvFrom(BufferReceived.GetData(), BufferSize, DataSize,
	                                                   InSocket->GetAddress()))
	{
		ProcessReceive(InSocket, BufferReceived, DataSize);
	}
	else
	{
		ProcessReceive(InSocket, BufferReceived, 0);
	}
}
