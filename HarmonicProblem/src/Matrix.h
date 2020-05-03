#pragma once
#include "stdafx.h"

class Matrix
{
public:
	Matrix(int n) : N(n)
	{
		DI.resize(n, 0.0);
		IA.resize(n + 1, 0);
	}

	double& operator()(int i, int j)
	{
		if (i >= N || j >= N || i < 0 || j < 0)
			throw std::out_of_range("Bad index for matrix");

		if (i == j)
			return DI[i];

		if (j > i)
		{
			int width = IA[j + 1] - IA[j];

			int begin = j - width;
			int end = j;
			for (int k = begin; k < end; k++)
			{
				if (k == i)
				{
					return AU[IA[j] + k - begin];
				}
			}

		}
		else
		{
			int width = IA[i + 1] - IA[i];

			int begin = i - width;
			int end = i;
			for (int k = begin; k < end; k++)
			{
				if (k == j)
				{
					return AL[IA[i] + k - begin];
				}
			}
		}

		throw std::out_of_range("Bad index for matrix");
	}

	void Clear()
	{
		for (auto& di : DI)
			di = 0;

		for (auto& al : AL)
			al = 0;

		for (auto& au : AU)
			au = 0;
	}

	int N = 0;
	std::vector<double> DI, AL, AU;
	std::vector<int> IA, JA;
};