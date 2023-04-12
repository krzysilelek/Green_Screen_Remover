#include "pch.h"
#include "CppDll.h"

void processPicture(unsigned char* byteArray, unsigned int start, unsigned int end, float threshold)
{
	for (int i = start; i < end; i += 4)
	{
		float greenValue = (float)byteArray[i] * (-0.5) + (float)byteArray[i + 1] + (float)byteArray[i + 2] * (-0.5);
		byteArray[i + 3] = (greenValue <= threshold) * byteArray[i + 3];
	}
}
