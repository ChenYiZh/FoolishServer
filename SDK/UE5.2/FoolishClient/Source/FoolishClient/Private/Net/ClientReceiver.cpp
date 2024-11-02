// Fill out your copyright notice in the Description page of Project Settings.


#include "Net/ClientReceiver.h"

void UClientReceiver::Close(UFSocket* InSocket, EOpCode OpCode)
{
	if (InSocket != nullptr)
	{
		InSocket->Close(OpCode);
	}
}
