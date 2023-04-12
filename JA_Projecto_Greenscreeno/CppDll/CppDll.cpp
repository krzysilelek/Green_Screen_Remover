#include "pch.h"
#include "CppDll.h"

void processPicture(unsigned char* byteArray, unsigned int start, unsigned int end, float threshold)
{
	// byteArray - nasz obraz, start - pocz¹tek w tablicy do przetworzenia 
	// end - koniec w tablicy do przetworzenia, threshold - próg ustalony przez u¿ytkownika  
	for (int i = start; i < end; i += 4)
	{
		//wyliczenie iloczynu skalarnego dla piksela
		float greenValue = (float)byteArray[i] * (-0.5) + (float)byteArray[i + 1] + (float)byteArray[i + 2] * (-0.5);
		
		// porównanie wyniku iloczynu skalarnego z progiem podanym przez u¿ytkowanika
		// je¿eli wartoœæ iloczynu skalarnego jest wiêksza od wartoœci progu to kana³ alpha zostanie wyzerowany
		// w przeciwnym wypadku kana³ zostanie przepisany
		byteArray[i + 3] = (greenValue <= threshold) * byteArray[i + 3];

		
		/*	stare rozwi¹zanie
		unsigned char max = max(max(pixelArray[i], pixelArray[i + 1]), pixelArray[i + 2]);
		unsigned char min = min(min(pixelArray[i], pixelArray[i + 1]), pixelArray[i + 2]);

		if (pixelArray[i + 1] != min
			&& max - pixelArray[i + 1] <= amongBiggestValue
			&& (max - min) > differenceMaxMin)
		{
			pixelArray[i + 3] = 0;
		}
		*/
	}
}
