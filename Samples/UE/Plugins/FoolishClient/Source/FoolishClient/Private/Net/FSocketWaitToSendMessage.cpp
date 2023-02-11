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


#include "Net/FSocketWaitToSendMessage.h"

#include "Log/FConsole.h"
#include "Log/FoolishClientCategories.h"
#include "Net/SocketSender.h"

void UFSocketWaitToSendMessage::Work_Implementation()
{
	//if (Sender != nullptr && Sender->Socket.IsValid() && Sender->Socket->GetConnected())
	if (Sender != nullptr && Sender->Socket != nullptr && Sender->Socket->GetConnected())
	{
		try
		{
			Sender->Post(Message);
		}
		catch (...)
		{
			UFConsole::WriteErrorWithCategory(UFoolishClientCategories::SOCKET,
			                                  TEXT("There are some exceptions happened on sending message"));
			Sender->Socket->Close();
		}
	}
	if (IsRooted())
	{
		RemoveFromRoot();
		ClearGarbage();
		MarkAsGarbage();
	}
}
