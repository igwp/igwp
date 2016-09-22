// ClrInjectionLib.h

#pragma once

using namespace System;

namespace ClrInjectionLib {

	public ref class Injector
	{
	public:
	    static bool Inject32(int processId, System::String^ dllName);
	};
}
